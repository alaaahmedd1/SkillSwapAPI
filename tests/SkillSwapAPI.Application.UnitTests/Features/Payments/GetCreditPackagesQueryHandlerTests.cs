using NSubstitute;
using SkillSwapAPI.Application.Features.Payments.Queries.GetCreditPackages;
using SkillSwapAPI.Application.UnitTests.Common;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Payments;

public sealed class GetCreditPackagesQueryHandlerTests
{
    [Fact]
    public async Task Handle_MapsActivePackageCreditPriceAndCurrency()
    {
        var fixture = new UnitOfWorkFixture();
        var package = TestData.CreditPackage(name: "Two Hours", credits: 120, price: 14.50m);
        fixture.CreditPackages.GetActivePackagesAsync(Arg.Any<CancellationToken>()).Returns([package]);

        var result = await new GetCreditPackagesQueryHandler(fixture.UnitOfWork)
            .Handle(new GetCreditPackagesQuery(), CancellationToken.None);

        var item = Assert.Single(result);
        Assert.Equal(package.Id, item.Id);
        Assert.Equal(120, item.CreditsCount);
        Assert.Equal(14.50m, item.Price);
        Assert.Equal("USD", item.Currency);
    }
}
