using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Models.Account;
    public class ApplicationUserRoleViewModel
{
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public bool IsSelected { get; set; }
}
