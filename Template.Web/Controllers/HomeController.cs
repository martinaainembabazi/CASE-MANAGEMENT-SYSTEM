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
        //add start
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        //add end
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger) => _logger = logger;

        //[Authorize]
		//public IActionResult Index()
		//{			
			//var permissions = HttpContext.Session.GetString("permissions");
            //ViewData["permissions"] = permissions;
			//return View();
        //}

        //add start

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel();

            // 1. IT Admin Metrics & Feeds (Requirement: IT Admin Operations)
            if (User.IsInRole("Admin") || User.IsInRole("IT Admin"))
            {
                model.TotalUsers = await _context.Users.CountAsync();
                model.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
                model.LockedAccounts = await _context.Users.CountAsync(u => u.LockReason != null);
                model.ActiveLawFirms = await _context.LawFirms.CountAsync(lf => lf.Status == "Active");

                // Fetch recent audit logs for the dashboard widget
                model.RecentAuditLogs = await _context.AuditLogs
                    .OrderByDescending(a => a.LogEntryId)
                    .Take(5)
                    .Select(a => new AuditLogItemDto
                    {
                        Timestamp = DateTime.Now, // Map your timestamp entity
                        UserName = a.Username,
                        OperationPerformed = a.OperationPerformed,
                        SourceIp = a.SourceIP
                    }).ToListAsync();
            }

            // 2. Legal Officer & Law Firm Metrics (Requirement: CASMS-REQ-034)
            if (User.IsInRole("LegalOfficer") || User.IsInRole("Admin") || User.IsInRole("LawFirm") || User.IsInRole("Lawyer"))
            {
                model.ActiveCases = await _context.Cases.CountAsync(c => !c.IsArchived);

                model.UpcomingHearings = await _context.Hearings
                    .Include(h => h.Case)
                    .Where(h => h.Date >= DateTime.Today)
                    .OrderBy(h => h.Date)
                    .Take(5)
                    .Select(h => new UpcomingHearingDto
                    {
                        CaseId = h.CaseId,
                        Title = h.Case.Title,
                        HearingDate = h.Date
                    }).ToListAsync();
            }

            return View(model);
        }
        //add end
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
