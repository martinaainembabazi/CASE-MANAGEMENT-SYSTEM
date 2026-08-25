using Microsoft.AspNetCore.Mvc.Rendering;
using Template.Data.Entities;

namespace Template.Core.Models.Cases
{
    internal class CaseDetailsViewModel
    {
        public Case Case { get; set; } = null!;
        public CaseAssignment? CurrentAssignment { get; set; }

        // Binding fields for the modal/form
        public int SelectedLawFirmId { get; set; }
        public IEnumerable<SelectListItem> LawFirms { get; set; } = new List<SelectListItem>();
    }
}
