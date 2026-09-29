using FluentValidation;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.SaveWhiteboardSnapshot;

public sealed class SaveWhiteboardSnapshotCommandValidator : AbstractValidator<SaveWhiteboardSnapshotCommand>
{
    public SaveWhiteboardSnapshotCommandValidator()
    {
        RuleFor(command => command.CanvasDataJson).NotEmpty();
    }
}
