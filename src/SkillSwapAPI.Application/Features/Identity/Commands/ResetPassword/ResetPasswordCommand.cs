using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Otp,
    string NewPassword,
    string ConfirmPassword
) : IRequest<Result<Success>>;
