using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;

public sealed record GetGuestListingsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SkillFilter = null
) : IRequest<Result<PagedResult<PublicListingDto>>>;
