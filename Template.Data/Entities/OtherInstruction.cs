using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class OtherInstruction : AuditableEntity
    {
        public int Id { get; set; }

        public required string Type { get; set; }

        public string? Description { get; set; }

        public int? AssignedLawFirmId { get; set; }
        public LawFirm? AssignedLawFirm { get; set; }

        public DateTime? AssignedDate { get; set; }

        public string Status { get; set; } = "Pending";
    }
}