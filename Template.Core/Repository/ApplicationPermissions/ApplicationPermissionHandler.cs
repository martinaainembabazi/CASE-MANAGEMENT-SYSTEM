using System.Security.Claims;
using Template.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Template.Core.Repository.ApplicationPermissions;
public class ApplicationPermissionHandler(
    UserManager<ApplicationUser> _userManager
    , RoleManager<IdentityRole<Guid>> _roleManager) : AuthorizationHandler<ApplicationPermissionRequirement>
{

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApplicationPermissionRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return;

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // Check permission in user roles
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            if (roleClaims.Any(c => c.Type == "Permission" && c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}

