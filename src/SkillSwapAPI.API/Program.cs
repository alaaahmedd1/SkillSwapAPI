using App.Infrastructure.Persistance.Seed;
using SkillSwapAPI.API;
using SkillSwapAPI.Application;
using SkillSwapAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi(builder.Configuration);

var app = builder.Build();


app.UseApiPipeline();
await DatabaseSeeder.SeedAsync(app.Services);
app.Run();

