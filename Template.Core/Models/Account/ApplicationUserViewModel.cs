using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Template.Data.Entities;

namespace Template.Core.Models.Account
{
	public class ApplicationUserViewModel
	{
		public string Id  { get; set; }
		public string UserName  { get; set; }
		public string FirstName  { get; set; }
		public string MiddleName  { get; set; }
		public string LastName  { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
		public bool IsActive { get; set; } = true;
		public DateTime? DisableDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string FullName { get; set; }
        public string BusinessUnit { get; set; }
        public string JobTitle { get; set; }
        public string Station { get; set; }
        public string AgeBracket { get; set; }
        public string Gender { get; set; }
        public string? LockReason { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool PasswordResetRequired { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }

        // Multi-role selection properties
        public List<string> SelectedRoles { get; set; } = new();
        public IEnumerable<SelectListItem>? RoleOptions { get; set; }
    }
}
