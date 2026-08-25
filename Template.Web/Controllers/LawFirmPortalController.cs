using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Template.Common.Static;
using Template.Data;
using Template.Data.Configurations;

namespace Template.Web.Controllers;

[Authorize(Roles = RoleConstants.LawFirm)]
public class LawFirmPortalController : Controller
{
    private readonly ApplicationDbContext _context;

    public LawFirmPortalController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userIdStr);

        if (currentUser?.LawFirmId == null)
        {
            return Forbid();
        }

        var assignedCases = await _context.CaseAssignments
            .Include(a => a.Case)
                .ThenInclude(c => c.Status)
            .Include(a => a.Case)
                .ThenInclude(c => c.Type)
            .Include(a => a.Instructions)
            .Where(a => a.AssignedLawFirmId == currentUser.LawFirmId)
            .OrderByDescending(a => a.AssignedDate)
            .ToListAsync();

        return View(assignedCases);
    }
}