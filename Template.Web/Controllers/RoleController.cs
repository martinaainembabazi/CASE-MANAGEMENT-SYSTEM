using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Template.Core.Repository.Roles;
using Template.Core.Models.Roles;
using Template.Core.Repository.ApplicationPermission;
using Microsoft.AspNetCore.Identity;
using Template.Core.Services.Authorization;
using SmartBreadcrumbs.Attributes;
using Template.Core.Models.Permissions;

namespace Template.Web.Controllers
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
}
