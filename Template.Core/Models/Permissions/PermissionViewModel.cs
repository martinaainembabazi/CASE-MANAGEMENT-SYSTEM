using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Models.Permissions;
public class PermissionViewModel
{
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public bool IsSelected { get; set; }
}
