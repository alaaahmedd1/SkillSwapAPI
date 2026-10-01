using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SkillSwapAPI.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; } = 0.00m;
    public int TotalReviewsCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? TimeZone { get; set; }
    public bool OpenForInstantSwaps { get; set; } = true;
    public bool OnlineOnly { get; set; }
    public bool AutoMatchBarterRequests { get; set; }
}