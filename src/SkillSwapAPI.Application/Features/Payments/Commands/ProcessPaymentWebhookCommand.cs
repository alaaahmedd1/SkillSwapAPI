using MediatR;

namespace SkillSwapAPI.Application.Features.Payments.Commands.ProcessPaymentWebhook;

public sealed record ProcessPaymentWebhookCommand(
    string JsonPayload,
    string StripeSignature) : IRequest;