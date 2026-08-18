using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Template.Core.Models.LawFirm;
using Template.Core.Repository;
using Template.Data.Entities;

namespace Template.Web.Controllers;

public class LawFirmController : Controller
{
    private readonly ILawFirmRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<LawFirmController> _logger;

    public LawFirmController(
        ILawFirmRepository repo,
        IMapper mapper,
        ILogger<LawFirmController> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
    }

    // GET: LawFirm
    public async Task<IActionResult> Index()
    {
        var entities = await _repo.GetAllAsync();
        var model = _mapper.Map<IEnumerable<LawFirmViewModel>>(entities);
        return View(model);
    }

    // GET: LawFirm/Details/5
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
    public IActionResult Create()
    {
        return View(new LawFirmViewModel());
    }

    // POST: LawFirm/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
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
}
