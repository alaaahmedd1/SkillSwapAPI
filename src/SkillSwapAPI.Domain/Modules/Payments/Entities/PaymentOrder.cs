using SkillSwapAPI.Domain.Modules.Payments.Enums;

namespace SkillSwapAPI.Domain.Modules.Payments.Entities;

public sealed class PaymentOrder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CreditPackageId { get; set; }
    public CreditPackage CreditPackage { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? ExternalPaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}