using System.ComponentModel.DataAnnotations.Schema;
using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public enum HearingStatus
    {
        Scheduled = 1,
        Completed = 2,
        Adjourned = 3,
        Cancelled = 4
    }

    public class Hearing : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        [ForeignKey(nameof(CaseId))]
        public Case Case { get; set; } = null!;

        public DateTime HearingDate { get; set; }
        public string CourtLocation { get; set; } = string.Empty;
        public string JudgeOrMagistrate { get; set; } = string.Empty;
        public string? Purpose { get; set; } // e.g., "Mention", "Cross-examination", "Plea"
        public string? Outcome { get; set; }

        public HearingStatus Status { get; set; } = HearingStatus.Scheduled;
    }
}