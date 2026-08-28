using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Template.Common.Static;
using Template.Core.Models.Cases;
using Template.Core.Models.LawFirm;
using Template.Core.Repository;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Controllers;

[Authorize]
public class LawFirmController : Controller
{
    private readonly ILawFirmRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<LawFirmController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LawFirmController(
        ILawFirmRepository repo,
        IMapper mapper,
        ILogger<LawFirmController> logger,
        ApplicationDbContext context,
    UserManager<ApplicationUser> userManager)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    // GET: LawFirm
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> Index()
    {
        var entities = await _repo.GetAllAsync();
        var model = _mapper.Map<IEnumerable<LawFirmViewModel>>(entities);
        return View(model);
    }

    // GET: LawFirm/Details/5
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> Details(int id)
    {
        var entity = await _repo.FindByIdAsync(id);
        if (entity == null)
        {
            TempData["ErrorMessage"] = "Law firm record not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<LawFirmViewModel>(entity);
        return View(model);
    }

    // GET: LawFirm/Create
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public IActionResult Create()
    {
        return View(new LawFirmViewModel());
    }

    // POST: LawFirm/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> Create(LawFirmViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = _mapper.Map<LawFirm>(model);
        var result = await _repo.CreateAsync(entity);

        if (result)
        {
            TempData["SuccessMessage"] = $"Law firm '{model.Name}' registered successfully.";
            _logger.LogInformation("Created new law firm: {Name}", model.Name);
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to save law firm record. Please try again.");
        return View(model);
    }

    // GET: LawFirm/Edit/5
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _repo.FindByIdAsync(id);
        if (entity == null)
        {
            TempData["ErrorMessage"] = "Law firm record not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<LawFirmViewModel>(entity);
        return View(model);
    }

    // POST: LawFirm/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> Edit(int id, LawFirmViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var entity = await _repo.FindByIdAsync(id);
        if (entity == null)
        {
            TempData["ErrorMessage"] = "Law firm record not found.";
            return RedirectToAction(nameof(Index));
        }

        _mapper.Map(model, entity);
        var result = await _repo.UpdateAsync(entity);

        if (result)
        {
            TempData["SuccessMessage"] = $"Law firm '{model.Name}' updated successfully.";
            _logger.LogInformation("Updated law firm #{Id}", id);
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to update law firm details.");
        return View(model);
    }

    // GET: LawFirm/Delete/5
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.FindByIdAsync(id);
        if (entity == null)
        {
            TempData["ErrorMessage"] = "Law firm record not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<LawFirmViewModel>(entity);
        return View(model);
    }

    // POST: LawFirm/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _repo.SoftDeleteAsync(id);
        if (result)
        {
            TempData["SuccessMessage"] = "Law firm deactivated successfully.";
            _logger.LogWarning("Deactivated law firm #{Id}", id);
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Failed to deactivate law firm.";
        return RedirectToAction(nameof(Index));
    }

    // GET: LawFirm/CreateCounsel
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> CreateCounsel(int? selectedLawFirmId)
    {
        var model = new CreateCounselViewModel
        {
            LawFirmId = selectedLawFirmId ?? 0,
            AvailableLawFirms = await _context.LawFirms
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name
                }).ToListAsync()
        };

        return View(model);
    }

    // POST: LawFirm/CreateCounsel
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> CreateCounsel(CreateCounselViewModel model)
    {
        if (!ModelState.IsValid)
        {
            
            model.AvailableLawFirms = await _context.LawFirms
                .OrderBy(f => f.Name)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(), 
                    Text = f.Name            
                }).ToListAsync();
            return View(model);
        }

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "A user account with this email address already exists.");
            model.AvailableLawFirms = await _context.LawFirms
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToListAsync();
            return View(model);
        }

        // Create the ApplicationUser instance
        var counselUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            LawFirmId = model.LawFirmId, // Link counsel user to chosen Law Firm
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(counselUser, model.Password);

        if (result.Succeeded)
        {
            // Assign the LawFirm / External Counsel role
            await _userManager.AddToRoleAsync(counselUser, RoleConstants.LawFirm);

            TempData["Success"] = $"Counsel account for '{model.FullName}' created successfully!";
            return RedirectToAction("Index", "LawFirm");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.AvailableLawFirms = await _context.LawFirms
            .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
            .ToListAsync();

        return View(model);
    }
}