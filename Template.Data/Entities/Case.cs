using System.Reflection.Metadata;
using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Case : AuditableEntity
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateTime DateCreated { get; set; }

        // Case Type 
        public int TypeId { get; set; }
        public CaseType Type { get; set; } = null!;

        // Case Status
        public int StatusId { get; set; }
        public CaseStatus Status { get; set; } = null!;

        // User who created the case
        public Guid CreatedBy { get; set; }
        public ApplicationUser CreatedByUser { get; set; } = null!;

        //Archive flags
        public bool IsArchived { get; set; }
        public DateTime? ArchivedDate { get; set; }

        public DateTime? DateClosed { get; set; }

        public int? LawFirmId { get; set; }
        public virtual LawFirm? LawFirm { get; set; }

        // Navigation
        public ICollection<Report> Reports { get; set; }
            = new List<Report>();

        public ICollection<Document> Documents { get; set; }
            = new List<Document>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

        public ICollection<CaseAssignment> Assignments { get; set; }
            = new List<CaseAssignment>();

        public ICollection<Hearing> Hearings { get; set; }
            = new List<Hearing>();

        public ICollection<CaseUpdate> Updates { get; set; }
            = new List<CaseUpdate>();

        public ICollection<FinancialProvision> FinancialProvisions { get; set; }
            = new List<FinancialProvision>();

        public ICollection<Notification> Notifications { get; set; }
            = new List<Notification>();
    }

    
}