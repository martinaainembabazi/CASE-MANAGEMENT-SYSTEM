using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System.Security.Claims;
using Template.Core.Models.Cases;
using Template.Core.Repository.Cases;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Controllers;

[Authorize]
public class CaseController(
    ILogger<CaseController> _logger,
    ICaseRepository _caseRepo,
    IMapper _mapper,
    ApplicationDbContext _context 
    ) : Controller
{
    [Breadcrumb("Cases & Matters", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public async Task<IActionResult> Index()
    {
        var cases = await _caseRepo.FindAll();

        var viewModels = cases.Select(c => new CaseViewModel
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            DateCreated = c.DateCreated,
            TypeId = c.TypeId,
            TypeName = c.Type?.Name ?? "Unassigned",
            StatusId = c.StatusId,
            StatusName = c.Status?.Name ?? "Pending",
            CreatedBy = c.CreatedBy,
            CreatedByName = c.CreatedByUser?.UserName ?? "System",
            IsArchived = c.IsArchived,
            DateClosed = c.DateClosed,
            HearingCount = c.Hearings?.Count ?? 0,
            DocumentCount = c.Documents?.Count ?? 0,
            AssignmentCount = c.Assignments?.Count ?? 0
        }).ToList();

        return View(viewModels);
    }

    [Breadcrumb("Case Details", FromAction = nameof(Index))]
    public async Task<IActionResult> Details(int id)
    {
        var caseEntity = await _caseRepo.FindById(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var viewModel = new CaseViewModel
        {
            Id = caseEntity.Id,
            Title = caseEntity.Title,
            Description = caseEntity.Description,
            DateCreated = caseEntity.DateCreated,
            TypeId = caseEntity.TypeId,
            TypeName = caseEntity.Type?.Name ?? "Unassigned",
            StatusId = caseEntity.StatusId,
            StatusName = caseEntity.Status?.Name ?? "Pending",
            CreatedBy = caseEntity.CreatedBy,
            CreatedByName = caseEntity.CreatedByUser?.UserName ?? "System",
            IsArchived = caseEntity.IsArchived,
            DateClosed = caseEntity.DateClosed,
            HearingCount = caseEntity.Hearings?.Count ?? 0,
            DocumentCount = caseEntity.Documents?.Count ?? 0,
            AssignmentCount = caseEntity.Assignments?.Count ?? 0
        };

        return View(viewModel);
    }

    [HttpGet]
    [Breadcrumb("Create New Case", FromAction = nameof(Index))]
    public async Task<IActionResult> Create()
    {
        var model = new CreateCaseViewModel
        {
            TypeOptions = await GetTypeSelectListAsync(),
            StatusOptions = await GetStatusSelectListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.TypeOptions = await GetTypeSelectListAsync();
            model.StatusOptions = await GetStatusSelectListAsync();
            return View(model);
        }

        // Get current logged-in user's Id (fallback to Guid.Empty if claim isn't found)
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(userIdClaim, out var userId);

        var newCase = new Case
        {
            Title = model.Title,
            Description = model.Description,
            TypeId = model.TypeId,
            StatusId = model.StatusId,
            CreatedBy = userId,
            DateCreated = DateTime.UtcNow,
            IsArchived = false
        };

        await _caseRepo.Add(newCase);
        _logger.LogInformation("New case created successfully with ID {CaseId}", newCase.Id);

        TempData["SuccessMessage"] = "Case created successfully!";
        return RedirectToAction(nameof(Index));
    }

    // GET: Case/Edit/5
    [HttpGet]
    [Breadcrumb("Edit Case", FromAction = nameof(Index))]
    public async Task<IActionResult> Edit(int id)
    {
        var caseEntity = await _caseRepo.FindById(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var model = new EditCaseViewModel
        {
            Id = caseEntity.Id,
            Title = caseEntity.Title,
            Description = caseEntity.Description,
            TypeId = caseEntity.TypeId,
            StatusId = caseEntity.StatusId,
            IsArchived = caseEntity.IsArchived,
            TypeOptions = await GetTypeSelectListAsync(),
            StatusOptions = await GetStatusSelectListAsync()
        };

        return View(model);
    }

    // POST: Case/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditCaseViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            model.TypeOptions = await GetTypeSelectListAsync();
            model.StatusOptions = await GetStatusSelectListAsync();
            return View(model);
        }

        var caseEntity = await _caseRepo.FindById(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        // Get logged-in user ID for audit column
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        _ = Guid.TryParse(userIdClaim, out var userId);

        caseEntity.Title = model.Title;
        caseEntity.Description = model.Description;
        caseEntity.TypeId = model.TypeId;
        caseEntity.StatusId = model.StatusId;
        caseEntity.IsArchived = model.IsArchived;
        caseEntity.ModifiedBy = userIdClaim;
        caseEntity.ModifiedDate = DateTime.UtcNow;

        await _caseRepo.Update(caseEntity);
        _logger.LogInformation("Case ID {CaseId} updated successfully.", caseEntity.Id);

        TempData["SuccessMessage"] = "Case updated successfully!";
        return RedirectToAction(nameof(Details), new { id = caseEntity.Id });
    }

    // GET: Case/Delete/5
    [HttpGet]
    [Breadcrumb("Delete Case", FromAction = nameof(Index))]
    public async Task<IActionResult> Delete(int id) 
    {
        var caseEntity = await _caseRepo.FindById(id);
        if (caseEntity == null)
        {
            TempData["ErrorMessage"] = "Legal case record not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<CaseViewModel>(caseEntity);
        return View(model);
    }

    // POST: Case/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
  
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var caseEntity = await _caseRepo.FindById(id);
        if (caseEntity == null)
        {
            TempData["ErrorMessage"] = "Legal case record not found.";
            return RedirectToAction(nameof(Index));
        }

        await _caseRepo.Delete(id);

        TempData["SuccessMessage"] = $"Case #{id} ('{caseEntity.Title}') was successfully removed.";
        _logger.LogWarning("Legal case record #{CaseId} deleted.", id);

        return RedirectToAction(nameof(Index));
    }

    // Inject ApplicationDbContext (or ICaseStatusRepository) into CaseController constructor
    private async Task<IEnumerable<SelectListItem>> GetTypeSelectListAsync()
    {
        // Fetch directly from your DbContext
        return await _context.CaseTypes
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            })
            .ToListAsync();
    }

    private async Task<IEnumerable<SelectListItem>> GetStatusSelectListAsync()
    {
        // Fetch directly from your DbContext
        return await _context.CaseStatuses
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();
    }
}