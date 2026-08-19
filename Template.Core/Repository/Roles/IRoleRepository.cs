using System.Security.Claims;
using Template.Core.Repository.Common;
using Template.Data.Entities;

namespace Template.Core.Repository.Roles;

public interface IRoleRepository : IRepositoryBase<Role, int>
{
    Task<bool> Delete(int id);
    Task<IList<Claim>> GetClaims(Role role);
    Task<bool> UpdateRoleClaims(Role role, List<string> selectedPermissions);
}