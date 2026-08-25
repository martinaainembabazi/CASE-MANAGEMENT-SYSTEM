using System.ComponentModel.DataAnnotations.Schema;
using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class FinancialProvision : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }

        [ForeignKey(nameof(CaseId))]
        public Case Case { get; set; } = null!;

        public string FinancialYear { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Justification { get; set; }

        public ProvisionStatus Status { get; set; } = ProvisionStatus.Pending;

        public DateTime DateLogged { get; set; } = DateTime.UtcNow;
    }
}

public enum ProvisionStatus
    {
        Pending = 1,
        Approved = 2,
        RolledOver = 3,
        Released = 4
    }
