using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Static;
using Template.Core.Models.Report;
using Template.Data;
using Template.Data.Configurations;
using Template.ViewModels;

namespace Template.Web.Controllers
{
    [Authorize(Roles = RoleConstants.LegalStaff + ",Admin," + RoleConstants.ItSupport)]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(LegalReportFilterViewModel filter)
        {
            if (!filter.StartDate.HasValue || !filter.EndDate.HasValue)
            {
                filter.EndDate = DateTime.UtcNow;
                filter.StartDate = filter.Frequency switch
                {
                    "BiAnnual" => DateTime.UtcNow.AddMonths(-6),
                    _ => DateTime.UtcNow.AddMonths(-1)
                };
            }

            var query = _context.Cases
                .Include(c => c.Type)
                .Include(c => c.Status)
                .Include(c => c.LawFirm)
                .Include(c => c.Payments)
                .Include(c => c.Hearings)
                .Include(c => c.Updates)
                .AsQueryable();

            // Filter: Date Range
            query = query.Where(c => c.DateCreated >= filter.StartDate && c.DateCreated <= filter.EndDate);

            // Filter: Management Type (Internal vs External)
            if (filter.ManagementType == "Internal")
            {
                query = query.Where(c => c.LawFirmId == null);
            }
            else if (filter.ManagementType == "External")
            {
                query = query.Where(c => c.LawFirmId != null);
            }

            // Filter: Status (Concluded vs Active/Outstanding)
            if (filter.Status == "Concluded")
            {
                query = query.Where(c => c.DateClosed != null || c.Status.Name == "Concluded" || c.Status.Name == "Closed");
            }
            else if (filter.Status == "Outstanding")
            {
                query = query.Where(c => c.DateClosed == null && c.Status.Name != "Concluded" && c.Status.Name != "Closed");
            }

            var caseEntities = await query.ToListAsync();

            var rows = caseEntities.Select(c =>
            {
                var latestUpdate = c.Updates.OrderByDescending(u => u.CreatedDate).FirstOrDefault();
                var nextHearing = c.Hearings.Where(h => h.HearingDate >= DateTime.UtcNow).OrderBy(h => h.HearingDate).FirstOrDefault();

                // Map payments (adjust filtering if you have specific milestones or types)
                var legalFees = c.Payments.Sum(p => p.Amount);
                var disbursements = 0m;

                return new CaseReportRowViewModel
                {
                    CaseId = c.Id,
                    CaseTitle = c.Title,
                    ManagementType = c.LawFirmId.HasValue ? $"External ({c.LawFirm?.Name})" : "Internal",
                    CaseCategory = c.Type?.Name ?? "General",
                    Status = c.Status?.Name ?? "Pending",

                    SuccessProbability = "Medium", // Placeholder or pull from FinancialProvisions if stored there
                    ClaimedAmount = c.FinancialProvisions.Sum(fp => fp.Amount), // Pulls from Financial Provisions if available
                    LegalFees = legalFees,
                    Disbursements = disbursements,
                    TotalCost = legalFees + disbursements,

                    NextHearingDate = nextHearing?.HearingDate,
                    LastUpdated = latestUpdate?.CreatedDate ?? c.DateCreated,
                    LatestStatusUpdate = latestUpdate?.Description ?? c.Description ?? "No updates logged"
                };
            }).ToList();

            var viewModel = new LegalReportViewModel
            {
                Filter = filter,
                TotalCasesHandled = rows.Count,
                OutstandingCasesCount = rows.Count(r => r.Status != "Concluded" && r.Status != "Closed"),
                ConcludedCasesCount = rows.Count(r => r.Status == "Concluded" || r.Status == "Closed"),
                TotalCosts = rows.Sum(r => r.TotalCost),
                ReportRows = rows
            };

            return View(viewModel);
        }
    }
}