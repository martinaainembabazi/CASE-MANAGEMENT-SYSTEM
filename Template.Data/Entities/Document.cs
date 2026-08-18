using Template.Common.AuditColumn;

namespace Template.Data.Entities
{
    public class Document : AuditableEntity
    {
        public int Id { get; set; }

        public int CaseId { get; set; }
        public Case Case { get; set; } = null!;

        public Guid UploadedBy { get; set; }
        public ApplicationUser UploadedByUser { get; set; } = null!;

        public required string FileName { get; set; }

        public required string FilePath { get; set; }

        public string? FileType { get; set; }

        public DateTime UploadDate { get; set; }

        public string? Description { get; set; }
    }
}