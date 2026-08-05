using Template.Core.Repository.ApplicationPermission;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace Template.Core.TagHelpers;

[HtmlTargetElement("permission")]
public class PermissionTagHelper(
    IApplicationPermissionRepository _permissionService,
    IHttpContextAccessor _httpContextAccessor) : TagHelper
{
    //private readonly IApplicationPermissionRepository _permissionRepo = permissionRepo;
    //private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    [HtmlAttributeName("name")]
    public string Permission { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Permission))
        {
            output.SuppressOutput();
            return;
        }

        var user = _httpContextAccessor.HttpContext.User;
        if (!user.Identity.IsAuthenticated)
        {
            output.SuppressOutput();
            return;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            output.SuppressOutput();
            return;
        }

        var hasPermission = await _permissionService.UserHasPermission(userId, Permission);

        if (!hasPermission)
        {
            output.SuppressOutput();
        }
        else
        {
            // Remove the permission element but keep its contents
            output.TagName = null;
        }
    }
}

// For permission groups
//[HtmlTargetElement(Attributes = "permission-group")]
//public class PermissionGroupTagHelper (IApplicationPermissionRepository _permissionRepo
//    , IHttpContextAccessor _httpContextAccessor) : TagHelper
//{

//    [HtmlAttributeName("permission-group")]
//    public string PermissionGroup { get; set; }

//    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
//    {
//        if (string.IsNullOrEmpty(PermissionGroup))
//        {
//            output.SuppressOutput();
//            return;
//        }

//        var user = _httpContextAccessor.HttpContext.User;
//        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

//        if (string.IsNullOrEmpty(userId))
//        {
//            output.SuppressOutput();
//            return;
//        }

//        var userPermissions = await _permissionRepo.GetAllUserPermissionsAsync(userId);
//        var hasAnyPermissionInGroup = userPermissions.Any(p => p.StartsWith(PermissionGroup));

//        if (!hasAnyPermissionInGroup)
//        {
//            output.SuppressOutput();
//        }
//    }
//}
