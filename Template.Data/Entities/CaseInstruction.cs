using Template.Common.AuditColumn;

namespace Template.Data.Entities;

public class CaseInstruction : AuditableEntity
{
    public int Id { get; set; }

    public int CaseAssignmentId { get; set; }
    public CaseAssignment CaseAssignment { get; set; } = null!;

    public string InstructionsText { get; set; } = string.Empty;
    public DateTime DateSent { get; set; } = DateTime.UtcNow;

    public Guid SentById { get; set; }
    public ApplicationUser? SentBy { get; set; }
}