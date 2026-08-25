using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System.Security.Claims;
using Template.Core.Models.Cases;
using Template.Core.Models.Document;
using Template.Core.Repository.Cases;
using Template.Data.Configurations;
using Template.Data.Entities;
using Template.Core.Services;

namespace Template.Web.Controllers;

[Authorize]
public class CaseController(
    ILogger<CaseController> _logger,
    ICaseRepository _caseRepo,
    IMapper _mapper,
    IEmailService _emailService,
    ApplicationDbContext _context
    ) : Controller
{
    [HttpGet]
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
    [Authorize(Roles = RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
    public async Task<IActionResult> Index()
    {
        var cases = await _caseRepo.FindAll();

        // Filter out archived cases so only active cases are shown
        var viewModels = cases
            .Where(c => !c.IsArchived)
            .Select(c => new CaseViewModel
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
    [Authorize(Roles = RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
    public async Task<IActionResult> Details(int id)
    {
        var caseEntity = await _caseRepo.FindById(id);
        if (caseEntity == null)
        {
            return NotFound();
        }

        var lawFirms = await _context.LawFirms
            .Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name
            })
            .ToListAsync();

        var activeAssignment = await _context.CaseAssignments
            .Include(a => a.AssignedLawFirm)
            .Include(a => a.Instructions)
                .ThenInclude(i => i.SentBy)
            .Where(a => a.CaseId == id)
            .OrderByDescending(a => a.AssignedDate)
            .FirstOrDefaultAsync();

        // Fetch milestones dropdown list 
        ViewBag.Milestones = await _context.PaymentMilestones.ToListAsync();

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
            AssignmentCount = caseEntity.Assignments?.Count ?? 0,

            ActiveAssignmentId = activeAssignment?.Id,
            AssignedLawFirmId = activeAssignment?.AssignedLawFirmId,
            AssignedLawFirmName = activeAssignment?.AssignedLawFirm?.Name ?? "N/A",
            AvailableLawFirms = lawFirms,

            InstructionHistory = activeAssignment?.Instructions?
                .OrderByDescending(i => i.DateSent)
                .Select(i => new InstructionItemViewModel
                {
                    Id = i.Id,
                    InstructionsText = i.InstructionsText,
                    DateSent = i.DateSent,
                    SentByName = i.SentBy?.UserName ?? "System"
                }).ToList() ?? new List<InstructionItemViewModel>(),

            Documents = caseEntity.Documents?.Select(d => new DocumentItemViewModel
            {
                Id = d.Id,
                FileName = d.FileName,
                FilePath = d.FilePath,
                FileType = d.FileType,
                UploadDate = d.UploadDate,
                Description = d.Description,
                UploadedByName = d.UploadedByUser?.UserName ?? "System"
            }).ToList() ?? new List<DocumentItemViewModel>()
        };

        // Fetch payments recorded for this case
        viewModel.Payments = await _context.Payments
            .Include(p => p.PaymentMilestone)
            .Where(p => p.CaseId == id)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentItemViewModel
            {
                Id = p.Id,
                MilestoneName = p.PaymentMilestone != null ? p.PaymentMilestone.Name : "Unspecified",
                Amount = p.Amount,
                PaymentDate = p.PaymentDate,
                Description = p.Description
            })
            .ToListAsync();

        // Fetch financial provisions recorded for this case
        viewModel.FinancialProvisions = await _context.FinancialProvisions
            .Where(p => p.CaseId == id)
            .OrderByDescending(p => p.DateLogged)
            .Select(p => new FinancialProvisionItemViewModel
            {
                Id = p.Id,
                FinancialYear = p.FinancialYear,
                Amount = p.Amount,
                Justification = p.Justification,
                Status = p.Status.ToString(),
                DateLogged = p.DateLogged
            })
            .ToListAsync();

        viewModel.Hearings = await _context.Hearings
    .Where(h => h.CaseId == id)
    .OrderBy(h => h.HearingDate)
    .Select(h => new HearingItemViewModel
    {
        Id = h.Id,
        HearingDate = h.HearingDate,
        CourtLocation = h.CourtLocation,
        JudgeOrMagistrate = h.JudgeOrMagistrate,
        Purpose = h.Purpose,
        Outcome = h.Outcome,
        Status = h.Status.ToString()
    })
    .ToListAsync();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.LegalStaff + "," + RoleConstants.LawFirm)]
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

    // 1. Assign Case to Law Firm
    [HttpPost]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> AssignLawFirm(int caseId, int lawFirmId, string? initialInstructions)
    {
        var legalCase = await _context.Cases.FindAsync(caseId);
        var lawFirm = await _context.LawFirms.FindAsync(lawFirmId);

        if (legalCase == null || lawFirm == null)
        {
            return NotFound();
        }

        // 1. Update LawFirmId on Case
        legalCase.LawFirmId = lawFirmId;

        // 2. Create the CaseAssignment record so the dashboard/card recognizes active status
        var newAssignment = new CaseAssignment
        {
            CaseId = caseId,
            AssignedLawFirmId = lawFirmId,
            AssignedDate = DateTime.UtcNow, // Adjust property name if using 'AssignedDate' or 'CreatedDate'
            InstructionsText = initialInstructions ?? "Case assigned."
        };

        _context.CaseAssignments.Add(newAssignment);

        // Save changes to generate assignment ID and link entities
        await _context.SaveChangesAsync();

        // 3. Dispatch Email Notification
        if (!string.IsNullOrEmpty(lawFirm.Email))
        {
            try
            {
                string subject = $"New Case Assignment: {legalCase.Title}";
                string body = $@"
                <h3>Case Assignment Notification</h3>
                <p>Dear {lawFirm.Name},</p>
                <p>You have been assigned legal case: <strong>{legalCase.Title}</strong>.</p>
                <p><strong>Initial Instructions:</strong> {initialInstructions ?? "None provided."}</p>
                <p>Please log in to your dashboard to access case details.</p>";

                await _emailService.SendEmailAsync(lawFirm.Email, subject, body);
                TempData["Success"] = "Case successfully assigned to law firm!";
            }
            catch (Exception)
            {
                TempData["Warning"] = "Case assigned successfully, but email dispatch failed.";
            }
        }

        return RedirectToAction("Details", new { id = caseId });
    }

    // 2. Unassign Case from Law Firm
    [HttpPost]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> UnassignLawFirm(int caseId, int? assignmentId, string reason)
    {
        // 1. Fetch case record
        var legalCase = await _context.Cases
            .Include(c => c.LawFirm)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (legalCase == null)
        {
            return NotFound();
        }

        // 2. Fetch the assignment record
        var assignment = await _context.CaseAssignments
            .Include(a => a.AssignedLawFirm)
            .FirstOrDefaultAsync(a => (assignmentId.HasValue && a.Id == assignmentId.Value) || a.CaseId == caseId);

        // Determine target firm for email before removing assignment
        var unassignedFirm = assignment?.AssignedLawFirm ?? legalCase.LawFirm;

        // 3. Remove the assignment record
        if (assignment != null)
        {
            _context.CaseAssignments.Remove(assignment);
        }

        // 4. Clear LawFirm link on Case entity
        legalCase.LawFirmId = null;
        legalCase.LawFirm = null;

        await _context.SaveChangesAsync();

        // 5. Send notification email using the 'reason' parameter from the view modal
        if (unassignedFirm != null && !string.IsNullOrEmpty(unassignedFirm.Email))
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

        TempData["Success"] = "Case successfully unassigned from the law firm.";
        return RedirectToAction("Details", new { id = caseId });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.LegalStaff)]
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

        TempData["Success"] = "Additional instructions sent.";
        return RedirectToAction(nameof(Details), new { id = caseId });
    }

    [HttpGet]
    [Breadcrumb("Create New Case", FromAction = nameof(Index))]
    [Authorize(Roles = RoleConstants.LegalStaff)]
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
    [Authorize(Roles = RoleConstants.LegalStaff)]
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
    [Authorize(Roles = RoleConstants.LegalStaff)]
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
    [Authorize(Roles = RoleConstants.LegalStaff)]
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
    [Authorize(Roles = RoleConstants.LegalStaff)]
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
    [Authorize(Roles = RoleConstants.LegalStaff)]
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
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> ArchiveCase(int caseId)
    {
        var caseEntity = await _context.Cases
            .Include(c => c.Status)
            .FirstOrDefaultAsync(c => c.Id == caseId);

        if (caseEntity == null) return NotFound();

        // Fetch the "Closed" status entity from database
        var closedStatus = await _context.CaseStatuses
            .FirstOrDefaultAsync(s => s.Name == "Closed");

        if (closedStatus != null)
        {
            caseEntity.StatusId = closedStatus.Id;
        }

        // Set archival properties
        caseEntity.IsArchived = true;
        caseEntity.ArchivedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Case has been closed and archived successfully.";
        return RedirectToAction("Details", new { id = caseId });
    }

    // POST: Case/Unarchive
    [HttpPost]
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
    public async Task<IActionResult> UnarchiveCase(int caseId)
    {
        var caseEntity = await _context.Cases.FindAsync(caseId);

        if (caseEntity == null) return NotFound();

        // Optionally reset status back to Active upon retrieval
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
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
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
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
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
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
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
    [Authorize(Roles = "Admin," + RoleConstants.LegalStaff)]
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


}