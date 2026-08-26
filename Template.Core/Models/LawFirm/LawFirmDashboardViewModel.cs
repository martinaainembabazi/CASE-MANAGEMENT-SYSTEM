namespace Template.Core.Models.LawFirm
{
    public class LawFirmDashboardViewModel
    {
        public string LawFirmName { get; set; } = string.Empty;
        public int ActiveCasesCount { get; set; }
        public int PendingMilestonesCount { get; set; }
        public int CompletedCasesCount { get; set; }
        public bool IsContractActive { get; set; }
        public DateTime? ContractExpiryDate { get; set; }

        public IEnumerable<AssignedCaseDto> RecentAssignedCases { get; set; } = new List<AssignedCaseDto>();
        public IEnumerable<LawFirmActivityDto> RecentUpdates { get; set; } = new List<LawFirmActivityDto>();
    }

    public class AssignedCaseDto
    {
        public int CaseId { get; set; }
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
    }

    public class LawFirmActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string BadgeClass { get; set; } = "bg-light text-secondary";
    }
}