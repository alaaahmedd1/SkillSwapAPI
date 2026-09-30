using SkillSwapAPI.Application.Features.Admin.Commands.CreateCategory;
using SkillSwapAPI.Application.Features.Admin.Commands.CreateSkill;
using SkillSwapAPI.Application.Features.Admin.Queries.GetUsers;
using SkillSwapAPI.Application.Features.Identity.Commands.VerifyOtp;
using SkillSwapAPI.Application.Features.Users.Commands.UpdateProfile;
using SkillSwapAPI.Application.UnitTests.Common;
using NSubstitute;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Validation;

public sealed class UncoveredBusinessValidatorsTests
{
    [Fact]
    public async Task CreateCategoryValidator_RejectsDuplicateCategoryName()
    {
        var fixture = new UnitOfWorkFixture();
        fixture.SkillCategories.ExistsByNameAsync("Programming", Arg.Any<CancellationToken>()).Returns(true);

        var result = await new CreateCategoryCommandValidator(fixture.UnitOfWork)
            .ValidateAsync(new CreateCategoryCommand(Guid.NewGuid(), "Programming", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateSkillValidator_RejectsUnknownCategory()
    {
        var fixture = new UnitOfWorkFixture();
        fixture.SkillCategories.ExistsAsync(9, Arg.Any<CancellationToken>()).Returns(false);
        fixture.Skills.ExistsByNameAsync("C#", Arg.Any<CancellationToken>()).Returns(false);

        var result = await new CreateSkillCommandValidator(fixture.UnitOfWork)
            .ValidateAsync(new CreateSkillCommand(Guid.NewGuid(), 9, "C#", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("category does not exist", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CreateSkillValidator_RejectsDuplicateSkillName()
    {
        var fixture = new UnitOfWorkFixture();
        fixture.SkillCategories.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        fixture.Skills.ExistsByNameAsync("C#", Arg.Any<CancellationToken>()).Returns(true);

        var result = await new CreateSkillCommandValidator(fixture.UnitOfWork)
            .ValidateAsync(new CreateSkillCommand(Guid.NewGuid(), 1, "C#", null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("not-an-email", "1234")]
    [InlineData("person@example.com", "12ab")]
    [InlineData("person@example.com", "123")]
    public async Task VerifyOtpValidator_RejectsInvalidEmailOrOtp(string email, string otp)
    {
        var result = await new VerifyOtpCommandValidator().ValidateAsync(new VerifyOtpCommand(email, otp));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task VerifyOtpValidator_AcceptsValidEmailAndFourToSixDigitOtp()
    {
        var result = await new VerifyOtpCommandValidator().ValidateAsync(new VerifyOtpCommand("person@example.com", "123456"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task UpdateProfileValidator_RejectsNameAboveIdentityLimit()
    {
        var command = new UpdateProfileCommand(Guid.NewGuid(), new string('A', 51), "Lovelace");

        var result = await new UpdateProfileCommandValidator().ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.FirstName));
    }

    [Theory]
    [InlineData(0, 10, "")]
    [InlineData(1, 51, "")]
    [InlineData(1, 10, "this search value exceeds the supported user-search length because it is deliberately longer than one hundred characters and should trigger the maximum length validation rule in the administrator user search query")]
    public async Task GetUsersValidator_RejectsInvalidPageOrOverlongSearch(int pageNumber, int pageSize, string searchTerm)
    {
        var result = await new GetUsersQueryValidator().ValidateAsync(new GetUsersQuery(SearchTerm: searchTerm, PageNumber: pageNumber, PageSize: pageSize));

        Assert.False(result.IsValid);
    }
}
