using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Features.Identity.Commands.Register;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public class RegisterCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IOtpService _otpService = Substitute.For<IOtpService>();
    private readonly ILogger<RegisterCommandHandler> _logger = Substitute.For<ILogger<RegisterCommandHandler>>();

    private RegisterCommandHandler CreateHandler() => new(_identityService, _otpService, _logger);

    [Fact]
    public async Task Handle_ShouldReturnCreatedUser_WhenRegistrationSucceeds()
    {
        var user = TestData.AppUser(email: "new-user@test.local", firstName: "Grace", lastName: "Hopper");
        var command = new RegisterCommand("Grace", "Hopper", "new-user@test.local", "SecurePassword123!");

        _identityService
            .CreateAsync(command.FirstName, command.LastName, command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromSuccess(user));
        _otpService
            .SendEmailConfirmationAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromSuccess(Result.Success));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(user.UserId, result.Value.UserId);
        Assert.Equal("new-user@test.local", result.Value.Email);
        Assert.Equal("Grace", result.Value.FirstName);
        Assert.Equal("Hopper", result.Value.LastName);
        Assert.Null(result.Value.Token);
        await _identityService.Received(1).CreateAsync(
            "Grace", "Hopper", "new-user@test.local", "SecurePassword123!", Arg.Any<CancellationToken>());
        await _otpService.Received(1).SendEmailConfirmationAsync("new-user@test.local", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnDuplicateEmail_WhenEmailAlreadyExists()
    {
        var command = new RegisterCommand("Grace", "Hopper", "existing@test.local", "SecurePassword123!");

        _identityService
            .CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromError(ApplicationErrors.Auth.EmailAlreadyExists));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Auth.EmailAlreadyExists", result.TopError.Code);
        Assert.Equal(ErrorKind.Conflict, result.TopError.Type);
        await _otpService.DidNotReceive().SendEmailConfirmationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldStillReturnCreatedUser_WhenOtpSendFails()
    {
        var user = TestData.AppUser(email: "new-user@test.local");
        var command = new RegisterCommand("Grace", "Hopper", "new-user@test.local", "SecurePassword123!");

        _identityService
            .CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromSuccess(user));
        _otpService
            .SendEmailConfirmationAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromError(ApplicationErrors.Otp.SendFailed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(user.UserId, result.Value.UserId);
        await _otpService.Received(1).SendEmailConfirmationAsync("new-user@test.local", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPropagateError_WhenUserCreationFailsWithUnexpectedError()
    {
        var command = new RegisterCommand("Grace", "Hopper", "new-user@test.local", "SecurePassword123!");

        _identityService
            .CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<AppUserDto>.FromError(ApplicationErrors.Token.GenerationFailed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Token.GenerationFailed", result.TopError.Code);
        Assert.Equal(ErrorKind.Unexpected, result.TopError.Type);
        await _otpService.DidNotReceiveWithAnyArgs().SendEmailConfirmationAsync(default!, default);
    }
}
