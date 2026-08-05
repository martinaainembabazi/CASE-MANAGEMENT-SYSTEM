using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Models.Permissions;
public class ManagePermissionsViewModel
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public List<PermissionGroupViewModel> PermissionGroups { get; set; } = [];
}
