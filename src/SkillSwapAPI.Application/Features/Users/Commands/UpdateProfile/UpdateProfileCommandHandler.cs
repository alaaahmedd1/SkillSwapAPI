using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateProfileCommand, Result<Updated>>
{
    public Task<Result<Updated>> Handle(UpdateProfileCommand command, CancellationToken ct) =>
        identityService.UpdateProfileNamesAsync(
            command.UserId.ToString(),
            command.FirstName,
            command.LastName,
            ct);
}
