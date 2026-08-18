// For the data displayed on the home page
namespace Template.Web.MyModels
{
    public class DashboardViewModel
    {
        // IT Admin Metrics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedAccounts { get; set; }
        public int ActiveLawFirms { get; set; }

        // Legal Officer & Lawyer Metrics
        public int ActiveCases { get; set; }
        public int PendingAssessments { get; set; }
        public int UpcomingHearingsCount { get; set; }
        public decimal TotalFinancialProvision { get; set; }

        // Data Lists for Tables & Feeds
        public List<AuditLogItemDto> RecentAuditLogs { get; set; } = new();
        public List<UpcomingHearingDto> UpcomingHearings { get; set; } = new();
        public List<RecentCaseDto> AssignedCases { get; set; } = new();
    }

    public class AuditLogItemDto
    {
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string OperationPerformed { get; set; }
        public string SourceIp { get; set; }
    }

    public class UpcomingHearingDto
    {
        public int CaseId { get; set; }
        public string Title { get; set; }
        public DateTime HearingDate { get; set; }
        public string AssignedLawFirm { get; set; }
    }

    public class RecentCaseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
