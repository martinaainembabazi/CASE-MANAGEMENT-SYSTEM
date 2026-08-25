using Template.Core.Repository.ApplicationPermission;
using Template.Data.Configurations;
using Template.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Template.Core.Repository.AuditLogs
{
    public class AuditLogRepository (ApplicationDbContext _db, ILogger<ApplicationPermissionRepository> _logger) : IAuditLogRepository
    {
        public async Task<ICollection<AuditLog>> FindAll()
        {
            return await _db.AuditLogs
                .Include(a => a.User)
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

    }
}
