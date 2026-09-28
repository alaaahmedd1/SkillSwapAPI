namespace SkillSwapAPI.Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Settings;
using SkillSwapAPI.Infrastructure.Identity;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;
using SkillSwapAPI.Infrastructure.Services;
using SkillSwapAPI.Infrastructure.Settings;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetRequiredSection(JwtSettings.SectionName));

        services.Configure<OtpSettings>(
            configuration.GetRequiredSection(OtpSettings.SectionName));

        services.Configure<SocialAuthSettings>(
            configuration.GetRequiredSection(SocialAuthSettings.SectionName));

        services.Configure<GmailSettings>(
            configuration.GetRequiredSection(GmailSettings.SectionName));

        services.Configure<IdentitySettings>(
            configuration.GetRequiredSection(IdentitySettings.SectionName));

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUnitOfWork, SkillSwapAPI.Infrastructure.UnitOfWork.UnitOfWork>();

        var identitySettings =
            configuration
                .GetRequiredSection(IdentitySettings.SectionName)
                .Get<IdentitySettings>()
            ?? throw new InvalidOperationException(
                "Identity settings are not configured.");

        services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit =
                identitySettings.Password.RequireDigit;

            options.Password.RequireLowercase =
                identitySettings.Password.RequireLowercase;

            options.Password.RequireNonAlphanumeric =
                identitySettings.Password.RequireNonAlphanumeric;

            options.Password.RequireUppercase =
                identitySettings.Password.RequireUppercase;

            options.Password.RequiredLength =
                identitySettings.Password.RequiredLength;

            options.User.RequireUniqueEmail =
                identitySettings.User.RequireUniqueEmail;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<ISocialAuthService, SocialAuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, GmailEmailService>();
        services.AddScoped<IEmailTempService, EmailTemplateService>();

        services.AddHttpClient("Facebook", (serviceProvider, client) =>
        {
            var settings =
                serviceProvider
                    .GetRequiredService<IOptions<SocialAuthSettings>>()
                    .Value;

            client.BaseAddress = new Uri(settings.Facebook.BaseUrl);
        });

        services.AddHttpClient("Apple", (serviceProvider, client) =>
        {
            var settings =
                serviceProvider
                    .GetRequiredService<IOptions<SocialAuthSettings>>()
                    .Value;

            client.BaseAddress = new Uri(settings.Apple.BaseUrl);
        });

        services.AddCachingServices(configuration);

        return services;
    }

    private static void AddCachingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection =
            configuration.GetConnectionString("Redis");

        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, CacheService>();
    }
}
