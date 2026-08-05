using System.Reflection;
using System.Security.Claims;
using Template.Common.Static;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Template.Core.Repository.ApplicationPermission;
public class ApplicationPermissionRepository(
    //ApplicationDbContext _db
    RoleManager<IdentityRole<Guid>> _roleManager
    , UserManager<ApplicationUser> _userManager
    //, ILogger<ApplicationPermissionRepository> _logger
    ) : IApplicationPermissionRepository
{
    public async Task AddPermissionsToRole(string roleName, IEnumerable<string> permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName) ?? throw new ArgumentException($"Role '{roleName}' does not exist.");
        var currentPermissions = await GetPermissionsForRole(roleName);
        var newPermissions = permissions.Except(currentPermissions);

        foreach (var permission in newPermissions)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", permission));
        }
    }
    public Task<List<string>> GetAllPermissions()
    {
        // Reflect to get all permission constants
        var permissions = new List<string>();

        // Get all nested classes in the SystemPermissions class
        var nestedTypes = typeof(SystemPermissions).GetNestedTypes();

        foreach (var type in nestedTypes)
        {
            // Get all string constants
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

        return Task.FromResult(permissions);
    }

    public async Task<List<string>> GetPermissionsForRole(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            return new List<string>();

        var claims = await _roleManager.GetClaimsAsync(role);
        return claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();
    }

    public async Task RemovePermissionsFromRole(string roleName, IEnumerable<string> permissions)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
            throw new ArgumentException($"Role '{roleName}' does not exist.");

        var currentClaims = await _roleManager.GetClaimsAsync(role);

        foreach (var permission in permissions)
        {
            var claimToRemove = currentClaims.FirstOrDefault(c =>
                c.Type == "Permission" && c.Value == permission);

            if (claimToRemove != null)
            {
                await _roleManager.RemoveClaimAsync(role, claimToRemove);
            }
        }
    }

    public async Task<bool> UserHasPermission(string userId, string permission)
    {
        var permissions = await GetAllUserPermissions(userId);
        return permissions.Contains(permission);
    }

    public async Task<List<string>> GetAllUserPermissions(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return new List<string>();

        var userRoles = await _userManager.GetRolesAsync(user);
        var allPermissions = new List<string>();

        foreach (var roleName in userRoles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);
                var rolePermissions = roleClaims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value);

                allPermissions.AddRange(rolePermissions);
            }
        }

        return allPermissions.Distinct().ToList();
    }

}
