namespace SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;

public sealed record PublicListingDto(
    string Id,
    string DisplayName,
    string Title,
    string Country,
    IReadOnlyList<string> SkillsOffered);
