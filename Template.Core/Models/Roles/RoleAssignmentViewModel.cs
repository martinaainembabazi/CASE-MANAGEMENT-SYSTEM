using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.Roles
{
	public class RoleAssignmentViewModel
	{
		[Key]
		public string Id { get; set; }
        public string UserName { get; set; }
		public string[] NewRoles { get; set; }
        public IList<string> AssignedRoles { get; set; }
	}
}
