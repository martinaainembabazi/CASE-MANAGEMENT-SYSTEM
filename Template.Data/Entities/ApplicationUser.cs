using Microsoft.AspNetCore.Identity;
using static Template.Common.Static.SystemPermissions;

namespace Template.Data.Entities
{
  public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    public string Title { get; set; }
    public DateTime? DisableDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsLoggedIn { get; set; }
    public DateTime LastActivity { get; set; }
    public string BusinessUnit { get; set; }
    public string JobTitle { get; set; }
    public string Station { get; set; }
    public string AgeBracket { get; set; }
    public string Gender { get; set; }

    public int? RoleId { get; set; }
    public Role? Role { get; set; }

    public bool IsActive { get; set; } = true;
    public string? LockReason { get; set; }

    public DateTime? LastLoginDate { get; set; }
    public bool PasswordResetRequired { get; set; }

    public DateTime CreatedDate { get; set; }
    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
    public Guid? UpdatedBy { get; set; }

  public ICollection<InnovationIdea> Ideas { get; set; } = new List<InnovationIdea>();
}
}