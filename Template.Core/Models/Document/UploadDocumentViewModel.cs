using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.Document
{
    public class UploadDocumentViewModel
    {
        public int CaseId { get; set; }

        [Required(ErrorMessage = "Please select a file.")]
        public IFormFile File { get; set; } = null;

        [Display(Name = "Description / Remarks")]
        public string? Description { get; set; }
    }
}
