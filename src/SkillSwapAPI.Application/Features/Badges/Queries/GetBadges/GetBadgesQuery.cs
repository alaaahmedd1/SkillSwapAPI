using MediatR;
using SkillSwapAPI.Application.Features.Badges.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Badges.Queries.GetBadges;

public sealed record GetBadgesQuery : IRequest<Result<IReadOnlyList<BadgeDto>>>;
