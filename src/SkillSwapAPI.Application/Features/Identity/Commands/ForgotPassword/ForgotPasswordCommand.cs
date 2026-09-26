using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest<Result<Success>>;
