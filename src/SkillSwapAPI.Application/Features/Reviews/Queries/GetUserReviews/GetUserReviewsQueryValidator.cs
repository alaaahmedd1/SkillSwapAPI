using FluentValidation;

namespace SkillSwapAPI.Application.Features.Reviews.Queries.GetUserReviews;

public sealed class GetUserReviewsQueryValidator : AbstractValidator<GetUserReviewsQuery>
{
    public GetUserReviewsQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 50);
    }
}
