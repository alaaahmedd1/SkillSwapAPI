namespace SkillSwapAPI.Application.Common.Interfaces.Payments;

public interface IPaymentGatewayService
{
    Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        Guid userId,
        Guid packageId,
        CancellationToken ct = default);
}