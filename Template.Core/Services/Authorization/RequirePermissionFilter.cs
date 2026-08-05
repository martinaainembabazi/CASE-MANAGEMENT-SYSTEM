using System.Security.Claims;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Template.Core.Services.Authorization;
public class RequirePermissionFilter(string _permission
    //, IApplicationPermissionRepository _permissionRepo
    , RoleManager<IdentityRole<Guid>> _roleManager
    , UserManager<ApplicationUser> _userManager) : IAsyncAuthorizationFilter
{

    //public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    //{
    //    var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    //    if (string.IsNullOrEmpty(userId))
    //    {
    //        context.Result = new UnauthorizedResult();
    //        return;
    //    }

    //    var hasPermission = await _permissionRepo.UserHasPermissionAsync(userId, _permission);

    //    if (!hasPermission)
    //    {
    //        context.Result = new ForbidResult();
    //    }
    //}

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var appUser = context.HttpContext.User;
        if (!appUser.Identity.IsAuthenticated)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Logout", "Account", new { ReturnUrl = returnUrl });
            return;
        }

        // Get user ID
        var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            //context.Result = new UnauthorizedResult();
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Logout", "Account", new { ReturnUrl = returnUrl });
            return;
        }

        // For debugging
        System.Diagnostics.Debug.WriteLine($"Checking permission {_permission} for user {userId}");

        // Get user
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get user roles
        var userRoles = await _userManager.GetRolesAsync(user);

        // For debugging
        System.Diagnostics.Debug.WriteLine($"User has roles: {string.Join(", ", userRoles)}");

        // Check each role for permissions
        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) continue;

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims
                .Where(c => c.Type == "Permission")
                .Select(c => c.Value)
                .ToList();

            // For debugging    
            System.Diagnostics.Debug.WriteLine($"Role {roleName} has permissions: {string.Join(", ", permissions)}");

            if (permissions.Contains(_permission))
            {
                // User has permission, allow access
                return;
            }
        }

        // User doesn't have the required permission
        //context.Result = new ForbidResult();

        context.Result = new RedirectToActionResult("Index", "Home", null);
    }
}
