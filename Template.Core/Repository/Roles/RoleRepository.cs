using Template.Data.Configurations;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Template.Core.Repository.Roles;

public class RoleRepository(ApplicationDbContext _db
    , RoleManager<IdentityRole<Guid>> _roleManager
    , ILogger<RoleRepository> _logger) : IRoleRepository
{

    //returns all roles
    public async Task<ICollection<IdentityRole<Guid>>> FindAll()
    {
        return await _roleManager.Roles.AsNoTracking().ToListAsync();
    }

    public async Task<IdentityRole<Guid>> FindById(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        return role ?? throw new ArgumentException($"Role ID '{roleId}' was not found.");
    }

    //creates new role
    public async Task<bool> Create(IdentityRole<Guid> role)
    {
        var result = await _roleManager.CreateAsync(role);

        if (result.Succeeded)
            return true;
        else
            return false;
    }

    //saves database transaction
    public async Task<bool> Save()
    {
        return await _db.SaveChangesAsync() > 0;
    }

    //update role
    public async Task<bool> Update(IdentityRole<Guid> model)
    {
        var role = await _roleManager.FindByIdAsync(model.Id.ToString());

        if (role != null)
        {
            role.Name = model.Name;
            //role.Description = model.Description;
            //role.Permissions = model.Permissions;

            var result = await _roleManager.UpdateAsync(role);

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

    //deletes role
    public async Task<bool> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role != null)
        {
            var result = await _roleManager.DeleteAsync(role);
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

    public async Task<bool> IsExists(string id)
    {
        return await _roleManager.Roles.AnyAsync(r => r.Id.ToString() == id);
    }
}
