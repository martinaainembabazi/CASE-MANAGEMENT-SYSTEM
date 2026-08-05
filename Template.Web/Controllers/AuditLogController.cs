using AutoMapper;
using Template.Core.Models.AuditLogs;
using Template.Core.Repository.AuditLogs;
using Template.Core.Services.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;

namespace Template.Web.Controllers
{
    public class AuditLogController(IMapper _mapper, IAuditLogRepository _auditLogRepo, ILogger<AuditLogController> _logger) : Controller
    {
        [Breadcrumb("Audit Logs", FromAction = nameof(Index), FromController = typeof(HomeController))]
        [RequirePermission(SystemPermissions.AuditLog.ViewAuditLogs)]
        public async Task<IActionResult> Index()
        {
            var allLogs = await _auditLogRepo.FindAll();
            var model = _mapper.Map<ICollection<ApplicationAuditLogViewModel>>(allLogs);
            return View(model);
        }
    }
}
