namespace SkillSwapAPI.Infrastructure;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Settings;
using SkillSwapAPI.Infrastructure.Identity;
using SkillSwapAPI.Infrastructure.Persistence.Data.NewFolder;
using SkillSwapAPI.Infrastructure.Services;
using SkillSwapAPI.Infrastructure.Settings;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<OtpSettings>(configuration.GetSection("Otp"));
        services.Configure<SocialAuthSettings>(configuration.GetSection("SocialAuth"));
        services.Configure<GmailSettings>(configuration.GetSection("Gmail"));

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(localdb)\\mssqllocaldb;Database=SkillSwapDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<ISocialAuthService, SocialAuthService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<IEmailService, GmailEmailService>();
        services.AddScoped<IEmailTempService, EmailTemplateService>();

        services.AddHttpClient("Facebook", client =>
        {
            client.BaseAddress = new Uri("https://graph.facebook.com/v19.0/");
        });
        services.AddHttpClient("Apple", client =>
        {
            client.BaseAddress = new Uri("https://appleid.apple.com/");
        });
        services.AddCachingServices(configuration);

        return services;
    }

    private static void AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis");

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
