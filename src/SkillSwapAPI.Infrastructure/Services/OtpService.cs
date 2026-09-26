using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Settings;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Infrastructure.Identity;
using System.Security.Cryptography;

namespace SkillSwapAPI.Infrastructure.Services;

public sealed class OtpService(
    UserManager<AppUser> userManager,
    IEmailService emailService,
    IEmailTempService templateService,
    IOptions<OtpSettings> options,
    ILogger<OtpService> logger)
    : IOtpService
{
    private readonly OtpSettings _settings = options.Value;

    public async Task<Result<Success>> SendEmailConfirmationAsync(
        string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return Result.Success;

        if (user.EmailConfirmed)
            return ApplicationErrors.Otp.AlreadyVerified;

        var otp = GenerateOtp();
        var htmlbody = templateService.GetOtpTemplate(otp);

        await userManager.SetAuthenticationTokenAsync(user, "Email", "EmailConfirmation", otp);

        await emailService.SendAsync(
            to: email,
            subject: "SkillSwapAPI — Email Confirmation Code",
            htmlBody: htmlbody);

        logger.LogInformation("Confirmation OTP sent to {Email}", email);
        return Result.Success;
    }

    public async Task<Result<Success>> VerifyEmailConfirmationAsync(
        string email, string otp, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return ApplicationErrors.Otp.Invalid;

        if (user.EmailConfirmed) return ApplicationErrors.Otp.AlreadyVerified;

        var stored = await userManager.GetAuthenticationTokenAsync(
            user, "Email", "EmailConfirmation");

        if (stored is null || stored != otp)
            return ApplicationErrors.Otp.Invalid;

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
        await userManager.RemoveAuthenticationTokenAsync(user, "Email", "EmailConfirmation");

        logger.LogInformation("Email confirmed: {Email}", email);
        return Result.Success;
    }

    public async Task<bool> IsEmailConfirmedAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user?.EmailConfirmed ?? false;
    }

    public async Task<Result<Success>> SendPasswordResetAsync(
        string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Success;

        var otp = GenerateOtp();
        var htmlbody = templateService.GetOtpTemplate(otp);

        await userManager.SetAuthenticationTokenAsync(user, "Email", "PasswordReset", otp);

        await emailService.SendAsync(
            to: email,
            subject: "SkillSwapAPI — Password Reset Code",
            htmlBody: htmlbody);

        logger.LogInformation("Reset OTP sent to {Email}", email);
        return Result.Success;
    }

    public async Task<Result<Success>> VerifyAndResetPasswordAsync(
        string email, string otp, string newPassword, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return ApplicationErrors.Otp.Invalid;

        var stored = await userManager.GetAuthenticationTokenAsync(
            user, "Email", "PasswordReset");

        if (stored is null || stored != otp)
            return ApplicationErrors.Otp.Invalid;

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            var err = result.Errors.First();
            return Error.Validation(err.Code, err.Description);
        }

        await userManager.RemoveAuthenticationTokenAsync(user, "Email", "PasswordReset");

        logger.LogInformation("Password reset for {Email}", email);
        return Result.Success;
    }

    private string GenerateOtp()
    {
        int length = _settings.OtpLength > 0 ? _settings.OtpLength : 6;
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var number = BitConverter.ToUInt32(bytes, 0) % (uint)Math.Pow(10, length);
        return number.ToString($"D{length}");
    }
}