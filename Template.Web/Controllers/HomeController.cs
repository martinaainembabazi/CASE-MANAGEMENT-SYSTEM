using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System.Diagnostics;
using Template.Data.Configurations;
using Template.Web.Models;
using Template.Web.MyModels;

namespace Template.Web.Controllers
{
	[Authorize]
    [DefaultBreadcrumb]
    public class HomeController : Controller
	{
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            // 1. IT Admin & System Administration Metrics
            if (User.IsInRole("Admin") || User.IsInRole(RoleConstants.ItSupport))
            {
                model.TotalUsers = await _context.Users.CountAsync();
                model.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
                model.LockedAccounts = await _context.Users.CountAsync(u => u.LockReason != null);
                model.ActiveLawFirms = await _context.LawFirms.CountAsync(lf => lf.Status == "Active");

                model.RecentAuditLogs = await _context.AuditLogs
                    .OrderByDescending(a => a.LogEntryId)
                    .Take(5)
                    .Select(a => new AuditLogItemDto
                    {
                        Timestamp = DateTime.Now,
                        UserName = a.Username,
                        OperationPerformed = a.OperationPerformed,
                        SourceIp = a.SourceIP
                    }).ToListAsync();
            }

            // 2. Legal Staff & External Law Firm Metrics
            if (User.IsInRole("Admin") || User.IsInRole(RoleConstants.LegalStaff) || User.IsInRole(RoleConstants.LawFirm))
            {
                model.ActiveCases = await _context.Cases.CountAsync(c => !c.IsArchived);

                model.UpcomingHearings = await _context.Hearings
                    .Include(h => h.Case)
                    .Where(h => h.HearingDate >= DateTime.Today)
                    .OrderBy(h => h.HearingDate)
                    .Take(5)
                    .Select(h => new UpcomingHearingDto
                    {
                        CaseId = h.CaseId,
                        Title = h.Case.Title,
                        HearingDate = h.HearingDate
                    }).ToListAsync();
            }

            return View(model);
        }
        public IActionResult TestPage()
        {
            return View();
        }

        [Breadcrumb("UI Kit", FromAction = nameof(Index))]
        public IActionResult UiKit()
        {
            return View();
        }

        [Breadcrumb("Privacy", FromAction = nameof(Index))]
        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
