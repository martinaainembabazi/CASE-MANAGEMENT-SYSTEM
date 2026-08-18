using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class CaseUpdate : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public Guid UpdatedBy { get; set; }
        public ApplicationUser UpdatedByUser { get; set; } = null!;

        public DateTime UpdatedDate { get; set; }

        public string? Description { get; set; }
    }
}