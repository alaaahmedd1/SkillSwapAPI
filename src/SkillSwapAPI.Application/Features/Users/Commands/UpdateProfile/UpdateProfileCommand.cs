using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string? Title = null,
    string? Bio = null,
    string? City = null,
    string? Country = null,
    string? TimeZone = null,
    bool? OpenForInstantSwaps = null,
    bool? OnlineOnly = null,
    bool? AutoMatchBarterRequests = null)
    : IRequest<Result<Updated>>;
