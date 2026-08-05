using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Models.Permissions;
public class PermissionGroupViewModel
{
    public string GroupName { get; set; }
    public List<PermissionViewModel> Permissions { get; set; } = [];
}
