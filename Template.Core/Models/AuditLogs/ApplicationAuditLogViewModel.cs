
using Template.Common.Enums;

namespace Template.Core.Models.AuditLogs
{
	public class ApplicationAuditLogViewModel
	{
		public int Id { get; set; }
        public string LogEntryId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public AuditEventType EventType { get; set; }
        public string OperationPerformed { get; set; }
        public string? SourceIP { get; set; }
        public string? DestinationIP { get; set; }
        public string? SourceName { get; set; }
        public string? DestinationName { get; set; }
        public string? AffectedEntityType { get; set; }
        public string? AffectedEntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public AuditStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RequestData { get; set; }
        public string? ResponseData { get; set; }
        public string? SessionId { get; set; }
        public string? UserAgent { get; set; }
        public string? ActionDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
	}
}
