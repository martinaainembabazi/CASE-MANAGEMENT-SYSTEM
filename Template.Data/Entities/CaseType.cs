using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class CaseType : AuditableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        // Navigation
        public ICollection<Case> Cases { get; set; }
            = new List<Case>();
    }
}