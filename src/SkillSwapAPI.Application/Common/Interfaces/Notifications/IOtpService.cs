using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Common.Interfaces.Notifications;

public interface IOtpService
{
    Task<Result<Success>> SendEmailConfirmationAsync(string email, CancellationToken ct = default);

    Task<Result<Success>> VerifyEmailConfirmationAsync(string email, string otp, CancellationToken ct = default);

    Task<bool> IsEmailConfirmedAsync(string email, CancellationToken ct = default);

    Task<Result<Success>> SendPasswordResetAsync(string email, CancellationToken ct = default);

    Task<Result<Success>> VerifyAndResetPasswordAsync(string email, string otp, string newPassword, CancellationToken ct = default);
}