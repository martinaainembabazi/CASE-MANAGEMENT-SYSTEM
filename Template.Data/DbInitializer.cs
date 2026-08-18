using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // Make sure the database is available
            await context.Database.MigrateAsync();

            // 1. Create the custom Admin role if it doesn't exist

            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Admin");

            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Name = "Admin",
                    Description = "System Administrator",
                    IsActive = true
                };

                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
            }

            // 2. Create the Admin user if it doesn't exist

            var adminUser = await userManager
                .FindByNameAsync("admin");

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@cms.local",

                    FullName = "System Administrator",
                    FirstName = "System",
                    LastName = "Administrator",
                    Title = "System Administrator",

                    BusinessUnit = "Administration",
                    JobTitle = "System Administrator",
                    Station = "Head Office",
                    AgeBracket = "N/A",
                    Gender = "N/A",

                    IsActive = true,
                    PasswordResetRequired = false,

                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = Guid.Empty,
                    LastActivity = DateTime.UtcNow,

                    RoleId = adminRole.Id
                };

                var result = await userManager.CreateAsync(
                    adminUser,
                    "Admin@123"
                );

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    );

                    throw new Exception(
                        $"Failed to create admin user: {errors}"
                    );
                }
            }
        }
    }
}