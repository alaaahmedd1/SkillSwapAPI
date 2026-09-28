namespace SkillSwapAPI.Domain.Modules.Payments.Entities;

public sealed class CreditPackage
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CreditsCount { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}