using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
using Template.ViewModels;
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
   SignInManager<ApplicationUser> _signInManager,
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

        // Check if account is locked out or deactivated
        if (result.User != null && (!result.User.IsActive || result.User.DisableDate.HasValue))
        {
            var lockMessage = !string.IsNullOrWhiteSpace(result.User.LockReason)
                ? $"Account locked: {result.User.LockReason}"
                : "Your account has been locked by IT Support. Please contact your administrator.";

            ViewData["status"] = "AccountLocked";
            ModelState.AddModelError("", lockMessage);
            return View();
        }

        await _authService.SignInApplicationUser(result.User);
        _logger.LogInformation("Login successful for user: {Username}", username);

        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // Role-based dashboard redirection
        if (User.IsInRole(RoleConstants.ItSupport))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        if (User.IsInRole(RoleConstants.LawFirm))
        {
            return RedirectToAction("Dashboard", "LawFirmPortal");
        }

        if (User.IsInRole(RoleConstants.LegalStaff))
        {
            return RedirectToAction("Dashboard", "Case");
        }

        // Default fallback if role is unspecified or standard user
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    // GET: User/Create
    [HttpGet]
    [Authorize(Roles = "Admin,IT Support")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateViewModel
        {
            AvailableRoles = await _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToListAsync()
        };

        return View(model);
    }

    // POST: User/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,IT Support")]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableRoles = await _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToListAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Username.Trim(),
            EmailConfirmed = true
        };

        // Create user with the local password
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Assign selected role for access control
            if (!string.IsNullOrEmpty(model.SelectedRole) && await _roleManager.RoleExistsAsync(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            TempData["Success"] = $"User '{user.UserName}' created and assigned to role '{model.SelectedRole}'.";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        model.AvailableRoles = await _roleManager.Roles
            .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
            .ToListAsync();

        return View(model);
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

        if (model.IsActive)
        {
            userInfo.IsActive = true;
            userInfo.DisableDate = null;
            userInfo.LockReason = null; // Clear lock reason when activating

            // Clear ASP.NET Identity lockout date and reset access counter
            await _userManager.SetLockoutEndDateAsync(userInfo, null);
            await _userManager.ResetAccessFailedCountAsync(userInfo);
        }
        else
        {
            userInfo.IsActive = false;
            userInfo.DisableDate = DateTime.UtcNow;
            userInfo.LockReason = model.LockReason; // Capture lock reason from model

            // Lock ASP.NET Identity lockout end into the distant future
            await _userManager.SetLockoutEndDateAsync(userInfo, DateTimeOffset.UtcNow.AddYears(100));
        }

        _mapper.Map(model, userInfo);

        userInfo.FirstName = string.IsNullOrWhiteSpace(userInfo.FirstName) ? userInfo.UserName : userInfo.FirstName;
        userInfo.LastName = string.IsNullOrWhiteSpace(userInfo.LastName) ? "Staff" : userInfo.LastName;
        userInfo.FullName = string.IsNullOrWhiteSpace(userInfo.FullName) ? $"{userInfo.FirstName} {userInfo.LastName}" : userInfo.FullName;
        userInfo.Title = string.IsNullOrWhiteSpace(userInfo.Title) ? "N/A" : userInfo.Title;

        userInfo.BusinessUnit = string.IsNullOrWhiteSpace(userInfo.BusinessUnit) ? "N/A" : userInfo.BusinessUnit;
        userInfo.JobTitle = string.IsNullOrWhiteSpace(userInfo.JobTitle) ? "N/A" : userInfo.JobTitle;
        userInfo.Station = string.IsNullOrWhiteSpace(userInfo.Station) ? "N/A" : userInfo.Station;
        userInfo.AgeBracket = string.IsNullOrWhiteSpace(userInfo.AgeBracket) ? "N/A" : userInfo.AgeBracket;
        userInfo.Gender = string.IsNullOrWhiteSpace(userInfo.Gender) ? "N/A" : userInfo.Gender;

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
            var roleEntity = await _roleRepo.FindById(int.Parse(role.RoleId));
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

    // POST: User/ToggleLockout
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,ITSupport")]
    public async Task<IActionResult> ToggleLockout(Guid userId, string? lockReason)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            TempData["ErrorMessage"] = "User account not found.";
            return RedirectToAction(nameof(Index));
        }

        var isCurrentlyLocked = await _userManager.IsLockedOutAsync(user);

        if (isCurrentlyLocked)
        {
            // Unlock user
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            user.LockReason = null;
            user.IsActive = true;

            TempData["SuccessMessage"] = $"Account for '{user.UserName}' has been unlocked.";
        }
        else
        {
            // Lock user until distant future date
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            user.LockReason = string.IsNullOrWhiteSpace(lockReason) ? "Locked by IT Administrator." : lockReason;
            user.IsActive = false;

            TempData["SuccessMessage"] = $"Account for '{user.UserName}' has been locked.";
        }

        await _accountRepo.Update(user); // Save custom field changes
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = new UserAccountSettingsViewModel
        {
            UserName = user.UserName ?? "N/A",
            Email = user.Email ?? "N/A",
            FullName = string.IsNullOrWhiteSpace(user.FullName) ? $"{user.FirstName} {user.LastName}" : user.FullName,
            JobTitle = user.JobTitle,
            BusinessUnit = user.BusinessUnit
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(UserAccountSettingsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Re-populate profile data for returning the view if validation fails
        model.UserName = user.UserName ?? "N/A";
        model.Email = user.Email ?? "N/A";
        model.FullName = string.IsNullOrWhiteSpace(user.FullName) ? $"{user.FirstName} {user.LastName}" : user.FullName;
        model.JobTitle = user.JobTitle;
        model.BusinessUnit = user.BusinessUnit;

        if (!ModelState.IsValid) return View("Settings", model);

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = "Your password has been changed successfully.";
            return RedirectToAction(nameof(Settings));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Settings", model);
    }
}