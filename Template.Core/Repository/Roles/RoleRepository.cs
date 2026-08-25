using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Core.Repository.Roles;

public class RoleRepository(ApplicationDbContext _db) : IRoleRepository
{
    public async Task<ICollection<Role>> FindAll()
    {
        return await _db.Roles.Include(r => r.Users).AsNoTracking().ToListAsync();
    }

    public async Task<Role?> FindById(int id)
    {
        return await _db.Roles.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> Create(Role entity)
    {
        await _db.Roles.AddAsync(entity);
        return await Save();
    }

    public async Task<bool> Update(Role entity)
    {
        _db.Roles.Update(entity);
        return await Save();
    }

    public async Task<bool> Delete(int id)
    {
        var entity = await FindById(id);
        if (entity == null) return false;

        _db.Roles.Remove(entity);
        return await Save();
    }

    public async Task<bool> IsExists(int id)
    {
        return await _db.Roles.AnyAsync(r => r.Id == id);
    }

    public async Task<bool> Save()
    {
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<IList<Claim>> GetClaims(Role role)
    {
        return await Task.FromResult(new List<Claim>());
    }

    public async Task<bool> UpdateRoleClaims(Role role, List<string> selectedPermissions)
    {
        return await Task.FromResult(true);
    }
}