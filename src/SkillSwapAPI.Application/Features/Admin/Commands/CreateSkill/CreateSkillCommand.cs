using MediatR;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Admin.Commands.CreateSkill;

public sealed record CreateSkillCommand(
    Guid AdminId,
    int CategoryId,
    string Name,
    string? Description) : IRequest<Result<SkillDto>>;
