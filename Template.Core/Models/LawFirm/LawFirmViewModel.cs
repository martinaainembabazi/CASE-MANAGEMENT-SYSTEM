using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.LawFirm;

public class LawFirmViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Law firm name is required.")]
    [Display(Name = "Firm Name")]
    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }

    public string? Phone { get; set; }

    [Required(ErrorMessage = "Contract start date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Contract Start Date")]
    public DateTime ContractStartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Contract End Date")]
    public DateTime? ContractEndDate { get; set; }

    public string Status { get; set; } = "Active";

    // Summary counters for UI cards and tables
    [Display(Name = "Lawyers")]
    public int TotalLawyersCount { get; set; }

    [Display(Name = "Assigned Cases")]
    public int TotalCaseAssignmentsCount { get; set; }
}