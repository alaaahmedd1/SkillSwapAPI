using SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public sealed class GetGuestListingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithSecondPage_ReturnsOnlyThatPageAndCorrectPagingMetadata()
    {
        var result = await new GetGuestListingsQueryHandler()
            .Handle(new GetGuestListingsQuery(PageNumber: 2, PageSize: 1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(1, result.Value.PageSize);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("user-2", item.Id);
    }

    [Fact]
    public async Task Handle_WithSkillFilter_ReturnsOnlyMatchingPublicListing()
    {
        var result = await new GetGuestListingsQueryHandler()
            .Handle(new GetGuestListingsQuery(SkillFilter: "english"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Single(result.Value.Items);
        Assert.Equal("Nesreen", result.Value.Items[0].DisplayName);
        Assert.DoesNotContain(result.Value.Items, item => item.SkillsOffered.Any(skill => skill.Contains("C#", StringComparison.OrdinalIgnoreCase)));
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task QueryValidator_RejectsOutOfRangePagination(int pageNumber, int pageSize)
    {
        var result = await new GetGuestListingsQueryValidator().ValidateAsync(
            new GetGuestListingsQuery(PageNumber: pageNumber, PageSize: pageSize));

        Assert.False(result.IsValid);
    }
}
