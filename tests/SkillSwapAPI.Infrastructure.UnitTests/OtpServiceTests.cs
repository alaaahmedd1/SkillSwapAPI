using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Settings;
using SkillSwapAPI.Infrastructure.Identity;
using SkillSwapAPI.Infrastructure.Services;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class OtpServiceTests
{
    [Fact]
    public async Task SendEmailConfirmation_ForUnverifiedAccount_StoresAndEmailsGeneratedOtp()
    {
        var user = new AppUser { Id = Guid.NewGuid(), Email = "ada@example.com", EmailConfirmed = false };
        var userManager = CreateUserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.SetAuthenticationTokenAsync(user, "Email", "EmailConfirmation", Arg.Any<string>()).Returns(IdentityResult.Success);
        var email = Substitute.For<IEmailService>();
        email.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var templates = Substitute.For<IEmailTempService>();
        var generatedOtp = string.Empty;
        templates.GetOtpTemplate(Arg.Do<string>(otp => generatedOtp = otp)).Returns("confirmation body");
        var service = CreateService(userManager, email, templates);

        var result = await service.SendEmailConfirmationAsync(user.Email);

        Assert.True(result.IsSuccess);
        Assert.Matches(new Regex("^\\d{6}$"), generatedOtp);
        await userManager.Received(1).SetAuthenticationTokenAsync(user, "Email", "EmailConfirmation", generatedOtp);
        await email.Received(1).SendAsync(user.Email, Arg.Any<string>(), "confirmation body", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyEmailConfirmation_WithMatchingOtp_ConfirmsAccountAndRemovesOtp()
    {
        var user = new AppUser { Id = Guid.NewGuid(), Email = "ada@example.com", EmailConfirmed = false };
        var userManager = CreateUserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GetAuthenticationTokenAsync(user, "Email", "EmailConfirmation").Returns("012345");
        userManager.UpdateAsync(user).Returns(IdentityResult.Success);
        userManager.RemoveAuthenticationTokenAsync(user, "Email", "EmailConfirmation").Returns(IdentityResult.Success);

        var result = await CreateService(userManager).VerifyEmailConfirmationAsync(user.Email, "012345");

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailConfirmed);
        await userManager.Received(1).UpdateAsync(user);
        await userManager.Received(1).RemoveAuthenticationTokenAsync(user, "Email", "EmailConfirmation");
    }

    [Fact]
    public async Task VerifyEmailConfirmation_WithWrongOtp_DoesNotConfirmOrConsumeStoredCode()
    {
        var user = new AppUser { Id = Guid.NewGuid(), Email = "ada@example.com", EmailConfirmed = false };
        var userManager = CreateUserManager();
        userManager.FindByEmailAsync(user.Email).Returns(user);
        userManager.GetAuthenticationTokenAsync(user, "Email", "EmailConfirmation").Returns("012345");

        var result = await CreateService(userManager).VerifyEmailConfirmationAsync(user.Email, "999999");

        Assert.False(result.IsSuccess);
        Assert.False(user.EmailConfirmed);
        await userManager.DidNotReceive().UpdateAsync(Arg.Any<AppUser>());
        await userManager.DidNotReceive().RemoveAuthenticationTokenAsync(Arg.Any<AppUser>(), Arg.Any<string>(), Arg.Any<string>());
    }

    private static OtpService CreateService(UserManager<AppUser> userManager, IEmailService? email = null, IEmailTempService? templates = null) =>
        new(userManager,
            email ?? Substitute.For<IEmailService>(),
            templates ?? Substitute.For<IEmailTempService>(),
            Options.Create(new OtpSettings { OtpLength = 6, OtpExpiryMinutes = 5, MaxResendAttempts = 3 }),
            Substitute.For<ILogger<OtpService>>());

    private static UserManager<AppUser> CreateUserManager() => Substitute.For<UserManager<AppUser>>(
        Substitute.For<IUserStore<AppUser>>(),
        Options.Create(new IdentityOptions()),
        Substitute.For<IPasswordHasher<AppUser>>(),
        Array.Empty<IUserValidator<AppUser>>(),
        Array.Empty<IPasswordValidator<AppUser>>(),
        Substitute.For<ILookupNormalizer>(),
        new IdentityErrorDescriber(),
        Substitute.For<IServiceProvider>(),
        Substitute.For<ILogger<UserManager<AppUser>>>());
}
