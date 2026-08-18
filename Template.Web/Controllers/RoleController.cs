using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Template.Core.Repository.Roles;
using Template.Core.Models.Roles;
using Template.Core.Repository.ApplicationPermission;
using Microsoft.AspNetCore.Identity;
using Template.Core.Services.Authorization;
using SmartBreadcrumbs.Attributes;
using Template.Core.Models.Permissions;
using Template.Core.Repository.Accounts;

/*namespace Template.Web.Controllers
{
    public class RoleController(IMapper _mapper, IRoleRepository _roleRepo, IApplicationPermissionRepository _permissionRepo, ILogger<RoleController> _logger) : Controller
    {
        [RequirePermission(SystemPermissions.Roles.ViewRoles)]
        [Breadcrumb("Role", FromAction = nameof(Index), FromController = typeof(HomeController))]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepo.FindAll();

            var viewModel = new RoleListPageViewModel
            {
                Roles = _mapper.Map<List<RoleListViewModel>>(roles)
            };

            return View(viewModel);
        }

        [Breadcrumb("Create", FromAction = nameof(Index))]
        [RequirePermission(SystemPermissions.Roles.CreateRole)]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(SystemPermissions.Roles.CreateRole)]
        public async Task<IActionResult> Create(RoleListViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var roleExists = await _roleRepo.IsExists(model.Name);
            if (roleExists)
            {
                ModelState.AddModelError("", $"Role '{model.Name}' already exists");
                return View(model);
            }

            var role = _mapper.Map<IdentityRole<Guid>>(model);

            var result = await _roleRepo.Create(role);

            if (result)
            {
                _logger.LogInformation($"Role '{model.Name}' created successfully.");
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to create role. Please try again.");
            return View(model);
        }

        [Breadcrumb("Edit", FromAction = nameof(Index))]
        [RequirePermission(SystemPermissions.Roles.EditRole)]
        public async Task<IActionResult> Edit(string roleId)
        {
            var role = await _roleRepo.FindById(roleId);

            if (role == null)
            {
                TempData["ErrorMessage"] = "The requested role could not be found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new RoleListViewModel
            {
                Id = role.Id.ToString(),
                Name = role.Name
            }; // fix

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(SystemPermissions.Roles.EditRole)]
        public async Task<IActionResult> Edit(RoleListViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var role = await _roleRepo.FindById(model.Id);

            if (role == null)
            {
                TempData["ErrorMessage"] = "The requested role could not be found.";
                return RedirectToAction(nameof(Index));
            }

            role.Name = model.Name;
            var result = await _roleRepo.Update(role);

            if (result)
            {
                _logger.LogInformation($"Role '{model.Name}' updated successfully.");
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Failed to update role. Please try again.");
            return View(model);
        }


        [Breadcrumb("Manage Permissions", FromAction = nameof(Index))]
        [RequirePermission(SystemPermissions.Roles.EditRole)]
        public async Task<IActionResult> ManagePermissions(string roleId)
        {
            var role = await _roleRepo.FindById(roleId);
            if (role == null)
                return NotFound();

            var allPermissions = await _permissionRepo.GetAllPermissions();
            var rolePermissions = await _permissionRepo.GetPermissionsForRole(role.Name);

            // Group permissions by controller name
            var model = new ManagePermissionsViewModel
            {
                RoleId = roleId,
                RoleName = role.Name,
                PermissionGroups = allPermissions
                    .GroupBy(p => ExtractControllerName(p))
                    .Select(g => new PermissionGroupViewModel
                    {
                        GroupName = g.Key,
                        Permissions = g.Select(p => new PermissionViewModel
                        {
                            Name = p,
                            DisplayName = ExtractActionName(p),
                            IsSelected = rolePermissions.Contains(p)
                        }).ToList()
                    }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(SystemPermissions.Roles.EditRole)]
        public async Task<IActionResult> ManagePermissions(ManagePermissionsViewModel model)
        {
            var role = await _roleRepo.FindById(model.RoleId);
            if (role == null)
                return NotFound();

            var currentPermissions = await _permissionRepo.GetPermissionsForRole(role.Name);

            var selectedPermissions = model.PermissionGroups
                .SelectMany(g => g.Permissions)
                .Where(p => p.IsSelected)
                .Select(p => p.Name)
                .ToList();

            // Determine permissions to add and remove
            var permissionsToAdd = selectedPermissions.Except(currentPermissions).ToList();
            var permissionsToRemove = currentPermissions.Except(selectedPermissions).ToList();

            // Update permissions
            await _permissionRepo.AddPermissionsToRole(role.Name, permissionsToAdd);
            await _permissionRepo.RemovePermissionsFromRole(role.Name, permissionsToRemove);

            _logger.LogInformation("Permissions updated successfully.");

            return RedirectToAction(nameof(Index));
        }


        //FUNCTIONS
        private string ExtractControllerName(string permission)
        {
            var parts = permission.Split(' ');
            return parts.Length > 0 ? parts[0] : "Other";
        }

        private string ExtractActionName(string permission)
        {
            var parts = permission.Split(' ');
            return parts.Length > 1 ? parts[1] : permission;
        }
    }
}*/




