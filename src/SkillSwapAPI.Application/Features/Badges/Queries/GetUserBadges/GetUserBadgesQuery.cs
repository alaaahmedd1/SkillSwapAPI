using MediatR;
using SkillSwapAPI.Application.Features.Badges.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Badges.Queries.GetUserBadges;

public sealed record GetUserBadgesQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<UserBadgeDto>>>;
