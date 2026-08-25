using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Template.Core.Models.Document;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Web.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Document/Upload?caseId=5
        [HttpGet]
        public IActionResult Upload(int caseId)
        {
            var model = new UploadDocumentViewModel
            {
                CaseId = caseId
            };
            return View(model);
        }

        // POST: Document/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadDocumentViewModel model)
        {
            // 1. Allowed file extensions
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a valid file.");
                return View(model);
            }

            var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("File", "Only PDF, Word (.doc, .docx), and Excel (.xls, .xlsx) files are allowed.");
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            // 2. Physical File Saving
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

            // 3. User Extraction
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid currentUserId))
            {
                return Unauthorized();
            }

            // 4. Database Record
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
            return RedirectToAction("Details", "Case", new { id = model.CaseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int caseId)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                TempData["Error"] = "Document not found.";
                return RedirectToAction("Details", "Case", new { id = caseId });
            }

            // 1. Delete the physical file from wwwroot if it exists
            if (!string.IsNullOrEmpty(document.FilePath))
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            // 2. Remove record from database
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Document deleted successfully.";
            return RedirectToAction("Details", "Case", new { id = caseId });
        }
    }
}
