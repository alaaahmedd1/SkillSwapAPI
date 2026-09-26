using MediatR;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Skills.Queries.GetSkillCatalog;

public sealed record GetSkillCatalogQuery : IRequest<Result<List<SkillCatalogItemDto>>>;
