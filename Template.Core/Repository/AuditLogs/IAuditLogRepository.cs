using Template.Data.Entities;

namespace Template.Core.Repository.AuditLogs
{
    public interface IAuditLogRepository
    {
        Task<ICollection<AuditLog>> FindAll();
    }
}
