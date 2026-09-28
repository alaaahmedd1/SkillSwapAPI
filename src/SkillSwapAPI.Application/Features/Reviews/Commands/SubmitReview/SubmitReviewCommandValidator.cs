using FluentValidation;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;

namespace SkillSwapAPI.Application.Features.Reviews.Commands.SubmitReview;

public sealed class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.ReviewerId).NotEmpty();
        RuleFor(command => command.RevieweeId).NotEmpty();
        RuleFor(command => command.Rating).InclusiveBetween(1, 5);
        RuleFor(command => command.Comment).MaximumLength(1000);

        RuleFor(command => command)
            .MustAsync(async (command, ct) => !await unitOfWork.Reviews.HasReviewAsync(
                command.SwapRequestId, command.ReviewerId, ct))
            .WithMessage("You have already submitted a review for this swap request.")
            .OverridePropertyName("Review")
            .When(command => command.SwapRequestId != Guid.Empty && command.ReviewerId != Guid.Empty);
    }
}
