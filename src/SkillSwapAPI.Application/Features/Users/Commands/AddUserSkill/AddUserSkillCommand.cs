using MediatR;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;

public sealed record AddUserSkillCommand(
    Guid UserId,
    Guid SkillId,
    SkillType Type,
    ProficiencyLevel ProficiencyLevel,
    int? YearsOfExperience) : IRequest<Result<UserSkillDto>>;
