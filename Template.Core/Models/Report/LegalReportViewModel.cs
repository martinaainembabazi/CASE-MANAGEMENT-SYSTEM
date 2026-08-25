namespace Template.Core.Models.Report
{
    public class LegalReportFilterViewModel
    {
        public string Frequency { get; set; } = "Monthly";
        public string ManagementType { get; set; } = "All"; // "All", "Internal", "External"
        public string Status { get; set; } = "All"; // "All", "Outstanding", "Concluded"
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class LegalReportViewModel
    {
        public LegalReportFilterViewModel Filter { get; set; } = new();

        public int TotalCasesHandled { get; set; }
        public int OutstandingCasesCount { get; set; }
        public int ConcludedCasesCount { get; set; }
        public decimal TotalClaimedAmount { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal TotalLegalFeesPaid { get; set; }
        public decimal TotalDisbursementsPaid { get; set; }

        public List<CaseReportRowViewModel> ReportRows { get; set; } = new();
    }

    public class CaseReportRowViewModel
    {
        public int CaseId { get; set; }
        public string CaseNumber => $"CASE-{CaseId:D4}";
        public string CaseTitle { get; set; } = string.Empty;
        public string ManagementType { get; set; } = string.Empty;
        public string CaseCategory { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Added properties to resolve CS1061 errors
        public string SuccessProbability { get; set; } = "Unassessed";
        public decimal ClaimedAmount { get; set; } = 0;
        public decimal LegalFees { get; set; } = 0;
        public decimal Disbursements { get; set; } = 0;

        public decimal TotalCost { get; set; }
        public DateTime? NextHearingDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public string LatestStatusUpdate { get; set; } = string.Empty;
    }
}