using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Admin.Queries.GetUsers;

public sealed record GetUsersQuery(
    bool? IsActive = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ProfileIdentityDto>>>;
