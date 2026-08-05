using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Role : AuditableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<ApplicationUser> Users { get; set; } 
            = new List<ApplicationUser>();
    }
}