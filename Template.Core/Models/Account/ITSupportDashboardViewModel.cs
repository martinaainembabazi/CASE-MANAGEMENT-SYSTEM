namespace Template.Core.Models.Account
{
    public class ITSupportDashboardViewModel
    {
        // Metric Counters
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }
        public int TotalLawFirms { get; set; }
        public int ActiveLawFirmContracts { get; set; }
        public int ExpiredLawFirmContracts { get; set; }

        // Summary Lists
        public List<RecentUserSummaryViewModel> RecentUsers { get; set; } = new();
        public List<RecentAuditLogViewModel> RecentAuditLogs { get; set; } = new();
    }

    public class RecentUserSummaryViewModel
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    public class RecentAuditLogViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}