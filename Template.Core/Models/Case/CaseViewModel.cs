using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public DateTime? DateClosed { get; set; }

    // Counts for UI summary
    public int HearingCount { get; set; }
    public int DocumentCount { get; set; }
    public int AssignmentCount { get; set; }
}
