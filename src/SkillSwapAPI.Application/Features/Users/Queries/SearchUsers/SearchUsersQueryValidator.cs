using FluentValidation;

namespace SkillSwapAPI.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 50);
        RuleFor(query => query.MinRating).InclusiveBetween(0, 5).When(query => query.MinRating.HasValue);
        RuleFor(query => query.ProficiencyLevel).IsInEnum().When(query => query.ProficiencyLevel.HasValue);
    }
}
