using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using QuestPDF.Infrastructure;
using SkillSwapAPI.Infrastructure.Services;
using SkillSwapAPI.Infrastructure.Services.Pdf;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class CacheAndPdfServiceTests
{
    [Fact]
    public async Task CacheService_RoundTripsValuesAndRemovesInvalidatedEntries()
    {
        var cache = Substitute.For<IDistributedCache>();
        byte[]? stored = null;
        cache.GetAsync("catalog", Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(stored));
        cache.SetAsync("catalog", Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                stored = call.Arg<byte[]>();
                return Task.CompletedTask;
            });
        cache.RemoveAsync("catalog", Arg.Any<CancellationToken>()).Returns(_ =>
        {
            stored = null;
            return Task.CompletedTask;
        });
        var service = new CacheService(cache);

        await service.SetAsync("catalog", new[] { "C#", "Design" }, TimeSpan.FromMinutes(1));
        var cached = await service.GetAsync<string[]>("catalog");
        await service.RemoveAsync("catalog");
        var afterRemoval = await service.GetAsync<string[]>("catalog");

        cached.Should().Equal("C#", "Design");
        afterRemoval.Should().BeNull();
        await cache.Received(1).SetAsync("catalog", Arg.Any<byte[]>(), Arg.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(1)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void QuestPdfService_GeneratesNonEmptyPdfReceipt()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var pdf = new QuestPdfService().GenerateTimeReceipt(
            Guid.NewGuid(), "SWAP-20260101-abc", "Time earned", "Earned", 45, 120, DateTimeOffset.UtcNow);

        pdf.Should().NotBeEmpty();
        System.Text.Encoding.ASCII.GetString(pdf, 0, 5).Should().Be("%PDF-");
    }
}
