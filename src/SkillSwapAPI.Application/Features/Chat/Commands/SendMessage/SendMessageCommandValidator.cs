using FluentValidation;

namespace SkillSwapAPI.Application.Features.Chat.Commands.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(command => command.ConversationId).NotEmpty();
        RuleFor(command => command.SenderId).NotEmpty();
        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
