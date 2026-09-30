using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AspNetCoreRateLimit;
using NSubstitute;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using SkillSwapAPI.API;
using SkillSwapAPI.API.Hubs;
using SkillSwapAPI.API.Middleware;
using SkillSwapAPI.API.Controllers.Wallet;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetMyWallet;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt;
using SkillSwapAPI.Application.Features.Payments.Commands.ProcessPaymentWebhook;
using SkillSwapAPI.Domain.Common.Results;
using Xunit;

namespace SkillSwapAPI.API.IntegrationTests;

public sealed class AuthorizationPipelineTests
{
    private const string Secret = "test-only-signing-key-which-is-at-least-32-characters";

    [Fact]
    public async Task WalletEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        await using var app = await CreateAppAsync();
        var response = await app.GetTestClient().GetAsync("/api/v1/wallet/me");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminEndpoint_WithAuthenticatedMemberToken_ReturnsForbidden()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken("member"));

        var response = await client.GetAsync("/api/v1/admin/users");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WalletEndpoint_WithExpiredToken_ReturnsUnauthorized()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken("member", DateTime.UtcNow.AddMinutes(-2)));

        var response = await client.GetAsync("/api/v1/wallet/me");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WalletEndpoint_WithTokenSignedByUntrustedKey_ReturnsUnauthorized()
    {
        await using var app = await CreateAppAsync();
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken("User", signingSecret: "different-test-key-which-is-at-least-32-characters"));

        var response = await client.GetAsync("/api/v1/wallet/me");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WalletEndpoint_WithValidUserToken_ReturnsWalletForTokenSubject()
    {
        var userId = Guid.NewGuid();
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetMyWalletQuery>(), Arg.Any<CancellationToken>())
            .Returns(new WalletBalanceDto(75, 120, 45));
        await using var app = await CreateAppAsync(sender);
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken("User", userId: userId));

        var response = await client.GetAsync("/api/v1/wallet/me");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("75", payload, StringComparison.Ordinal);
        await sender.Received(1).Send(Arg.Is<GetMyWalletQuery>(query => query.UserId == userId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReceiptEndpoint_WithValidUserToken_ReturnsPdfContent()
    {
        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();
        var pdf = new byte[] { 37, 80, 68, 70, 45, 1 };
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetWalletTransactionReceiptQuery>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<byte[]>.FromSuccess(pdf)));
        await using var app = await CreateAppAsync(sender);
        using var client = app.GetTestClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", CreateToken("User", userId: userId));

        var response = await client.GetAsync($"/api/v1/wallet/transactions/{transactionId}/receipt");
        var body = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(pdf, body);
        await sender.Received(1).Send(Arg.Is<GetWalletTransactionReceiptQuery>(query => query.UserId == userId && query.TransactionId == transactionId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PaymentWebhookEndpoint_AllowsAnonymousProviderRequest()
    {
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<ProcessPaymentWebhookCommand>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        await using var app = await CreateAppAsync(sender);

        var response = await app.GetTestClient().PostAsync("/api/v1/payments/webhook", new System.Net.Http.StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        await sender.Received(1).Send(Arg.Is<ProcessPaymentWebhookCommand>(command => command.JsonPayload == "{}"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HealthEndpoint_RemainsAnonymous()
    {
        await using var app = await CreateAppAsync();

        var response = await app.GetTestClient().GetAsync("/api/health");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<WebApplication> CreateAppAsync(ISender? sender = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = Secret,
            ["JwtSettings:Issuer"] = "SkillSwapAPI.Tests",
            ["JwtSettings:Audience"] = "SkillSwapAPI.Tests.Client",
            ["ConnectionStrings:HangfireConnection"] = "Server=(local);Database=SkillSwapTests;Trusted_Connection=True;TrustServerCertificate=True"
        });
        builder.Services.AddApi(builder.Configuration);
        builder.Services.AddControllers().AddApplicationPart(typeof(WalletController).Assembly);
        if (sender is not null) builder.Services.AddSingleton(sender);
        // The test host exercises the real HTTP/auth pipeline without starting a worker that
        // would need a SQL Server Hangfire store.
        builder.Services.RemoveAll<Microsoft.Extensions.Hosting.IHostedService>();
        var app = builder.Build();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseIpRateLimiting();
        app.MapHub<ChatHub>("/hubs/chat");
        app.MapHub<LiveSessionHub>("/hubs/live-session");
        app.MapControllers();
        app.MapHealthChecks("/health");
        await app.StartAsync();
        return app;
    }

    private static string CreateToken(string role, DateTime? expires = null, Guid? userId = null, string signingSecret = Secret)
    {
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingSecret)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "SkillSwapAPI.Tests",
            audience: "SkillSwapAPI.Tests.Client",
            claims: [new Claim(ClaimTypes.Role, role), new Claim(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString())],
            notBefore: DateTime.UtcNow.AddMinutes(-5),
            expires: expires ?? DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
