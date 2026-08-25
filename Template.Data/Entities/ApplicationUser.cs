using Microsoft.AspNetCore.Identity;
using static Template.Common.Static.SystemPermissions;

namespace Template.Data.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public DateTime? DisableDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsLoggedIn { get; set; }
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        public string? BusinessUnit { get; set; } = "N/A";
        public string? JobTitle { get; set; } = "N/A";
        public string? Station { get; set; } = "N/A";
        public string? AgeBracket { get; set; } = "N/A";
        public string? Gender { get; set; } = "N/A";

        // Role
        public int? RoleId { get; set; }
        public Role? Role { get; set; }

        // Law Firm
        public int? LawFirmId { get; set; }
        public LawFirm? LawFirm { get; set; }

        public bool IsActive { get; set; } = true;
        public string? LockReason { get; set; }

        public DateTime? LastLoginDate { get; set; }
        public bool PasswordResetRequired { get; set; }

        public DateTime CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }

        // Navigation
        public ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();

        public ICollection<Case> CreatedCases { get; set; }
            = new List<Case>();

        public ICollection<CaseAssignment> CaseAssignments { get; set; }
            = new List<CaseAssignment>();
    }
}