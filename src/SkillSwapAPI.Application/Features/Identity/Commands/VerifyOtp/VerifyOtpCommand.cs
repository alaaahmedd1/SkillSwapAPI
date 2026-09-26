using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.VerifyOtp;

public sealed record VerifyOtpCommand(string Email, string Otp) : IRequest<Result<Success>>;