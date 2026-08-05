using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Models.Roles
{
    public class RoleListViewModel
    {
        public string Id { get; set; }

        [StringLength(50, ErrorMessage = "Role name cannot exceed 50 characters")]
        public required string Name { get; set; }
    }
}
