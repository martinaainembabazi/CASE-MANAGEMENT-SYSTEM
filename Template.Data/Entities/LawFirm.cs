using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class LawFirm : AuditableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public DateTime ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public string Status { get; set; } = "Active";

        // Navigation

        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
        public ICollection<ApplicationUser> Users { get; set; }
            = new List<ApplicationUser>();

        public ICollection<Lawyer> Lawyers { get; set; }
            = new List<Lawyer>();

        public ICollection<CaseAssignment> CaseAssignments { get; set; }
            = new List<CaseAssignment>();

        public ICollection<OtherInstruction> OtherInstructions { get; set; }
            = new List<OtherInstruction>();
    }
}