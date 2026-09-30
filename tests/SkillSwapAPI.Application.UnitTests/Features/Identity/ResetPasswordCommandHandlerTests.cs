using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Features.Identity.Commands.ResetPassword;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public class ResetPasswordCommandHandlerTests
{
    private readonly IOtpService _otpService = Substitute.For<IOtpService>();
    private readonly ILogger<ResetPasswordCommandHandler> _logger = Substitute.For<ILogger<ResetPasswordCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPasswordResetSucceeds()
    {
        var command = new ResetPasswordCommand("user@test.local", "123456", "NewPassword1!", "NewPassword1!");

        _otpService
            .VerifyAndResetPasswordAsync(command.Email, command.Otp, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromSuccess(Result.Success));

        var handler = new ResetPasswordCommandHandler(_otpService, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        await _otpService.Received(1).VerifyAndResetPasswordAsync(
            "user@test.local", "123456", "NewPassword1!", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidOtp_WhenOtpVerificationFails()
    {
        var command = new ResetPasswordCommand("user@test.local", "000000", "NewPassword1!", "NewPassword1!");

        _otpService
            .VerifyAndResetPasswordAsync(command.Email, command.Otp, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromError(ApplicationErrors.Otp.Invalid));

        var handler = new ResetPasswordCommandHandler(_otpService, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Otp.Invalid", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldPropagateServiceError_WhenPasswordResetIsRejected()
    {
        var command = new ResetPasswordCommand("user@test.local", "123456", "NewPassword1!", "NewPassword1!");

        _otpService
            .VerifyAndResetPasswordAsync(command.Email, command.Otp, command.NewPassword, Arg.Any<CancellationToken>())
            .Returns(Result<Success>.FromError(ApplicationErrors.Otp.TooManyRequests));

        var handler = new ResetPasswordCommandHandler(_otpService, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Otp.TooManyRequests", result.TopError.Code);
        Assert.Equal(ErrorKind.Failure, result.TopError.Type);
    }
}
