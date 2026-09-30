using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Features.Identity.Commands.ResendOtp;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public class ResendOtpCommandHandlerTests
{
    private readonly IOtpService _otpService = Substitute.For<IOtpService>();

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenConfirmationOtpIsSent()
    {
        var command = new ResendOtpCommand("user@test.local");

        _otpService
            .SendEmailConfirmationAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromSuccess(Result.Success));

        var handler = new ResendOtpCommandHandler(_otpService);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        await _otpService.Received(1).SendEmailConfirmationAsync("user@test.local", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnTooManyRequests_WhenRateLimitIsHit()
    {
        var command = new ResendOtpCommand("user@test.local");

        _otpService
            .SendEmailConfirmationAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromError(ApplicationErrors.Otp.TooManyRequests));

        var handler = new ResendOtpCommandHandler(_otpService);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Otp.TooManyRequests", result.TopError.Code);
        Assert.Equal(ErrorKind.Failure, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnSendFailed_WhenOtpSendFails()
    {
        var command = new ResendOtpCommand("user@test.local");

        _otpService
            .SendEmailConfirmationAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromError(ApplicationErrors.Otp.SendFailed));

        var handler = new ResendOtpCommandHandler(_otpService);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Otp.SendFailed", result.TopError.Code);
        Assert.Equal(ErrorKind.Unexpected, result.TopError.Type);
    }
}
