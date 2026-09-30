using NSubstitute;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Identity;
using SkillSwapAPI.Application.Features.Identity.Commands.Login;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenProvider _tokenProvider = Substitute.For<ITokenProvider>();
    private readonly ILogger<LoginCommandHandler> _logger = Substitute.For<ILogger<LoginCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldReturnEmailNotVerified_WhenUserEmailIsUnconfirmed()
    {
        // Arrange
        var command = new LoginCommand("unverified@example.com", "Password123!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SkillSwapAPI.Application.Features.Identity.Dtos.AppUserDto>>(ApplicationErrors.Auth.EmailNotVerified));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Auth.EmailNotVerified", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnTokenPairAndUser_WhenAuthenticationSucceeds()
    {
        // Arrange
        var user = TestData.AppUser(email: "user@test.local", firstName: "Ada", lastName: "Lovelace");
        var expiresOnUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var tokenResponse = new TokenResponse("access-token", "refresh-token", expiresOnUtc);
        var command = new LoginCommand(user.Email, "Password123!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AppUserDto>.FromSuccess(user)));

        _tokenProvider
            .GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TokenResponse>.FromSuccess(tokenResponse)));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        Assert.Equal(expiresOnUtc, result.Value.ExpiresOnUtc);
        Assert.Equal(user.UserId, result.Value.User.UserId);
        Assert.Equal(user.Email, result.Value.User.Email);
        Assert.Equal(user.FirstName, result.Value.User.FirstName);
        Assert.Equal(user.LastName, result.Value.User.LastName);
        Assert.Null(result.Value.User.Token);
        await _tokenProvider.Received(1).GenerateJwtTokenAsync(
            Arg.Is<AppUserDto>(u => u.UserId == user.UserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidCredentials_WhenAuthenticationFails()
    {
        // Arrange
        var command = new LoginCommand("user@test.local", "WrongPassword1!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AppUserDto>.FromError(ApplicationErrors.Auth.InvalidCredentials)));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Auth.InvalidCredentials", result.TopError.Code);
        Assert.Equal(ErrorKind.Unauthorized, result.TopError.Type);
        await _tokenProvider.DidNotReceive().GenerateJwtTokenAsync(
            Arg.Any<AppUserDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnAccountLockedOut_WhenAccountIsLocked()
    {
        // Arrange
        var command = new LoginCommand("locked@test.local", "Password123!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AppUserDto>.FromError(ApplicationErrors.Auth.AccountLockedOut)));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Auth.AccountLockedOut", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _tokenProvider.DidNotReceive().GenerateJwtTokenAsync(
            Arg.Any<AppUserDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnAccountSuspended_WhenAccountIsSuspended()
    {
        // Arrange
        var command = new LoginCommand("suspended@test.local", "Password123!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AppUserDto>.FromError(ApplicationErrors.Auth.AccountSuspended)));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Auth.AccountSuspended", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _tokenProvider.DidNotReceive().GenerateJwtTokenAsync(
            Arg.Any<AppUserDto>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnTokenGenerationFailed_WhenTokenProviderFails()
    {
        // Arrange
        var user = TestData.AppUser(email: "user@test.local");
        var command = new LoginCommand(user.Email, "Password123!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<AppUserDto>.FromSuccess(user)));

        _tokenProvider
            .GenerateJwtTokenAsync(Arg.Any<AppUserDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<TokenResponse>.FromError(ApplicationErrors.Token.GenerationFailed)));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Token.GenerationFailed", result.TopError.Code);
        Assert.Equal(ErrorKind.Unexpected, result.TopError.Type);
        await _tokenProvider.Received(1).GenerateJwtTokenAsync(
            Arg.Is<AppUserDto>(u => u.UserId == user.UserId),
            Arg.Any<CancellationToken>());
    }
}
