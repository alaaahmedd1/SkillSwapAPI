using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Features.Identity.Commands.ForgotPassword;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public class ForgotPasswordCommandHandlerTests
{
    private readonly IOtpService _otpService = Substitute.For<IOtpService>();
    private readonly ILogger<ForgotPasswordCommandHandler> _logger = Substitute.For<ILogger<ForgotPasswordCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldReturnSuccess_AndSendResetOtp_WhenEmailProvided()
    {
        var command = new ForgotPasswordCommand("user@test.local");

        _otpService
            .SendPasswordResetAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromSuccess(Result.Success));

        var handler = new ForgotPasswordCommandHandler(_otpService, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        await _otpService.Received(1).SendPasswordResetAsync("user@test.local", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_EvenWhenOtpSendFails()
    {
        var command = new ForgotPasswordCommand("user@test.local");

        _otpService
            .SendPasswordResetAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromError(ApplicationErrors.Otp.SendFailed));

        var handler = new ForgotPasswordCommandHandler(_otpService, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        await _otpService.Received(1).SendPasswordResetAsync("user@test.local", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_AndReturnSuccess_WhenOtpServiceIsNotConfigured()
    {
        var command = new ForgotPasswordCommand("unknown@test.local");

        var handler = new ForgotPasswordCommandHandler(_otpService, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        await _otpService.Received(1).SendPasswordResetAsync("unknown@test.local", Arg.Any<CancellationToken>());
    }
}
