using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Infrastructure.Identity;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace App.Infrastructure.Persistance.Seed
{
    public class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

            var roleService = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var userService = scope.ServiceProvider
                .GetRequiredService<UserManager<AppUser>>();

            var config = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();


            await context.Database.MigrateAsync();


            await SeedRolesAsync(roleService);


            await SeedAdminAsync(userService);


            await SeedCreditPackagesAsync(context);
        }

        private static async Task SeedCreditPackagesAsync(ApplicationDbContext context)
        {
            if (await context.CreditPackages.AnyAsync())
                return;

            context.CreditPackages.AddRange(
                new CreditPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Starter",
                    Description = "2 hours of learning time",
                    CreditsCount = 120,
                    Price = 10.00m,
                    Currency = "USD",
                    IsActive = true
                },
                new CreditPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Plus",
                    Description = "5 hours of learning time",
                    CreditsCount = 300,
                    Price = 22.00m,
                    Currency = "USD",
                    IsActive = true
                },
                new CreditPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Pro",
                    Description = "12 hours of learning time",
                    CreditsCount = 720,
                    Price = 45.00m,
                    Currency = "USD",
                    IsActive = true
                });

            await context.SaveChangesAsync();
        }
        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleService)
        {
            string roleName = "Admin";

            await roleService.CreateAsync(new IdentityRole<Guid> { Name = roleName, NormalizedName = roleName.ToUpper() });
        }
        private static async Task SeedAdminAsync(UserManager<AppUser> userService)
        {
            string adminEmail1 = "admin_Ahmed@skillswap.com";
            string adminEmail2 = "admin_Alaa@skillswap.com";
            string adminEmail3 = "admin_Reeham@skillswap.com";
            string adminEmail4 = "admin_Abdulah@skillswap.com";
            List<string> adminEmails = new List<string> { adminEmail1, adminEmail2, adminEmail3, adminEmail4 };
            foreach (var email in adminEmails)
            {

                var existingAdmin = await userService.FindByEmailAsync(email);

                if (existingAdmin is not null)
                    continue;

                var admin = new AppUser
                {
                    FirstName = "Ahmed",
                    LastName = "Ali",
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };
                var res = await userService.CreateAsync(admin, "AHMEDswap22@");
                if (!res.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        res.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to create admin {email} : {errors}");
                }

                await userService.AddToRoleAsync(admin, "Admin");

            }

        }

    }
}
