using SkillSwapAPI.Application.Common.Interfaces.Payments;
using Stripe;

namespace SkillSwapAPI.Infrastructure.Services.Payments;

public sealed class StripePaymentGatewayService : IPaymentGatewayService
{
    public async Task<(string PaymentIntentId, string ClientSecret)> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        Guid userId,
        Guid packageId,
        CancellationToken ct = default)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100), 
            Currency = currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId.ToString() },
                { "PackageId", packageId.ToString() }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: ct);

        return (intent.Id, intent.ClientSecret);
    }
}
