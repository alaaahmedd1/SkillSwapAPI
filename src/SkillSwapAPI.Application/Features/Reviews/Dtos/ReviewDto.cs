namespace SkillSwapAPI.Application.Features.Reviews.Dtos;

public sealed record ReviewDto(
    Guid Id,
    Guid SwapRequestId,
    Guid ReviewerId,
    string ReviewerFirstName,
    string ReviewerLastName,
    Guid RevieweeId,
    int Rating,
    string? Comment,
    DateTimeOffset CreatedAtUtc);
