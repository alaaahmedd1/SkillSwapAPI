namespace SkillSwapAPI.Application.Features.Payments.Dtos;

public sealed record CheckoutResponseDto(
    Guid OrderId,
    string ClientSecret,
    decimal Amount,
    string Currency);
