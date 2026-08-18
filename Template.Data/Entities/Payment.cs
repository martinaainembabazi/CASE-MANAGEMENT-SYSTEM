using Template.Common.AuditColumn;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Data.Entities
{
    public class Payment : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        [Column(TypeName ="decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public int? PaymentMilestoneId { get; set; }
        public PaymentMilestone? PaymentMilestone { get; set; }

        public string? Description { get; set; }
    }
}