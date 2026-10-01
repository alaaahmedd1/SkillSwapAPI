using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using SkillSwapAPI.Infrastructure.Services.Payments;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class StripePaymentGatewayServiceTests
{
    private const string Secret = "whsec_test_fixture_secret";

    private static StripePaymentGatewayService CreateService() =>
        new(new ConfigurationBuilder().Build());

    [Fact]
    public void ProcessWebhookEvent_WithValidStripeSignature_ParsesPaymentIntentOutcome()
    {
        const string payload = """{"id":"evt_123","object":"event","api_version":"2026-08-26.dahlia","created":1760000000,"data":{"object":{"id":"pi_123","object":"payment_intent","last_payment_error":null}},"livemode":false,"pending_webhooks":1,"type":"payment_intent.succeeded"}""";
        var signature = Sign(payload);

        var result = CreateService().ProcessWebhookEvent(payload, signature, Secret);

        Assert.Equal("payment_intent.succeeded", result.EventType);
        Assert.Equal("pi_123", result.PaymentIntentId);
        Assert.True(result.IsSuccess);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public void ProcessWebhookEvent_WithInvalidSignature_RejectsWebhook()
    {
        var service = CreateService();

        Assert.Throws<Stripe.StripeException>(() => service.ProcessWebhookEvent("{}", "t=1,v1=invalid", Secret));
    }

    [Fact]
    public void ProcessWebhookEvent_WhenPaymentIntentFailed_ReportsFailureOutcome()
    {
        const string payload = """{"id":"evt_456","object":"event","api_version":"2026-08-26.dahlia","created":1760000000,"data":{"object":{"id":"pi_failed","object":"payment_intent","last_payment_error":null}},"livemode":false,"pending_webhooks":1,"type":"payment_intent.payment_failed"}""";

        var result = CreateService().ProcessWebhookEvent(payload, Sign(payload), Secret);

        Assert.Equal("payment_intent.payment_failed", result.EventType);
        Assert.Equal("pi_failed", result.PaymentIntentId);
        Assert.False(result.IsSuccess);
    }

    private static string Sign(string payload)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var signedContent = Encoding.UTF8.GetBytes($"{timestamp}.{payload}");
        var digest = HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), signedContent);
        return $"t={timestamp},v1={Convert.ToHexString(digest).ToLowerInvariant()}";
    }
}
