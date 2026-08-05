using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.Roles
{
	public class ApplicationRoleViewModel
	{
		[Key]
		public string Id { get; set; }
		
		public string Name { get; set; }

		[Required]
		public string Description { get; set; }

		[Required]
		public string Permissions { get; set; }
		
		public string[] ConvertedPermissions { get; set; }
	}
}
