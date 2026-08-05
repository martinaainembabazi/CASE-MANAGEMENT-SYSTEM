using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Repository.ApplicationPermissions;
public class ApplicationPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public ApplicationPermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
