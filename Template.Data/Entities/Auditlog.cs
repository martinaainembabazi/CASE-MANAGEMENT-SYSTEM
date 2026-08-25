using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class AuditLog : AuditableEntity
{
    public int Id { get; set; }

    public required string LogEntryId { get; set; }

    public Guid UserId { get; set; }              
    public ApplicationUser User { get; set; }= null!;

    public required string Username { get; set; }

    public required string OperationPerformed { get; set; }

    public string? SourceIP { get; set; }
    public string? DestinationIP { get; set; }

    public string? SourceName { get; set; }
    public string? DestinationName { get; set; }

    public string? AffectedEntityType { get; set; }
    public string? AffectedEntityId { get; set; }

    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string? ErrorMessage { get; set; }

    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }

    public string? SessionId { get; set; }
    public string? UserAgent { get; set; }

    public string? ActionDetails { get; set; }
}
}
