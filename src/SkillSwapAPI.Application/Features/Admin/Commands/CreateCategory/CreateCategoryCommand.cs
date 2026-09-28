using MediatR;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Admin.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    Guid AdminId,
    string Name,
    string? Description) : IRequest<Result<SkillCategoryDto>>;
