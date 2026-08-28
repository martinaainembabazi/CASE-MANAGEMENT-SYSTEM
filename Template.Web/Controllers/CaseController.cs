using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System.Security.Claims;
using Template.Common.Static;
using Template.Core.Models.Cases;
using Template.Core.Models.Document;
using Template.Core.Repository.Cases;
using Template.Core.Services;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Controllers;

[Authorize]
public class CaseController(
    ILogger<CaseController> _logger,
    ICaseRepository _caseRepo,
    IMapper _mapper,
    IEmailService _emailService,
    ApplicationDbContext _context,
    UserManager<ApplicationUser> _userManager
    ) : Controller
{
    [HttpGet]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.UtcNow;

        // 1. Basic Counts
        var totalCases = await _context.Cases.CountAsync(c => !c.IsArchived);
        var activeAssignments = await _context.CaseAssignments.CountAsync();

        var upcomingHearingsQuery = _context.Hearings
            .Include(h => h.Case)
            .Where(h => h.HearingDate >= today && h.Status == HearingStatus.Scheduled);

        var upcomingHearingsCount = await upcomingHearingsQuery.CountAsync();

        // 2. Financial Aggregates
        var totalProvisions = await _context.FinancialProvisions.SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var totalFeesPaid = await _context.Payments.SumAsync(p => (decimal?)p.Amount) ?? 0m;

        // 3. Detailed Lists
        var recentCases = await _context.Cases
            .Include(c => c.Type)
            .Include(c => c.Status)
            .Where(c => !c.IsArchived)
            .OrderByDescending(c => c.DateCreated)
            .Take(5)
            .ToListAsync();

        var upcomingHearingsList = await upcomingHearingsQuery
            .OrderBy(h => h.HearingDate)
            .Take(5)
            .Select(h => new UpcomingHearingDashboardItem
            {
                CaseId = h.CaseId,
                CaseTitle = h.Case.Title,
                HearingDate = h.HearingDate,
                CourtLocation = h.CourtLocation,
                JudgeOrMagistrate = h.JudgeOrMagistrate,
                Purpose = h.Purpose ?? "Scheduled Hearing"
            })
            .ToListAsync();

        // 4. Activity Notifications Feed (Combining recent events)
        var notifications = new List<SystemNotificationItem>();

        var recentPayments = await _context.Payments
            .Include(p => p.Case)
            .Include(p => p.PaymentMilestone)
            .OrderByDescending(p => p.PaymentDate)
            .Take(3)
            .ToListAsync();

        foreach (var p in recentPayments)
        {
            notifications.Add(new SystemNotificationItem
            {
                Title = "Legal Fee Logged",
                Message = $"Fee of {p.Amount:N2} logged for '{p.Case.Title}' ({p.PaymentMilestone?.Name ?? "Milestone"})",
                Timestamp = p.PaymentDate,
                IconClass = "ti ti-receipt",
                BadgeClass = "bg-success"
            });
        }

        var recentProvisions = await _context.FinancialProvisions
            .Include(p => p.Case)
            .OrderByDescending(p => p.DateLogged)
            .Take(3)
            .ToListAsync();

        foreach (var prov in recentProvisions)
        {
            notifications.Add(new SystemNotificationItem
            {
                Title = "Year-End Provision",
                Message = $"Provision of {prov.Amount:N2} reserved for '{prov.Case.Title}' ({prov.FinancialYear})",
                Timestamp = prov.DateLogged,
                IconClass = "ti ti-pig-money",
                BadgeClass = "bg-info text-dark"
            });
        }

        // Build Final ViewModel
        var viewModel = new DashboardViewModel
        {
            TotalCases = totalCases,
            ActiveAssignments = activeAssignments,
            UpcomingHearingsCount = upcomingHearingsCount,
            TotalFinancialProvisions = totalProvisions,
            TotalLegalFeesPaid = totalFeesPaid,
            RecentCases = recentCases,
            UpcomingHearings = upcomingHearingsList,
            Notifications = notifications.OrderByDescending(n => n.Timestamp).Take(5).ToList()
        };

        return View(viewModel);
    }

    [Breadcrumb("Cases & Matters", FromAction = nameof(Index), FromController = typeof(HomeController))]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
    public async Task<IActionResult> Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdString, out Guid currentUserId);

        // Fetch cases including active assignments
        var cases = await _context.Cases
            .Where(c => !c.IsArchived)
            .Include(c => c.Type)
            .Include(c => c.Status)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Include(c => c.Assignments)
                .ThenInclude(a => a.AssignedLawFirm)
            .Include(c => c.Assignments)
                .ThenInclude(a => a.AssignedUser)
            .ToListAsync();

        IEnumerable<Case> filteredCases = cases;

        // 1. External Counsel Filter
        if (User.IsInRole(RoleConstants.LawFirm))
        {
            // Query database directly to guarantee LawFirmId is populated
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (currentUser?.LawFirmId != null)
            {
                filteredCases = cases.Where(c => c.Assignments != null && c.Assignments.Any(a =>
                    a.IsActive &&
                    a.AssignmentType == AssignmentType.External &&
                    a.AssignedLawFirmId == currentUser.LawFirmId));
            }
            else
            {
                filteredCases = Enumerable.Empty<Case>();
            }
        }
        // 2. Regular Legal Staff Filter
        else if (User.IsInRole(RoleConstants.LegalStaff) && !User.IsInRole(RoleConstants.LegalStaffAdmin) && !User.IsInRole("Admin"))
        {
            filteredCases = cases.Where(c => c.Assignments != null && c.Assignments.Any(a =>
                a.IsActive &&
                a.AssignmentType == AssignmentType.Internal &&
                a.AssignedUserId == currentUserId));
        }
        // 3. Admin & LegalStaffAdmin pass through with access to all active cases

        var viewModels = filteredCases.Select(c =>
        {
            var activeAssignment = c.Assignments?.FirstOrDefault(a => a.IsActive);
            return new CaseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                DateCreated = c.DateCreated,
                TypeName = c.Type?.Name ?? "Unassigned",
                StatusName = c.Status?.Name ?? "Pending",
                HearingCount = c.Hearings?.Count ?? 0,
                DocumentCount = c.Documents?.Count ?? 0,
                AssignmentCount = c.Assignments?.Count ?? 0,
                CurrentAssignmentType = activeAssignment?.AssignmentType,
                AssignedLawFirmName = activeAssignment?.AssignedLawFirm?.Name,
                AssignedToUserName = activeAssignment?.AssignedUser?.FullName
            };
        }).ToList();

        return View(viewModels);
    }

    [Breadcrumb("Case Details", FromAction = nameof(Index))]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
    public async Task<IActionResult> Details(int id)
    {
        var legalCase = await _context.Cases
            .Include(c => c.Type)
            .Include(c => c.Status)
            .Include(c => c.Assignments)
                .ThenInclude(a => a.AssignedLawFirm)
            .Include(c => c.Assignments)
                .ThenInclude(a => a.AssignedUser)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (legalCase == null) return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdString, out Guid currentUserId);

        var activeAssignment = legalCase.Assignments.FirstOrDefault(a => a.IsActive);

        // Security Check: Law Firm Role
        if (User.IsInRole(RoleConstants.LawFirm))
        {
            var currentUser = await _userManager.FindByIdAsync(userIdString!);
            bool isAssignedToFirm = activeAssignment != null &&
                                   activeAssignment.AssignmentType == AssignmentType.External &&
                                   activeAssignment.AssignedLawFirmId == currentUser?.LawFirmId;

            if (!isAssignedToFirm)
            {
                return Forbid(); // Or RedirectToAction("AccessDenied", "Account");
            }
        }
        // Security Check: Legal Staff Role
        else if (User.IsInRole(RoleConstants.LegalStaff) && !User.IsInRole(RoleConstants.LegalStaffAdmin) && !User.IsInRole("Admin"))
        {
            bool isAssignedToUser = activeAssignment != null &&
                                    activeAssignment.AssignmentType == AssignmentType.Internal &&
                                    activeAssignment.AssignedUserId == currentUserId;

            if (!isAssignedToUser)
            {
                return Forbid();
            }
        }

        // Map CaseViewModel as usual...
        var model = new CaseViewModel
        {
            Id = legalCase.Id,
            Title = legalCase.Title,
            Description = legalCase.Description,
            DateCreated = legalCase.DateCreated,
            TypeName = legalCase.Type?.Name ?? "Unassigned",
            StatusName = legalCase.Status?.Name ?? "Pending",
            ActiveAssignmentId = activeAssignment?.Id,
            CurrentAssignmentType = activeAssignment?.AssignmentType,
            AssignedToUserId = activeAssignment?.AssignedUserId,
            AssignedToUserName = activeAssignment?.AssignedUser?.FullName,
            AssignedLawFirmId = activeAssignment?.AssignedLawFirmId,
            AssignedLawFirmName = activeAssignment?.AssignedLawFirm?.Name
        };

        // Populate dropdowns for Admins/LegalStaffAdmins
        if (User.IsInRole("Admin") || User.IsInRole(RoleConstants.LegalStaffAdmin))
        {
            model.AvailableLawFirms = await _context.LawFirms
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToListAsync();

            model.AvailableLegalStaff = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToListAsync();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
    public async Task<IActionResult> UploadDocument(UploadDocumentViewModel model)
    {
        if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
        {
            TempData["Error"] = "Invalid file or missing required data.";
            return RedirectToAction(nameof(Details), new { id = model.CaseId });
        }

        var caseEntity = await _caseRepo.FindById(model.CaseId);
        if (caseEntity == null) return NotFound();

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var originalFileName = Path.GetFileName(model.File.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
        var filePathOnDisk = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePathOnDisk, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out Guid currentUserId))
        {
            return Unauthorized();
        }

        var document = new Document
        {
            CaseId = model.CaseId,
            UploadedBy = currentUserId,
            FileName = originalFileName,
            FilePath = $"/uploads/documents/{uniqueFileName}",
            FileType = model.File.ContentType,
            UploadDate = DateTime.UtcNow,
            Description = model.Description
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Document uploaded successfully.";
        return RedirectToAction(nameof(Details), new { id = model.CaseId });
    }

    // POST: Case/AssignCase
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> AssignCase(
    int caseId,
    AssignmentType assignmentType,
    int? lawFirmId,
    Guid? assignedUserId,
    string? initialInstructions)
    {
        var legalCase = await _context.Cases.FindAsync(caseId);
        if (legalCase == null) return NotFound();

        // Validation based on assignment type
        if (assignmentType == AssignmentType.External && !lawFirmId.HasValue)
        {
            TempData["Error"] = "Please select a law firm for external assignment.";
            return RedirectToAction("Details", new { id = caseId });
        }

        if (assignmentType == AssignmentType.Internal && !assignedUserId.HasValue)
        {
            TempData["Error"] = "Please select a legal staff member for internal assignment.";
            return RedirectToAction("Details", new { id = caseId });
        }

        // Deactivate previous active assignments for this case
        var existingAssignments = await _context.CaseAssignments
            .Where(a => a.CaseId == caseId && a.IsActive)
            .ToListAsync();

        foreach (var assignment in existingAssignments)
        {
            assignment.IsActive = false;
        }

        // Build the new assignment record
        var newAssignment = new CaseAssignment
        {
            CaseId = caseId,
            AssignmentType = assignmentType,
            AssignedDate = DateTime.UtcNow,
            IsActive = true,
            InstructionsText = initialInstructions ?? "Case assigned."
        };

        if (assignmentType == AssignmentType.External)
        {
            var lawFirm = await _context.LawFirms.FindAsync(lawFirmId.Value);
            if (lawFirm == null) return NotFound();

            legalCase.LawFirmId = lawFirmId;
            newAssignment.AssignedLawFirmId = lawFirmId;
            newAssignment.AssignedUserId = null;

            _context.CaseAssignments.Add(newAssignment);
            await _context.SaveChangesAsync();

            // Send email notification to Law Firm
            if (!string.IsNullOrEmpty(lawFirm.Email))
            {
                try
                {
                    string subject = $"New External Case Assignment: {legalCase.Title}";
                    string body = $@"
                    <h3>Case Assignment Notification</h3>
                    <p>Dear {lawFirm.Name},</p>
                    <p>You have been assigned legal case: <strong>{legalCase.Title}</strong>.</p>
                    <p><strong>Initial Instructions:</strong> {initialInstructions ?? "None provided."}</p>
                    <p>Please log in to your dashboard to access case details.</p>";

                    await _emailService.SendEmailAsync(lawFirm.Email, subject, body);
                    TempData["Success"] = $"Case successfully assigned externally to {lawFirm.Name}!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send assignment email to law firm {LawFirmId}", lawFirmId);
                    TempData["Warning"] = "Case assigned successfully, but email dispatch failed.";
                }
            }
        }
        else // Internal Assignment
        {
            var staffUser = await _context.Users.FindAsync(assignedUserId.Value);
            if (staffUser == null) return NotFound();

            legalCase.LawFirmId = null; // Unlink external firm if switching to internal
            newAssignment.AssignedUserId = assignedUserId;
            newAssignment.AssignedLawFirmId = null;

            _context.CaseAssignments.Add(newAssignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Case successfully assigned internally to {staffUser.FullName}!";
        }

        return RedirectToAction("Details", new { id = caseId });
    }

    // POST: Case/UnassignCase
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin)]
    public async Task<IActionResult> UnassignCase(int caseId, int? assignmentId, string reason)
    {
        var legalCase = await _context.Cases
            .Include(c => c.LawFirm)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (legalCase == null) return NotFound();

        // Fetch active assignment
        var assignment = await _context.CaseAssignments
            .Include(a => a.AssignedLawFirm)
            .Include(a => a.AssignedUser)
            .FirstOrDefaultAsync(a => (assignmentId.HasValue && a.Id == assignmentId.Value) || (a.CaseId == caseId && a.IsActive));

        if (assignment != null)
        {
            assignment.IsActive = false; // Mark inactive instead of hard deleting to preserve historical records
        }

        var unassignedFirm = assignment?.AssignedLawFirm ?? legalCase.LawFirm;

        // Reset Case External LawFirm Link
        legalCase.LawFirmId = null;
        legalCase.LawFirm = null;

        await _context.SaveChangesAsync();

        // Dispatch recall email if it was previously assigned to an external firm
        if (unassignedFirm != null && !string.IsNullOrEmpty(unassignedFirm.Email))
        {
            try
            {
                string subject = $"Case Recall Notification: {legalCase.Title}";
                string body = $@"
                <h3>Case Unassignment Notification</h3>
                <p>Dear {unassignedFirm.Name},</p>
                <p>Please be informed that legal case <strong>{legalCase.Title}</strong> has been unassigned/recalled by Bank of Uganda Legal Department.</p>
                <p><strong>Reason / Remarks:</strong> {reason ?? "No additional remarks provided."}</p>
                <p>This case will no longer appear on your firm's portal dashboard.</p>";

                await _emailService.SendEmailAsync(unassignedFirm.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send recall email to law firm {LawFirmId}", unassignedFirm.Id);
            }
        }

        TempData["Success"] = "Case assignment successfully revoked.";
        return RedirectToAction("Details", new { id = caseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> SendInstruction(int caseId, int assignmentId, string instructionsText)
    {
        if (string.IsNullOrWhiteSpace(instructionsText))
        {
            TempData["Error"] = "Instructions cannot be empty.";
            return RedirectToAction(nameof(Details), new { id = caseId });
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdString, out Guid currentUserId);

        var instruction = new CaseInstruction
        {
            CaseAssignmentId = assignmentId,
            InstructionsText = instructionsText,
            DateSent = DateTime.UtcNow,
            SentById = currentUserId
        };

        _context.CaseInstructions.Add(instruction);
        await _context.SaveChangesAsync();

        // 1. Fetch assignment details along with LawFirm and Case info
        var assignment = await _context.CaseAssignments
            .Include(ca => ca.Case)
            .Include(ca => ca.AssignedLawFirm)
            .FirstOrDefaultAsync(ca => ca.Id == assignmentId);

        // 2. Dispatch email notification if assigned to a firm with a valid email
        if (assignment?.AssignedLawFirm != null && !string.IsNullOrEmpty(assignment.AssignedLawFirm.Email))
        {
            var caseTitle = assignment.Case?.Title ?? $"CASE-{caseId:D4}";
            var subject = $"New Instructions: {caseTitle}";

            var message = $@"
            <h3>Additional Case Instructions</h3>
            <p><strong>Case:</strong> {caseTitle}</p>
            <p><strong>Instructions Received:</strong></p>
            <blockquote style='border-left: 4px solid #0056b3; padding-left: 12px; margin-left: 0; color: #333;'>
                {instructionsText}
            </blockquote>
            <p>Please log in to your portal dashboard to acknowledge these instructions.</p>";

            try
            {
                await _emailService.SendEmailAsync(assignment.AssignedLawFirm.Email, subject, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send instruction email for Case Assignment ID {AssignmentId}", assignmentId);
            }
        }

        TempData["Success"] = "Additional instructions sent and notification emailed successfully.";
        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    [HttpGet]
    [Breadcrumb("Create New Case", FromAction = nameof(Index))]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
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
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> Create(CreateCaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.TypeOptions = await GetTypeSelectListAsync();
            model.StatusOptions = await GetStatusSelectListAsync();
            return View(model);
        }

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

    [HttpGet]
    [Breadcrumb("Edit Case", FromAction = nameof(Index))]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
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

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

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

    [HttpGet]
    [Breadcrumb("Delete Case", FromAction = nameof(Index))]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
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

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
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

    // POST: Case/Archive
    [HttpPost]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> ArchiveCase(int caseId)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Status)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseEntity == null) return NotFound();

        var closedStatus = await _context.CaseStatuses
            .FirstOrDefaultAsync(s => s.Name == "Closed");

        if (closedStatus != null)
        {
            caseEntity.StatusId = closedStatus.Id;
        }

        caseEntity.IsArchived = true;
        caseEntity.ArchivedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Case has been closed and archived successfully.";
        return RedirectToAction("Details", new { id = caseId });
    }

    // POST: Case/Unarchive
    [HttpPost]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> UnarchiveCase(int caseId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);

        if (caseEntity == null) return NotFound();

        var activeStatus = await _context.CaseStatuses
            .FirstOrDefaultAsync(s => s.Name == "Active");

        if (activeStatus != null)
        {
            caseEntity.StatusId = activeStatus.Id;
        }

        caseEntity.IsArchived = false;
        caseEntity.ArchivedDate = null;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Case has been retrieved from archives and reopened.";
        return RedirectToAction("Details", new { id = caseId });
    }

    // GET: Case/Archives
    [HttpGet]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> Archives(string? searchTerm)
    {
        var query = _context.Cases
            .Include(c => c.Status)
            .Include(c => c.Type)
            .Where(c => c.IsArchived)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Title.Contains(searchTerm));
        }

        var archivedCases = await query
            .OrderByDescending(c => c.ArchivedDate)
            .Select(c => new CaseViewModel
            {
                Id = c.Id,
                Title = c.Title,
                StatusName = c.Status != null ? c.Status.Name : "N/A",
                TypeName = c.Type != null ? c.Type.Name : "N/A",
                IsArchived = c.IsArchived
            })
            .ToListAsync();

        return View(archivedCases);
    }

    // POST: Case/AddPayment
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> AddPayment(int caseId, int paymentMilestoneId, decimal amount, string? description)
    {
        if (amount <= 0)
        {
            TempData["Error"] = "Please enter a valid payment amount.";
            return RedirectToAction("Details", new { id = caseId });
        }

        var payment = new Payment
        {
            CaseId = caseId,
            PaymentMilestoneId = paymentMilestoneId,
            Amount = amount,
            Description = description,
            PaymentDate = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Legal fee payment recorded successfully.";
        return RedirectToAction("Details", new { id = caseId });
    }

    // POST: Case/AddFinancialProvision
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> AddFinancialProvision(int caseId, string financialYear, decimal amount, string? justification)
    {
        if (amount <= 0 || string.IsNullOrWhiteSpace(financialYear))
        {
            TempData["Error"] = "Please provide a valid Financial Year and Amount.";
            return RedirectToAction("Details", new { id = caseId });
        }

        var provision = new FinancialProvision
        {
            CaseId = caseId,
            FinancialYear = financialYear.Trim(),
            Amount = amount,
            Justification = justification,
            Status = ProvisionStatus.Pending,
            DateLogged = DateTime.UtcNow
        };

        _context.FinancialProvisions.Add(provision);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Financial provision of {amount:N2} for {financialYear} logged successfully.";
        return RedirectToAction("Details", new { id = caseId });
    }

    // POST: Case/ScheduleHearing
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaffAdmin + "," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> ScheduleHearing(int caseId, DateTime hearingDate, string courtLocation, string judgeOrMagistrate, string? purpose)
    {
        if (hearingDate < DateTime.UtcNow.Date)
        {
            TempData["Error"] = "Hearing date cannot be in the past.";
            return RedirectToAction("Details", new { id = caseId });
        }

        var hearing = new Hearing
        {
            CaseId = caseId,
            HearingDate = hearingDate,
            CourtLocation = courtLocation,
            JudgeOrMagistrate = judgeOrMagistrate,
            Purpose = purpose,
            Status = HearingStatus.Scheduled
        };

        _context.Hearings.Add(hearing);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Hearing scheduled successfully.";
        return RedirectToAction("Details", new { id = caseId });
    }

    private async Task<IEnumerable<SelectListItem>> GetTypeSelectListAsync()
    {
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
        return await _context.CaseStatuses
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();
    }

    [HttpGet]
    [Authorize(Roles = RoleConstants.LawFirm)]
    public async Task<IActionResult> AssignedCases()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Load the logged-in counsel user to retrieve their LawFirmId
        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id.ToString() == userIdStr);

        if (currentUser?.LawFirmId == null)
        {
            return Forbid();
        }

        // Mirror the exact query used in Dashboard()
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