using System.ComponentModel.DataAnnotations;

namespace Template.Common.AuditColumn;
public class AuditableEntity : IAuditableEntity
{
	public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
	public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

	[Display(Name = "Creator")]
	public string CreatedBy { get; set; } = "system";

	[Display(Name = "Modifier")]
	public string ModifiedBy { get; set; } = "system";
}
