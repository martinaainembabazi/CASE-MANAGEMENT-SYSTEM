using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class PaymentMilestone : AuditableEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        // Navigation
        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}