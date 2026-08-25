using Microsoft.AspNetCore.Mvc.Rendering;
using Template.Core.Models.Document;
using System;
using System.Collections.Generic;

namespace Template.Core.Models.Cases;

public class CaseViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DateCreated { get; set; }

    // Status & Type
    public int TypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // Creator & Archive Flag
    public Guid CreatedBy { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime? ArchivedDate { get; set; }
    public DateTime? DateClosed { get; set; }

    // Counts for UI summary
    public int HearingCount { get; set; }
    public int DocumentCount { get; set; }
    public int AssignmentCount { get; set; }

    // Current Assignment Info
    public int? ActiveAssignmentId { get; set; }
    public int? AssignedLawFirmId { get; set; }
    public string? AssignedLawFirmName { get; set; }
    public string? AssignedLawyerId { get; set; }
    public string? AssignedLawyerName { get; set; }

    public List<PaymentItemViewModel> Payments { get; set; } = new();
    public decimal TotalPayments => Payments.Sum(p => p.Amount);
    public List<FinancialProvisionItemViewModel> FinancialProvisions { get; set; } = new();
    public decimal TotalProvisionedAmount => FinancialProvisions.Sum(p => p.Amount);
    public List<HearingItemViewModel> Hearings { get; set; } = new();

    // Dropdowns for assignment
    public IEnumerable<SelectListItem>? AvailableLawFirms { get; set; }
    public IEnumerable<SelectListItem>? AvailableLawyers { get; set; }

    // List of uploaded documents for rendering
    public IEnumerable<DocumentItemViewModel> Documents { get; set; } = new List<DocumentItemViewModel>();

    // History of instructions sent for the current assignment
    public IEnumerable<InstructionItemViewModel> InstructionHistory { get; set; } = new List<InstructionItemViewModel>();
}

public class InstructionItemViewModel
{
    public int Id { get; set; }
    public string InstructionsText { get; set; } = string.Empty;
    public DateTime DateSent { get; set; }
    public string SentByName { get; set; } = string.Empty;
}

public class PaymentItemViewModel
{
    public int Id { get; set; }
    public string MilestoneName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Description { get; set; }
}

public class FinancialProvisionItemViewModel
{
    public int Id { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Justification { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DateLogged { get; set; }
}

public class HearingItemViewModel
{
    public int Id { get; set; }
    public DateTime HearingDate { get; set; }
    public string CourtLocation { get; set; } = string.Empty;
    public string JudgeOrMagistrate { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string? Outcome { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsUpcoming => HearingDate >= DateTime.UtcNow && Status == "Scheduled";
}