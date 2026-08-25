using System.ComponentModel.DataAnnotations;
using Template.Data.Entities;

namespace Template.Core.Models.Cases
{
    public class UpcomingHearingDashboardItem
    {
        public int CaseId { get; set; }
        public string CaseTitle { get; set; } = string.Empty;
        public DateTime HearingDate { get; set; }
        public string CourtLocation { get; set; } = string.Empty;
        public string JudgeOrMagistrate { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
    }

    public class SystemNotificationItem
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string IconClass { get; set; } = "ti ti-bell";
        public string BadgeClass { get; set; } = "bg-primary";
    }

    public class DashboardViewModel
    {
        // Summary Counter Cards
        public int TotalCases { get; set; }
        public int ActiveAssignments { get; set; }
        public int UpcomingHearingsCount { get; set; }
        public decimal TotalFinancialProvisions { get; set; }
        public decimal TotalLegalFeesPaid { get; set; }

        // Data Collections
        public List<Case> RecentCases { get; set; } = new();
        public List<UpcomingHearingDashboardItem> UpcomingHearings { get; set; } = new();
        public List<SystemNotificationItem> Notifications { get; set; } = new();
    }
}