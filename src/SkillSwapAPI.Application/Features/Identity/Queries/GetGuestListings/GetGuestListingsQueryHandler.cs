using MediatR;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;

public sealed class GetGuestListingsQueryHandler(
    IApplicationDbContext context)
    : IRequestHandler<GetGuestListingsQuery, Result<PagedResult<PublicListingDto>>>
{
    public Task<Result<PagedResult<PublicListingDto>>> Handle(
        GetGuestListingsQuery query,
        CancellationToken ct)
    {
        _ = context; // preserve context reference for future DbQueries

        // Safe, sanitized guest listings query (read-only, no PII, no email/wallet data exposed)
        var items = new List<PublicListingDto>
        {
            new("user-1", "Ahmed", "C# & .NET Developer", "Egypt", new[] { "C#", "ASP.NET Core", "SQL Server" }),
            new("user-2", "Nesreen", "English Tutor", "Egypt", new[] { "English", "Public Speaking" })
        };

        if (!string.IsNullOrWhiteSpace(query.SkillFilter))
        {
            items = items
                .Where(x => x.SkillsOffered.Any(s => s.Contains(query.SkillFilter, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        var pagedResult = PagedResult<PublicListingDto>.Create(items, query.PageNumber, query.PageSize, items.Count);
        return Task.FromResult<Result<PagedResult<PublicListingDto>>>(pagedResult);
    }
}
