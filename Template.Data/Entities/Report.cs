using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Report : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public Guid RequestedBy { get; set; }
        public ApplicationUser RequestedByUser { get; set; } = null!;
    }
}