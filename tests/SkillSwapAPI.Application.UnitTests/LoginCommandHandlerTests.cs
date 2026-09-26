using NSubstitute;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Identity.Commands.Login;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests;

public class LoginCommandHandlerTests
{
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly ITokenProvider _tokenProvider = Substitute.For<ITokenProvider>();
    private readonly ILogger<LoginCommandHandler> _logger = Substitute.For<ILogger<LoginCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldReturnEmailNotVerified_WhenUserEmailIsUnconfirmed()
    {
        // Arrange
        var command = new LoginCommand("unverified@example.com", "Password123!");

        _identityService
            .AuthenticateAsync(command.Email, command.Password, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<SkillSwapAPI.Application.Features.Identity.Dtos.AppUserDto>>(ApplicationErrors.Auth.EmailNotVerified));

        var handler = new LoginCommandHandler(
            _identityService,
            _tokenProvider,
            _logger);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("Auth.EmailNotVerified", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
    }
}
