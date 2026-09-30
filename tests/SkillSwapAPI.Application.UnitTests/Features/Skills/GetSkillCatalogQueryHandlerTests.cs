using NSubstitute;
using SkillSwapAPI.Application.Features.Skills.Queries.GetSkillCatalog;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Skills.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Skills;

public sealed class GetSkillCatalogQueryHandlerTests
{
    [Fact]
    public async Task Handle_MapsCategoriesAndSortsEachCategorySkillsByName()
    {
        var fixture = new UnitOfWorkFixture();
        var category = TestData.Category(3, "Programming");
        category.Skills.Add(TestData.Skill(name: "Rust", categoryId: 3, category: category));
        category.Skills.Add(TestData.Skill(name: "C#", categoryId: 3, category: category));
        fixture.SkillCategories.GetActiveWithSkillsAsync(Arg.Any<CancellationToken>()).Returns([category]);

        var result = await new GetSkillCatalogQueryHandler(fixture.UnitOfWork)
            .Handle(new GetSkillCatalogQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(new[] { "C#", "Rust" }, result.Value[0].Skills.Select(skill => skill.Name));
    }
}
