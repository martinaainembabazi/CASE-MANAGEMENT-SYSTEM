using AutoMapper;
using Template.Core.Models.AuditLogs;
using Template.Core.Repository.AuditLogs;
using Template.Core.Services.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;

/*namespace Template.Web.Controllers
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
}*/

namespace Template.Web.Controllers;

public class AuditLogController(
    IMapper _mapper,
    ILogger<AuditLogController> _logger,
    IAuditLogRepository _auditLogRepo
    ) : Controller
{
    //[RequirePermission(SystemPermissions.AuditTrail.ViewAuditLogs)]
    [Breadcrumb("Audit Trail", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public async Task<IActionResult> Index()
    {
        var logs = await _auditLogRepo.FindAll();
        
        var model = _mapper.Map<IEnumerable<ApplicationAuditLogViewModel>>(logs); // to map entities to my view model

        var logViewModels = logs
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new ApplicationAuditLogViewModel
            {
                Id = x.Id,
                LogEntryId = x.LogEntryId,
                UserId = x.UserId,
                Username = x.Username,
                OperationPerformed = x.OperationPerformed,
                SourceIP = x.SourceIP,
                DestinationIP = x.DestinationIP,
                SourceName = x.SourceName,
                DestinationName = x.DestinationName,
                AffectedEntityType = x.AffectedEntityType,
                AffectedEntityId = x.AffectedEntityId,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                ErrorMessage = x.ErrorMessage,
                RequestData = x.RequestData,
                ResponseData = x.ResponseData,
                SessionId = x.SessionId,
                UserAgent = x.UserAgent,
                ActionDetails = x.ActionDetails,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy
            }).ToList();

        return View(logViewModels);
    }
}
