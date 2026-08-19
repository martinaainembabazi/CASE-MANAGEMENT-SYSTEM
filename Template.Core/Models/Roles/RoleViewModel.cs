

namespace Template.Core.Models.Roles;

public class RoleViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int AssignedUsersCount { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
