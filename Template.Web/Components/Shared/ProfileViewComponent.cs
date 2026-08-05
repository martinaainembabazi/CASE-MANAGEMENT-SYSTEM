using System.Text;
using Template.Core.Repository.Accounts;
using Microsoft.AspNetCore.Mvc;

public class ProfileViewComponent(IAccountRepository _accountRepo) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {

        //if (User.Identity.IsAuthenticated)
        //{
        //    var user = await _accountRepo.FindByName(User?.Identity?.Name);
        //    var userRoles = await _accountRepo.GetApplicationUserRoles(user);
        //    var userCostCenters = await _userCostCenterRepo.FindByUser(user?.Id);

        //    var orgNamesBuilder = new StringBuilder();
        //    var orgIdStrings = userCostCenters?.OrganizationIds?
        //        .Split(",", StringSplitOptions.RemoveEmptyEntries);

        //    if (orgIdStrings != null)
        //    {
        //        foreach (var orgIdStr in orgIdStrings)
        //        {
        //            if (int.TryParse(orgIdStr.Trim(), out int orgId))
        //            {
        //                var organization = await _organizationRepo.FindById(orgId);
        //                if (organization != null)
        //                {
        //                    orgNamesBuilder.Append(organization.Name).Append(", ");
        //                }
        //            }
        //        }
        //    }
        //    var userData = new ProfileViewModel
        //    {
        //        Title = user?.Title,
        //        Roles = string.Join(", ", userRoles),
        //        CostCenters = orgNamesBuilder.ToString(),
        //    };

        //    return View(userData);
        //}
        return View();
    }
}