using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Queries.SearchUsers;

public sealed record SearchUsersQuery(
    string? SearchTerm = null,
    int? CategoryId = null,
    Guid? OfferedSkillId = null,
    Guid? SeekingSkillId = null,
    decimal? MinRating = null,
    ProficiencyLevel? ProficiencyLevel = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<UserSearchResultDto>>>;
