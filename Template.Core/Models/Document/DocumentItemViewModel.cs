using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Models.Document
{
    public class DocumentItemViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public DateTime UploadDate { get; set; }
        public string? Description { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
    }
}
