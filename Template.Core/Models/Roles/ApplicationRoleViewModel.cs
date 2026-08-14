using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.Roles;

	public class ApplicationRoleViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

public class RolePermissionMatrixViewModel
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public List<PermissionGroupViewModel> PermissionGroups { get; set; } = new();
}
		
public class PermissionGroupViewModel
{
    public string GroupName { get; set; }
    public List<PermissionItemViewModel> Permissions { get; set; } = new();
	}
}