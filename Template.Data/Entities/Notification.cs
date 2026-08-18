using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Notification : AuditableEntity
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public int? CaseId { get; set; }
        public Case? Case { get; set; }

        public required string Title { get; set; }

        public required string Message { get; set; }

        public DateTime SendDate { get; set; }

        public bool IsRead { get; set; }
    }
}