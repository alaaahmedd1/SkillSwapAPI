using MediatR;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Queries.GetOwnProfile;

public sealed record GetOwnProfileQuery(Guid UserId) : IRequest<Result<ProfileDto>>;
