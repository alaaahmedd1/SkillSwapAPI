using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Skills.Queries.GetSkillCatalog;

public sealed class GetSkillCatalogQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSkillCatalogQuery, Result<List<SkillCatalogItemDto>>>
{
    public async Task<Result<List<SkillCatalogItemDto>>> Handle(GetSkillCatalogQuery query, CancellationToken ct)
    {
        var categories = await unitOfWork.SkillCategories.GetActiveWithSkillsAsync(ct);

        return categories
            .Select(category => new SkillCatalogItemDto(
                category.Id,
                category.Name,
                category.Description,
                category.Skills
                    .OrderBy(skill => skill.Name)
                    .Select(skill => new SkillDto(skill.Id, skill.Name, skill.Description))
                    .ToList()))
            .ToList();
    }
}
