using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Lawyer : AuditableEntity
    {
        public int Id { get; set; }

        public int LawFirmId { get; set; }
        public LawFirm LawFirm { get; set; } = null!;

        public required string Name { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string Status { get; set; } = "Active";

        // Navigation
        public ICollection<CaseAssignment> CaseAssignments { get; set; }
            = new List<CaseAssignment>();
    }
}