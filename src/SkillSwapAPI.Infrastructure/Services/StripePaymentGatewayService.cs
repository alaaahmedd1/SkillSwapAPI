using SkillSwapAPI.Application.Common.Interfaces.Payments;
using Stripe;
using static SkillSwapAPI.Application.Common.Interfaces.Payments.IPaymentGatewayService;

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

    public PaymentWebhookEvent ProcessWebhookEvent(string jsonPayload, string stripeSignature, string webhookSecret)
    {
        var stripeEvent = Stripe.EventUtility.ConstructEvent(
            jsonPayload,
            stripeSignature,
            webhookSecret);

        if (stripeEvent.Data.Object is Stripe.PaymentIntent paymentIntent)
        {
            bool isSuccess = stripeEvent.Type == "payment_intent.succeeded";
            string? failureReason = isSuccess ? null : paymentIntent.LastPaymentError?.Message;

            return new PaymentWebhookEvent(
                stripeEvent.Type,
                paymentIntent.Id,
                isSuccess,
                failureReason);
        }

        return new PaymentWebhookEvent(stripeEvent.Type, string.Empty, false, "Unhandled event type");
    }
}
