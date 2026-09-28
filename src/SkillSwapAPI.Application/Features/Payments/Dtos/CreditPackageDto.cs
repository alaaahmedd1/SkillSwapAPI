namespace SkillSwapAPI.Application.Features.Payments.Dtos;

public sealed record CreditPackageDto(
    Guid Id,
    string Name,
    string Description,
    int CreditsCount,
    decimal Price,
    string Currency);