namespace Template.Web.Controllers;

public class RoleController(
    ILogger<RoleController> _logger,
    IRoleRepository _roleRepo,
    IAccountRepository _accountRepo
    ) : Controller
{
    //[RequirePermission(SystemPermissions.Roles.ViewRoles)]
    [Breadcrumb("Roles & Rights", FromAction = nameof(Index), FromController = typeof(HomeController))]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleRepo.FindAll();

        // Map IdentityRole<Guid> to ApplicationRoleViewModel (or equivalent model in Template.Core.Models.Roles)
        var roleViewModels = roles.Select(role => new ApplicationRoleViewModel
        {
            Id = role.Id.ToString(),
            Name = role.Name ?? string.Empty
        }).ToList();

        return View(roleViewModels);
    }

    [RequirePermission(SystemPermissions.Roles.CreateRole)]
    [Breadcrumb("Create Role", FromAction = nameof(Index))]
    public IActionResult Create()
    {
        return View(new ApplicationRoleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Roles.CreateRole)]
    public async Task<IActionResult> Create(ApplicationRoleViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var newRole = new IdentityRole<Guid>
        {
            Id = Guid.NewGuid(),
            Name = model.Name
        };

        var result = await _roleRepo.Create(newRole);
        if (result)
        {
            TempData["SuccessMessage"] = $"Role '{model.Name}' created successfully.";
            _logger.LogInformation("Admin created new role: {RoleName}", model.Name);
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Failed to create the role. Please try again.");
        return View(model);
    }

    [RequirePermission(SystemPermissions.Roles.EditRole)]
    [Breadcrumb("Manage Permissions", FromAction = nameof(Index))]
    public async Task<IActionResult> Permissions(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
        {
            TempData["ErrorMessage"] = "Role ID is required.";
            return RedirectToAction(nameof(Index));
        }

        var role = await _roleRepo.FindById(roleId);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction(nameof(Index));
        }

        var claims = await _roleRepo.GetClaims(role);
        var assignedPermissions = claims.Select(c => c.Value).ToList();

        var model = new RolePermissionsViewModel
        {
            RoleId = roleId,
            RoleName = role.Name ?? string.Empty,
            Permissions = BuildPermissionsList(assignedPermissions)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(SystemPermissions.Roles.EditRole)]
    public async Task<IActionResult> Permissions(RolePermissionsViewModel model)
    {
        var role = await _roleRepo.FindById(model.RoleId);
        if (role == null)
            return NotFound();

        var selectedPermissions = model.Permissions
            .Where(p => p.IsSelected)
            .Select(p => p.Name)
            .ToList();

        var result = await _roleRepo.UpdateRoleClaims(role, selectedPermissions);

        if (result)
        {
            TempData["SuccessMessage"] = $"Permissions updated successfully for role '{role.Name}'.";
            _logger.LogInformation("Updated permissions for role: {RoleName}", role.Name);
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = "Failed to update permissions.";
        return View(model);
    }

    private List<PermissionViewModel> BuildPermissionsList(List<string> assignedPermissions)
    {
        var list = new List<PermissionViewModel>();
        var nestedTypes = typeof(SystemPermissions).GetNestedTypes();

        foreach (var type in nestedTypes)
        {
            var fields = type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly)
                {
                    var permValue = field.GetValue(null)?.ToString();
                    if (!string.IsNullOrEmpty(permValue))
                    {
                        list.Add(new PermissionViewModel
                        {
                            Name = permValue,
                            DisplayName = $"{type.Name} - {field.Name}",
                            IsSelected = assignedPermissions.Contains(permValue, StringComparer.OrdinalIgnoreCase)
                        });
                    }
                }
            }
        }

        return list;
    }
}