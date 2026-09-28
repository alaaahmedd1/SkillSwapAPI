namespace SkillSwapAPI.Application.Features.SwapRequests.Dtos;

public sealed record SwapRequestSkillDto(
    Guid SkillId,
    string SkillName,
    string CategoryName);
