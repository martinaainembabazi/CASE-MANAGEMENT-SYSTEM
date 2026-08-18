using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Hearing : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public DateTime Date { get; set; }

        public string? Outcome { get; set; }
    }
}