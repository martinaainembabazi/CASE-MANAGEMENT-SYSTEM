using Template.Core.Models.Permissions;

namespace Template.Core.Models.Roles
{
    public class CombinedViewModel
    {
        public IEnumerable<PermissionViewModel> Permissions { get; set; }
        public ApplicationRoleViewModel Role { get; set; }
        public RoleAssignmentViewModel RoleAssignment { get; set; }
        public IEnumerable<ApplicationRoleViewModel> AllRoles { get; set; }
    }
}