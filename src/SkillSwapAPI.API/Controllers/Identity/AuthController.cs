using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Identity.Commands.ForgotPassword;
using SkillSwapAPI.Application.Features.Identity.Commands.Login;
using SkillSwapAPI.Application.Features.Identity.Commands.Logout;
using SkillSwapAPI.Application.Features.Identity.Commands.Register;
using SkillSwapAPI.Application.Features.Identity.Commands.ResetPassword;
using SkillSwapAPI.Application.Features.Identity.Commands.SocialLogin;
using SkillSwapAPI.Application.Features.Identity.Commands.VerifyOtp;
using SkillSwapAPI.Application.Features.Identity.Queries.RefreshTokens;
using System.Security.Claims;

namespace SkillSwapAPI.API.Controllers.Identity;

[Route("api/[controller]")]
public class AuthController : ApiBaseController
{
    /// <summary>
    /// UC-01: Register New Account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-02: Authenticate & Login
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-01: Social Auth Login (Google, Facebook, Apple)
    /// </summary>
    [HttpPost("social-login")]
    [AllowAnonymous]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-02: Refresh Access Token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-03: Request Password Reset OTP
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-04: Verify OTP Code
    /// </summary>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-05: Reset Password
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }

    /// <summary>
    /// UC-07: Logout & Token Revocation
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { code = "Auth.Unauthorized", message = "User identity claim missing." });
        }

        var command = new LogoutCommand(request.RefreshToken, userId);
        var result = await Mediator.Send(command, ct);
        return HandleResult(result);
    }
}

public sealed record LogoutRequest(string RefreshToken);
