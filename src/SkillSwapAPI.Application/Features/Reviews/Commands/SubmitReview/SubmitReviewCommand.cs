using MediatR;
using SkillSwapAPI.Application.Features.Reviews.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Reviews.Commands.SubmitReview;

public sealed record SubmitReviewCommand(
    Guid SwapRequestId,
    Guid ReviewerId,
    Guid RevieweeId,
    int Rating,
    string? Comment) : IRequest<Result<ReviewDto>>;
