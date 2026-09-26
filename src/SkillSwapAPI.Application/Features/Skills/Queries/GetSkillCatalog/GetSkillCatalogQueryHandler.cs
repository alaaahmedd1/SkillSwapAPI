using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Skills.Queries.GetSkillCatalog;

public sealed class GetSkillCatalogQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSkillCatalogQuery, Result<List<SkillCatalogItemDto>>>
{
    public async Task<Result<List<SkillCatalogItemDto>>> Handle(GetSkillCatalogQuery query, CancellationToken ct)
    {
        var categories = await context.SkillCategories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new SkillCatalogItemDto(
                category.Id,
                category.Name,
                category.Description,
                category.Skills
                    .OrderBy(skill => skill.Name)
                    .Select(skill => new SkillDto(skill.Id, skill.Name, skill.Description))
                    .ToList()))
            .ToListAsync(ct);

        return categories;
    }
}
