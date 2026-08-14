using AutoMapper;
using Template.Core.Repository.Accounts;
using Template.Core.Repository.Roles;
using Template.Core.Services.AdAuthentication;
using Template.Core.Services.Authorization;
using Template.Data.Entities;
using Template.Web.MyModels;
using Microsoft.AspNetCore.Mvc;
using SmartBreadcrumbs.Attributes;

namespace Template.Web.Controllers;

public class AccountController(
    IMapper _mapper,
    ILogger<AccountController> _logger,
    IAdAuthenticationService _adAuthService,
    IAuthService _authService,
    IAccountRepository _accountRepo,
    IRoleRepository _roleRepo
    ) : Controller
{
    [RequirePermission(SystemPermissions.Account.ViewApplicationUsers)]
    [Breadcrumb("User Accounts", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public async Task<IActionResult> Index()
    {
        var users = await _accountRepo.FindAll();
        var userViewModels = _mapper.Map<List<ApplicationUserViewModel>>(users);

        var pageViewModel = new ApplicationUserListPageViewModel
        {
            Users = userViewModels
        };

        return View(pageViewModel);
    }

    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
    {
        var result = await _authService.ValidateApplicationUser(username, password);

        if (!result.Success)
        {
            ViewData["status"] = result.Status;
            ModelState.AddModelError("", $"Failed login. {result.Status}");
            return View();
        }

        await _authService.SignInApplicationUser(result.User);
        _logger.LogInformation("Login successful for user: {Username}", username);

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    [Breadcrumb("Create", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Account.CreateApplicationUser)]
    public IActionResult Create()
    {
        var model = new ApplicationUserViewModel
        {
            Id = null
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.CreateApplicationUser)]
    public async Task<IActionResult> Create(ApplicationUserViewModel model)
    {
        var adUserResult = _adAuthService.IsExistsOnAd(model);

        if (!adUserResult.IsSuccess)
        {
            switch (adUserResult.ResultType)
            {
                case AdResultTypes.ServerUnavailable:
                    ModelState.AddModelError("", "The Active Directory server is currently unavailable. Please contact administrator.");
                    _logger.LogError("Active Directory server unavailable.");
                    break;

                case AdResultTypes.UserNotFound:
                    ModelState.AddModelError("UserName", "Username does not exist in Active Directory.");
                    _logger.LogWarning("Attempted to create user with non-existent AD username: {Username}", model.UserName);
                    break;

                default:
                    ModelState.AddModelError("", $"Error validating username: {adUserResult.ErrorMessage}");
                    _logger.LogError(adUserResult.Exception, "Error checking AD for username {Username}: {Message}",
                        model.UserName, adUserResult.ErrorMessage);
                    break;
            }

            return View(model);
        }

        // Check if user already exists in our database
        var existingUser = await _accountRepo.FindByName(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "A user with this username already exists in the system.");
            return View(model);
        }

        var result = await _accountRepo.Create(adUserResult.AppUser);

        if (result)
        {
            TempData["SuccessMessage"] = $"User {model.UserName} was created successfully.";
            _logger.LogInformation($"User {model.UserName} was created successfully.");
            return RedirectToAction("Index");
        }
        else
        {
            ModelState.AddModelError("", "Failed to create user. Please contact administrator.");
            _logger.LogError("Failed to create user.");
            return View(model);
        }
    }

    [Breadcrumb("Edit", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Update(string userId)
    {
        var result = await _accountRepo.FindById(userId);

        var user = _mapper.Map<ApplicationUserViewModel>(result);
        return View("Create", user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Update(ApplicationUserViewModel model)
    {
        var userInfo = await _accountRepo.FindById(model.Id);

        if (!model.IsActive)
        {
            userInfo.DisableDate = DateTime.UtcNow;
        }
        else
        {
            userInfo.DisableDate = null;
        }

        var user = _mapper.Map<ApplicationUser>(model);
        var result = await _accountRepo.Update(user);

        if (result)
        {
            TempData["updated"] = "true";

            if (model.IsActive)
                _logger.LogInformation($"User account {model.UserName} has been activated.");
            else
                _logger.LogInformation($"User account {model.UserName} has been deactivated and disabled.");

            return RedirectToAction("Index");
        }
        else
        {
            TempData["updated"] = "false";
            _logger.LogError($"Failed to update user details for {model.UserName}. Please contact Administrator");
            return RedirectToAction("Index");
        }
    }

    [Breadcrumb("Manage Roles", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Roles.ViewRoles)]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User ID is required.";
            return RedirectToAction("Index");
        }

        var user = await _accountRepo.FindById(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = $"User with ID {userId} was not found.";
            return RedirectToAction("Index");
        }

        var allRoles = await _roleRepo.FindAll();
        var userRoleNames = await _accountRepo.GetApplicationUserRoles(user);

        var userRoleSet = new HashSet<string>(
            userRoleNames ?? new List<string>(),
            StringComparer.OrdinalIgnoreCase
        );

        var model = new ApplicationUserRolesViewModel
        {
            UserId = userId,
            UserName = user.UserName,
            Roles = new List<ApplicationUserRoleViewModel>()
        };

        if (allRoles != null && allRoles.Any())
        {
            foreach (var role in allRoles.OrderBy(r => r.Name))
            {
                model.Roles.Add(new ApplicationUserRoleViewModel
                {
                    RoleId = role.Id.ToString(),
                    RoleName = role.Name ?? "(Unnamed Role)",
                    IsSelected = !string.IsNullOrEmpty(role.Name) && userRoleSet.Contains(role.Name)
                });
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Roles.EditRole)]
    public async Task<IActionResult> ManageRoles(ApplicationUserRolesViewModel model)
    {
        var user = await _accountRepo.FindById(model.UserId);
        if (user == null)
            return NotFound();

        var userRoles = await _accountRepo.GetApplicationUserRoles(user);

        // Remove roles that are no longer selected
        foreach (var role in userRoles)
        {
            if (!model.Roles.Any(r => r.IsSelected && _roleRepo.FindById(r.RoleId).Result.Name == role))
            {
                await _accountRepo.RemoveApplicationUserFromRole(user, role);

            }
        }


        foreach (var role in model.Roles.Where(r => r.IsSelected))
        {
            var roleName = (await _roleRepo.FindById(role.RoleId)).Name;
            if (!userRoles.Contains(roleName))
            {
                await _accountRepo.AddApplicationUserFromRole(user, roleName);
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> LogoutAsync(string returnUrl = null)
    {
        await _authService.SignOutApplicationUser();
        HttpContext.Session.Clear();
        _logger.LogInformation("Logout successful");
        return RedirectToAction("Login", "Account", new { ReturnUrl = returnUrl });
    }
}
