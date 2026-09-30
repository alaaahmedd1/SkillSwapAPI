using FluentValidation;

namespace SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;

public sealed class GetGuestListingsQueryValidator : AbstractValidator<GetGuestListingsQuery>
{
    public GetGuestListingsQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 50);
    }
}
