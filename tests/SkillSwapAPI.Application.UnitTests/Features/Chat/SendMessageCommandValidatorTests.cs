using SkillSwapAPI.Application.Features.Chat.Commands.SendMessage;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Chat;

public class SendMessageCommandValidatorTests
{
    private readonly SendMessageCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), "Hello!");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenConversationIdIsEmpty()
    {
        var command = new SendMessageCommand(Guid.Empty, Guid.NewGuid(), "Hello!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenSenderIdIsEmpty()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.Empty, "Hello!");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_ShouldFail_WhenContentIsNullOrEmpty(string? content)
    {
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), content!);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldFail_WhenContentExceedsMaxLength()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), new string('a', 2001));

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldPass_WhenContentIsExactlyMaxLength()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), Guid.NewGuid(), new string('a', 2000));

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
