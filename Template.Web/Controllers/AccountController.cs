using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using Template.Core.Repository.Accounts;
using Template.Core.Repository.AuditLogs;
using Template.Core.Repository.Roles;
using Template.Core.Services.AdAuthentication;
using Template.Core.Services.Authorization;
using Template.Data.Entities;
using Template.Web.MyModels;

// Commented out the permissions to gain access to the pages

namespace Template.Web.Controllers;

public class AccountController(
    IMapper _mapper,
    ILogger<AccountController> _logger,
    IAdAuthenticationService _adAuthService,
    IAuthService _authService,
    IAccountRepository _accountRepo,
    IRoleRepository _roleRepo,
   UserManager<ApplicationUser> _userManager,
    RoleManager<IdentityRole<Guid>> _roleManager
    ) : Controller
{

    //[RequirePermission(SystemPermissions.Account.ViewApplicationUsers)]
    [Breadcrumb("User Accounts", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public async Task<IActionResult> Index()
    {
        var users = await _accountRepo.FindAll();
        var userViewModels = _mapper.Map<List<ApplicationUserViewModel>>(users);

        var pageViewModel = new ApplicationUserListPageViewModel
        {
            Users = userViewModels
        };

        return View(userViewModels);
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
        //_logger.LogInformation("Login successful for user: {Username}", username);

        if (Url.IsLocalUrl(returnUrl))
        {
            _logger.LogInformation("Login successful for user: {Username}", username);
            return Redirect(returnUrl);
        }
        else
        {
            _logger.LogInformation("Login successful for user: {Username}", username);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    [HttpGet]
    [Breadcrumb("Create", FromAction = nameof(Index))]
    //[RequirePermission(SystemPermissions.Account.CreateApplicationUser)]
    public IActionResult Create()
    {
        var model = new CreateViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[RequirePermission(SystemPermissions.Account.CreateApplicationUser)]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Map to ApplicationUserViewModel expected by _adAuthService
        var appUserVm = new ApplicationUserViewModel
        {
            UserName = model.UserName
        };

        /*var adUserResult = _adAuthService.IsExistsOnAd(appUserVm);

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
        }*/

        //dummy test user creation
        var dummyAdUser = new ApplicationUser
        {
            UserName = model.UserName,
            Email = $"{model.UserName}@domain.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        // Check if user already exists in database
        var existingUser = await _accountRepo.FindByName(model.UserName);
        if (existingUser != null)
        {
            ModelState.AddModelError("UserName", "A user with this username already exists in the system.");
            return View(model);
        }

        //var result = await _accountRepo.Create(adUserResult.AppUser);
        var result = await _accountRepo.Create(dummyAdUser);

        if (result)
        {
            TempData["SuccessMessage"] = $"User {model.UserName} was created successfully.";
            _logger.LogInformation("User {UserName} was created successfully.", model.UserName);
            return RedirectToAction(nameof(Index));
        }
        else
        {
            ModelState.AddModelError("", "Failed to create user. Please contact administrator.");
            _logger.LogError("Failed to create user {UserName}.", model.UserName);
            return View(model);
        }
    }

    [HttpGet]
    [Breadcrumb("Edit User", FromAction = nameof(Index))]
    //[RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Update(string id)
    {
        var userEntity = await _accountRepo.FindById(id);
        if (userEntity == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<ApplicationUserViewModel>(userEntity);

        // Fetch user's currently assigned roles
        var userRoles = await _userManager.GetRolesAsync(userEntity);
        model.SelectedRoles = userRoles.ToList();

        // Fix: Execute ToListAsync() on EF Core Roles query FIRST, then project to SelectListItem
        var allRoles = await _roleManager.Roles.ToListAsync();

        model.RoleOptions = allRoles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name,
            Selected = userRoles.Contains(r.Name!)
        }).ToList();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    //[RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Update(ApplicationUserViewModel model)
    {
        var userInfo = await _accountRepo.FindById(model.Id);
        if (userInfo == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        // Toggle active / disable state
        if (!model.IsActive)
        {
            userInfo.DisableDate = DateTime.UtcNow;
        }
        else
        {
            userInfo.DisableDate = null;
        }

        // Map modified fields into domain entity
        _mapper.Map(model, userInfo);

        var result = await _accountRepo.Update(userInfo);

        if (result)
        {
            // --- Sync Assigned System Roles ---
            var currentRoles = await _userManager.GetRolesAsync(userInfo);
            var selectedRoles = model.SelectedRoles ?? new List<string>();

            // Remove unselected roles
            var rolesToRemove = currentRoles.Except(selectedRoles);
            await _userManager.RemoveFromRolesAsync(userInfo, rolesToRemove);

            // Add newly selected roles
            var rolesToAdd = selectedRoles.Except(currentRoles);
            await _userManager.AddToRolesAsync(userInfo, rolesToAdd);

            TempData["SuccessMessage"] = $"User account {model.UserName} updated successfully.";

            if (model.IsActive)
                _logger.LogInformation("User account {UserName} updated and activated.", model.UserName);
            else
                _logger.LogInformation("User account {UserName} updated and deactivated.", model.UserName);

            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Failed to update user details. Please contact Administrator.";
        _logger.LogError("Failed to update user details for {UserName}.", model.UserName);

        // Fetch database roles asynchronously first, then map in-memory
        var allRoles = await _roleManager.Roles.ToListAsync();

        model.RoleOptions = allRoles.Select(r => new SelectListItem
        {
            Value = r.Name,
            Text = r.Name
        }).ToList();

        return View(model);
    }

    // GET: Account/Details/5
    [HttpGet]
    [Breadcrumb("User Details", FromAction = nameof(Index))]
    //[RequirePermission(SystemPermissions.Account.ViewApplicationUser)]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToAction(nameof(Index));
        }

        var userEntity = await _accountRepo.FindById(id);
        if (userEntity == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<ApplicationUserViewModel>(userEntity);

        // Fetch assigned system roles
        var userRoles = await _userManager.GetRolesAsync(userEntity);
        model.SelectedRoles = userRoles.ToList();

        return View(model);
    }

    // GET: Account/Delete/5
    [HttpGet]
    [Breadcrumb("Delete Account", FromAction = nameof(Index))]
    //[RequirePermission(SystemPermissions.Account.DeleteApplicationUser)]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToAction(nameof(Index));
        }

        var userEntity = await _accountRepo.FindById(id);
        if (userEntity == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        var model = _mapper.Map<ApplicationUserViewModel>(userEntity);

        // Fetch user roles for context
        var userRoles = await _userManager.GetRolesAsync(userEntity);
        model.SelectedRoles = userRoles.ToList();

        return View(model);
    }

    // POST: Account/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    //[RequirePermission(SystemPermissions.Account.DeleteApplicationUser)]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var userEntity = await _accountRepo.FindById(id);
        if (userEntity == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        // Call repository delete (or soft delete / deactivate)
        var result = await _accountRepo.Delete(id);

        if (result)
        {
            TempData["SuccessMessage"] = $"User account {userEntity.UserName} was successfully deleted.";
            _logger.LogWarning("User account {UserName} (ID: {UserId}) was deleted by administrator.", userEntity.UserName, id);
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Failed to delete user account. Please contact Administrator.";
        return RedirectToAction(nameof(Index));
    }

    // =========================================================
    // IT ADMIN: ACCOUNT UNLOCK & PASSWORD ASSISTANCE
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Account.EditApplicationUser)]
    public async Task<IActionResult> Unlock(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User ID is required.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _accountRepo.FindById(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        // Reset lockout indicators on the domain user entity
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;

        var result = await _accountRepo.Update(user);

        if (result)
        {
            TempData["SuccessMessage"] = $"Account for {user.UserName} has been unlocked successfully.";
            _logger.LogInformation("IT Admin unlocked user account: {UserName}", user.UserName);
        }
        else
        {
            TempData["ErrorMessage"] = $"Failed to unlock account for {user.UserName}.";
            _logger.LogError("Failed to unlock user account: {UserName}", user.UserName);
        }

        return RedirectToAction(nameof(Index));
    }

    [Breadcrumb("Manage Roles", FromAction = nameof(Index))]
    [RequirePermission(SystemPermissions.Roles.ViewRoles)]
    public async Task<IActionResult> ManageRoles(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "User ID is required.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _accountRepo.FindById(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = $"User with ID {userId} was not found.";
            return RedirectToAction(nameof(Index));
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
            var matchedRole = model.Roles.FirstOrDefault(r => r.IsSelected && r.RoleName.Equals(role, StringComparison.OrdinalIgnoreCase));
            if (matchedRole == null)
            {
                await _accountRepo.RemoveApplicationUserFromRole(user, role);
                _logger.LogInformation("Removed role {RoleName} from user {UserName}", role, user.UserName);
            }
        }

        // Add selected roles
        foreach (var role in model.Roles.Where(r => r.IsSelected))
        {
            var roleEntity = await _roleRepo.FindById(role.RoleId);
            if (roleEntity != null && !userRoles.Contains(roleEntity.Name, StringComparer.OrdinalIgnoreCase))
            {
                await _accountRepo.AddApplicationUserFromRole(user, roleEntity.Name);
                _logger.LogInformation("Assigned role {RoleName} to user {UserName}", roleEntity.Name, user.UserName);
            }
        }

        TempData["SuccessMessage"] = $"Roles updated successfully for {user.UserName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        // 1. Get the current username safely
        var userName = User.Identity?.Name;

        // 2. Perform sign-out via your existing AuthService
        // If your AuthService.SignOutAsync() or Logout method expects a username, pass it safely:
        if (!string.IsNullOrEmpty(userName))
        {
            _logger.LogInformation("User {UserName} initiated logout.", userName);

            // Example call if your IAuthService requires the username:
            // await _authService.LogoutAsync(userName); 
        }

        // Call your auth service sign-out / cookie clearing method
        await _authService.SignOutApplicationUser(); 

        // 3. Redirect back to Login
        return RedirectToAction("Login", "Account");
    }
}