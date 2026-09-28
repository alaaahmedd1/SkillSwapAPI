using FluentValidation;

namespace SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequestDetails;

public sealed class GetSwapRequestDetailsQueryValidator : AbstractValidator<GetSwapRequestDetailsQuery>
{
    public GetSwapRequestDetailsQueryValidator()
    {
        RuleFor(query => query.SwapRequestId).NotEmpty();
        RuleFor(query => query.UserId).NotEmpty();
    }
}
