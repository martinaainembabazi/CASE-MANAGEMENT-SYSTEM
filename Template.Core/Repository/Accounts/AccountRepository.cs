using Template.Core.Models.Roles;
using Template.Data.Configurations;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Template.Core.Repository.Accounts
{
    public class AccountRepository(
        ApplicationDbContext _db
        , UserManager<ApplicationUser> _userManager
        ) : IAccountRepository
    {
        public async Task<bool> Create(ApplicationUser user)
        {
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
                return true;
            else
                return false;
        }

        public async Task<ICollection<ApplicationUser>> FindAll()
        {
            return await _userManager.Users.ToListAsync();
        }
        public async Task<ICollection<string>> GetApplicationUserRoles(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }
        public async Task<IdentityResult> RemoveApplicationUserFromRole(ApplicationUser user, string rolename)
        {
            return await _userManager.RemoveFromRoleAsync(user, rolename);
        }
        public async Task<IdentityResult> AddApplicationUserFromRole(ApplicationUser user, string rolename)
        {
            return await _userManager.AddToRoleAsync(user, rolename);
        }

        public async Task<ApplicationUser> FindById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            return user ?? throw new ArgumentException($"User ID '{id}' not found.");
        }

        //update user
        public async Task<bool> Update(ApplicationUser model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());

            if (user != null)
            {
                user.UserName = model.UserName;
                user.IsActive = model.IsActive;
                user.DisableDate = model.DisableDate;
                user.EndDate = model.EndDate;
                user.Title = model.Title;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> Edit(CombinedViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.RoleAssignment.Id);

            if (user == null)
                return false;

            // Get the current roles of the user
            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
                return false;

            // Add the new roles to the user
            var addResult = await _userManager.AddToRolesAsync(user, model.RoleAssignment.NewRoles);
            if (addResult.Succeeded)
                return true;
            else
                return false;
        }

        //deletes user
        public async Task<bool> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return true;
            }

            return false;
        }


        public async Task<bool> IsExists(string id)
        {
            if (Guid.TryParse(id, out var guidId))
            {
                return true;
            }
            return false;
        }
        public async Task<ApplicationUser> FindByName(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }
        public async Task<bool> Save()
        {
            return await _db.SaveChangesAsync() > 0;
        }

    }
}
