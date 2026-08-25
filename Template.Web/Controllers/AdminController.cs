using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Template.Common.Static;
using Template.Data;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Core.Models;

namespace Template.Web.Controllers;

[Authorize(Roles = RoleConstants.ItSupport)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Single unified constructor injecting all required services
    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,IT Support")]
    public async Task<IActionResult> Dashboard()
    {
        var users = await _userManager.Users.ToListAsync();
        var lawFirms = await _context.LawFirms.ToListAsync();

        // Fetch recent audit logs
        var recentLogs = await _context.AuditLogs
            .OrderByDescending(a => a.CreatedDate)
            .Take(5)
            .Select(a => new RecentAuditLogViewModel
            {
                Action = a.ActionDetails,
                PerformedBy = a.Username,
                Timestamp = a.CreatedDate
            })
            .ToListAsync();

        var recentUserList = new List<RecentUserSummaryViewModel>();
        foreach (var u in users.OrderByDescending(u => u.CreatedDate).Take(5))
        {
            var roles = await _userManager.GetRolesAsync(u);
            recentUserList.Add(new RecentUserSummaryViewModel
            {
                Id = u.Id,
                UserName = u.UserName ?? "N/A",
                Email = u.Email ?? "N/A",
                IsActive = u.IsActive && (!u.DisableDate.HasValue || u.DisableDate > DateTime.UtcNow),
                Roles = roles.ToList(),
                CreatedDate = u.CreatedDate
            });
        }

        var model = new ITSupportDashboardViewModel
        {
            TotalUsers = users.Count,
            ActiveUsers = users.Count(u => u.IsActive && (!u.DisableDate.HasValue || u.DisableDate > DateTime.UtcNow)),
            LockedUsers = users.Count(u => !u.IsActive || (u.DisableDate.HasValue && u.DisableDate <= DateTime.UtcNow)),
            TotalLawFirms = lawFirms.Count,
            ActiveLawFirmContracts = lawFirms.Count(f => f.ContractEndDate >= DateTime.UtcNow),
            ExpiredLawFirmContracts = lawFirms.Count(f => f.ContractEndDate < DateTime.UtcNow),
            RecentUsers = recentUserList,
            RecentAuditLogs = recentLogs
        };

        return View(model);
    }
}