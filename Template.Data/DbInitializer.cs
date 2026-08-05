using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Data;

public static class DbInitializer
{
    private const string DefaultPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.MigrateAsync();

        foreach (var roleName in new[] { "Admin", "Staff", "InnovationTeam" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        await SeedRolePermissionsAsync(roleManager, "Admin", GetAllPermissions());

        await SeedUserAsync(
            userManager,
            userName: "admin",
            email: "admin@bou.or.ug",
            roleName: "Admin",
            fullName: "System Administrator",
            firstName: "System",
            lastName: "Administrator",
            title: "Administrator",
            businessUnit: "ICT",
            jobTitle: "System Administrator",
            station: "Head Office");

        await SeedUserAsync(
            userManager,
            userName: "staff",
            email: "staff@bou.or.ug",
            roleName: "Staff",
            fullName: "Test Staff User",
            firstName: "Test",
            lastName: "Staff",
            title: "Officer",
            businessUnit: "Operations",
            jobTitle: "Innovation Officer",
            station: "Head Office");

        await SeedUserAsync(
            userManager,
            userName: "innovation",
            email: "innovation@bou.or.ug",
            roleName: "InnovationTeam",
            fullName: "Innovation Team User",
            firstName: "Innovation",
            lastName: "Reviewer",
            title: "Reviewer",
            businessUnit: "Strategy",
            jobTitle: "Innovation Team Lead",
            station: "Head Office");

        await ResetLoggedInStateAsync(userManager);
        await SeedCategoriesAsync(context);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string roleName,
        string fullName,
        string firstName,
        string lastName,
        string title,
        string businessUnit,
        string jobTitle,
        string station)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user != null)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }

            return;
        }

        user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            Title = title,
            BusinessUnit = businessUnit,
            JobTitle = jobTitle,
            Station = station,
            AgeBracket = "35-44",
            Gender = "Unspecified",
            IsActive = true,
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            LastActivity = DateTime.UtcNow.AddHours(-1),
            IsLoggedIn = false
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
    }

    private static async Task SeedRolePermissionsAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName,
        IEnumerable<string> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return;
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var existingPermissions = existingClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var permission in permissions)
        {
            if (!existingPermissions.Contains(permission))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }
    }

    private static List<string> GetAllPermissions()
    {
        var permissions = new List<string>();
        var nestedTypes = typeof(SystemPermissions).GetNestedTypes();

        foreach (var type in nestedTypes)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string));

            foreach (var field in fields)
            {
                if (field.GetValue(null) is string permissionValue)
                {
                    permissions.Add(permissionValue);
                }
            }
        }

        return permissions;
    }

    private static async Task ResetLoggedInStateAsync(UserManager<ApplicationUser> userManager)
    {
        foreach (var user in userManager.Users.Where(u => u.IsLoggedIn))
        {
            user.IsLoggedIn = false;
            await userManager.UpdateAsync(user);
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        context.Categories.AddRange(
            new Category { Name = "Process Improvement", Description = "Workflow and operational improvements", IsActive = true },
            new Category { Name = "Digital Innovation", Description = "Technology-driven ideas and solutions", IsActive = true },
            new Category { Name = "Customer Experience", Description = "Ideas that improve stakeholder experience", IsActive = true });

        await context.SaveChangesAsync();
    }
}
