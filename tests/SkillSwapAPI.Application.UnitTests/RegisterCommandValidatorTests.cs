using SkillSwapAPI.Application.Features.Identity.Commands.Register;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenEmailAndPasswordAreValid()
    {
        var command = new RegisterCommand("Test", "User", "user@example.com", "SecurePassword123!");
        var result = _validator.Validate(command);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("invalid-email", "Password123!")]
    [InlineData("user@example.com", "")]
    [InlineData("user@example.com", "123")]
    public void Validate_ShouldFail_WhenInputsAreInvalid(string email, string password)
    {
        var command = new RegisterCommand("Test", "User", email, password);
        var result = _validator.Validate(command);
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("", "User")]
    [InlineData("Test", "")]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz", "User")]
    [InlineData("Test", "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")]
    public void Validate_ShouldFail_WhenNameIsMissingOrTooLong(string firstName, string lastName)
    {
        var command = new RegisterCommand(firstName, lastName, "user@example.com", "SecurePassword123!");
        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("lowercase1!")]
    [InlineData("NoDigits!")]
    [InlineData("NoSpecial1")]
    public void Validate_ShouldFail_WhenPasswordDoesNotMeetSecurityPolicy(string password)
    {
        var command = new RegisterCommand("Test", "User", "user@example.com", password);
        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("plainaddress")]
    public void Validate_ShouldFail_WhenEmailFormatIsInvalid(string email)
    {
        var command = new RegisterCommand("Test", "User", email, "SecurePassword123!");
        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(" ", "User", "user@example.com", "SecurePassword123!")]
    [InlineData("Test", " ", "user@example.com", "SecurePassword123!")]
    [InlineData("Test", "User", " ", "SecurePassword123!")]
    [InlineData("Test", "User", "user@example.com", " ")]
    public void Validate_ShouldFail_WhenAnyFieldIsWhitespaceOnly(string firstName, string lastName, string email, string password)
    {
        var command = new RegisterCommand(firstName, lastName, email, password);
        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldPass_WhenNamesAreExactlyFiftyCharacters()
    {
        var command = new RegisterCommand(new string('a', 50), new string('b', 50), "user@example.com", "SecurePassword123!");
        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldPass_WhenPasswordIsExactlyEightCharacters()
    {
        var command = new RegisterCommand("Test", "User", "user@example.com", "Abcdef1!");
        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldPass_WhenPasswordHasNoLowercaseLetters()
    {
        var command = new RegisterCommand("Test", "User", "user@example.com", "ABCDEFG1!");
        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
