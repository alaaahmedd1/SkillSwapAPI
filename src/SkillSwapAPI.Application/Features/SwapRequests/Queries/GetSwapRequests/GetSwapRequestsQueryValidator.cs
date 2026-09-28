using FluentValidation;

namespace SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequests;

public sealed class GetSwapRequestsQueryValidator : AbstractValidator<GetSwapRequestsQuery>
{
    public GetSwapRequestsQueryValidator()
    {
        RuleFor(query => query.UserId).NotEmpty();
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 50);
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
