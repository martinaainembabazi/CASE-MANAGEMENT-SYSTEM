using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class CaseAssignment : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public Guid? AssignedUserId { get; set; }
        public ApplicationUser? AssignedUser { get; set; }

        public int? AssignedLawFirmId { get; set; }
        public LawFirm? AssignedLawFirm { get; set; }

        public int? AssignedLawyerId { get; set; }
        public Lawyer? AssignedLawyer { get; set; }

        public DateTime AssignedDate { get; set; }
        public bool IsActive { get; set; } = true;

        public string? InstructionsText { get; set; }

        public ICollection<CaseInstruction> Instructions { get; set; } = new List<CaseInstruction>();
    }
}