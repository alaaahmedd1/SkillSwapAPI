using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Admin.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<ProfileIdentityDto>>>
{
    public async Task<Result<PagedResult<ProfileIdentityDto>>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var (items, totalCount) = await identityService.GetPagedUsersAsync(
            query.IsActive, query.SearchTerm, query.PageNumber, query.PageSize, ct);

        return PagedResult<ProfileIdentityDto>.Create(items, totalCount, query.PageNumber, query.PageSize);
    }
}
