using MediatR;
using SkillSwapAPI.Application.Common.Settings;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ResendOtp;

public sealed record ResendOtpCommand(string Email) : IRequest<Result<Success>>;