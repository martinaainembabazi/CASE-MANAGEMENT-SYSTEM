using Template.Common.AuditColumn;
using System.ComponentModel.DataAnnotations.Schema;

namespace Template.Data.Entities
{
    public class FinancialProvision : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public int FinancialYear { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}