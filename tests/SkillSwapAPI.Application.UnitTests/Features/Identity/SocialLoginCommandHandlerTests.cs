using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Identity;
using SkillSwapAPI.Application.Features.Identity.Commands.SocialLogin;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public class SocialLoginCommandHandlerTests
{
    private readonly ISocialAuthService _socialAuthService = Substitute.For<ISocialAuthService>();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenProvider _tokenProvider = Substitute.For<ITokenProvider>();
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly ILogger<SocialLoginCommandHandler> _logger = Substitute.For<ILogger<SocialLoginCommandHandler>>();

    private SocialLoginCommandHandler CreateHandler() =>
        new(_socialAuthService, _identityService, _tokenProvider, _fixture.UnitOfWork, _logger);

    [Fact]
    public async Task Handle_ShouldReturnLoginResponse_WhenSocialLoginSucceeds()
    {
        var command = new SocialLoginCommand("social-id-token", SocialProvider.Google);
        var socialUser = new SocialUserInfo("google-123", "social@test.local", "Social User", "Google");
        var appUser = TestData.AppUser(email: "social@test.local", firstName: "Social", lastName: "User");
        var expiresOnUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tokenResponse = new TokenResponse("access-token", "refresh-token", expiresOnUtc);

        _socialAuthService
            .VerifyTokenAsync(command.IdToken, command.Provider, Arg.Any<CancellationToken>())
            .Returns(Result<SocialUserInfo>.FromSuccess(socialUser));
        _identityService
            .FindOrCreateSocialUserAsync(socialUser, Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromSuccess(appUser));
        _tokenProvider
            .GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<TokenResponse>.FromSuccess(tokenResponse));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        Assert.Equal(expiresOnUtc, result.Value.ExpiresOnUtc);
        Assert.Equal(appUser.UserId, result.Value.User.UserId);
        Assert.Equal("social@test.local", result.Value.User.Email);
        Assert.Equal("Social", result.Value.User.FirstName);
        Assert.Equal("User", result.Value.User.LastName);
        Assert.Null(result.Value.User.Token);
        await _socialAuthService.Received(1).VerifyTokenAsync("social-id-token", SocialProvider.Google, Arg.Any<CancellationToken>());
        await _identityService.Received(1).FindOrCreateSocialUserAsync(socialUser, Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await _tokenProvider.Received(1).GenerateJwtTokenAsync(
            Arg.Is<AppUserDto>(u => u.UserId == appUser.UserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidToken_WhenSocialTokenVerificationFails()
    {
        var command = new SocialLoginCommand("invalid-token", SocialProvider.Google);

        _socialAuthService
            .VerifyTokenAsync(command.IdToken, command.Provider, Arg.Any<CancellationToken>())
            .Returns(Result<SocialUserInfo>.FromError(ApplicationErrors.SocialAuth.InvalidToken));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SocialAuth.InvalidToken", result.TopError.Code);
        Assert.Equal(ErrorKind.Unauthorized, result.TopError.Type);
        await _identityService.DidNotReceive().FindOrCreateSocialUserAsync(Arg.Any<SocialUserInfo>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceiveWithAnyArgs().CompleteAsync(default);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenSocialUserCreationFails()
    {
        var command = new SocialLoginCommand("social-id-token", SocialProvider.Facebook);
        var socialUser = new SocialUserInfo("facebook-123", "social@test.local", "Social User", "Facebook");

        _socialAuthService
            .VerifyTokenAsync(command.IdToken, command.Provider, Arg.Any<CancellationToken>())
            .Returns(Result<SocialUserInfo>.FromSuccess(socialUser));
        _identityService
            .FindOrCreateSocialUserAsync(socialUser, Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromError(ApplicationErrors.Auth.EmailAlreadyExists));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Auth.EmailAlreadyExists", result.TopError.Code);
        Assert.Equal(ErrorKind.Conflict, result.TopError.Type);
        await _fixture.UnitOfWork.DidNotReceiveWithAnyArgs().CompleteAsync(default);
        await _tokenProvider.DidNotReceive().GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnTokenGenerationFailed_WhenTokenProviderFails()
    {
        var command = new SocialLoginCommand("social-id-token", SocialProvider.Google);
        var socialUser = new SocialUserInfo("google-123", "social@test.local", "Social User", "Google");
        var appUser = TestData.AppUser(email: "social@test.local");

        _socialAuthService
            .VerifyTokenAsync(command.IdToken, command.Provider, Arg.Any<CancellationToken>())
            .Returns(Result<SocialUserInfo>.FromSuccess(socialUser));
        _identityService
            .FindOrCreateSocialUserAsync(socialUser, Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromSuccess(appUser));
        _tokenProvider
            .GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
            .Returns(Result<TokenResponse>.FromError(ApplicationErrors.Token.GenerationFailed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Token.GenerationFailed", result.TopError.Code);
        Assert.Equal(ErrorKind.Unexpected, result.TopError.Type);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }
}
