namespace Template.Core.Repository.ApplicationPermission
{
    public interface IApplicationPermissionRepository
    {
        Task<List<string>> GetAllPermissions();
        Task<List<string>> GetPermissionsForRole(string roleName);
        Task AddPermissionsToRole(string roleName, IEnumerable<string> permissions);
        Task RemovePermissionsFromRole(string roleName, IEnumerable<string> permissions);
        Task<bool> UserHasPermission(string userId, string permission);
        Task<List<string>> GetAllUserPermissions(string userId);
    }
}
