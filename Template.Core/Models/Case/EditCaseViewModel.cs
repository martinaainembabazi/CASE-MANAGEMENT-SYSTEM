using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Template.Core.Models.Cases;

public class EditCaseViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Case Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Please select a Case Type.")]
    [Display(Name = "Case Type")]
    public int TypeId { get; set; }

    [Required(ErrorMessage = "Please select a Case Status.")]
    [Display(Name = "Case Status")]
    public int StatusId { get; set; }

    public bool IsArchived { get; set; }

    // Dropdown options
    public IEnumerable<SelectListItem>? TypeOptions { get; set; }
    public IEnumerable<SelectListItem>? StatusOptions { get; set; }
}
