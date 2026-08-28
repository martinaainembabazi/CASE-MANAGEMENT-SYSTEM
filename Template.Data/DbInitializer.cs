using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            if (!context.PaymentMilestones.Any())
            {
                context.PaymentMilestones.AddRange(
                    new PaymentMilestone { Name = "Issuance of instructions" },
                    new PaymentMilestone { Name = "Conclusion of hearing" },
                    new PaymentMilestone { Name = "Judgement" }
                );
                await context.SaveChangesAsync();
            }

            // 1. Helper to seed custom Role table with int Primary Key
            async Task<Role> EnsureRoleExistsAsync(string roleName, string description)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Role
                    {
                        Name = roleName,
                        Description = description,
                        IsActive = true
                    };
                    context.Roles.Add(role);
                    await context.SaveChangesAsync();
                }
                return role;
            }

            var adminRole = await EnsureRoleExistsAsync("Admin", "System Administrator");
            var itSupportRole = await EnsureRoleExistsAsync(RoleConstants.ItSupport, "IT Support Administrator");
            var legalStaffAdminRole = await EnsureRoleExistsAsync(RoleConstants.LegalStaffAdmin, "Legal Department Administrator");
            var legalStaffRole = await EnsureRoleExistsAsync(RoleConstants.LegalStaff, "Legal Department Staff");
            var lawFirmRole = await EnsureRoleExistsAsync(RoleConstants.LawFirm, "External Counsel & Law Firm");

            // 2. Helper to seed user with int RoleId
            async Task SeedUserAsync(
                string username,
                string email,
                string fullName,
                string businessUnit,
                string jobTitle,
                Role role,
                string password)
            {
                var user = await userManager.FindByNameAsync(username);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        FullName = fullName,
                        FirstName = fullName.Split(' ')[0],
                        LastName = fullName.Contains(' ') ? fullName.Split(' ')[1] : fullName,
                        Title = jobTitle,
                        BusinessUnit = businessUnit,
                        JobTitle = jobTitle,
                        Station = "Head Office",
                        AgeBracket = "N/A",
                        Gender = "N/A",
                        IsActive = true,
                        PasswordResetRequired = false,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = Guid.Empty,
                        LastActivity = DateTime.UtcNow,
                        RoleId = role.Id
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role.Name);
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create user {username}: {errors}");
                    }
                }
            }

            // 3. Execute Seeding
            await SeedUserAsync("admin", "admin@cms.local", "System Administrator", "Administration", "System Administrator", adminRole, "Admin@123");
            await SeedUserAsync("itsupport", "itsupport@bou.or.ug", "IT Support Officer", "IT Department", "IT Support Specialist", itSupportRole, "Admin@123");
            await SeedUserAsync("legaladmin", "legaladmin@bou.or.ug", "Senior Legal Administrator", "Legal Department", "Legal Admin Officer", legalStaffAdminRole, "Admin@123");
            await SeedUserAsync("legalstaff", "legalstaff@bou.or.ug", "Legal Counsel", "Legal Department", "Legal Officer", legalStaffRole, "Admin@123");
            await SeedUserAsync("counsel", "counsel@lawfirm.com", "External Counsel", "External Law Firm", "Managing Partner", lawFirmRole, "Admin@123");
        }
    }
}