using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Template.Core.Repository.Common;
using Template.Data.Entities;

namespace Template.Core.Repository.Roles
{
public interface IRoleRepository: IRepositoryBase<IdentityRole<Guid>, string>
    {
        //Task<bool> Create(ApplicationRole model);
        Task<bool> Delete(string id);

        Task<IList<Claim>> GetClaims(IdentityRole<Guid> role);
        Task<bool> UpdateRoleClaims(IdentityRole<Guid> role, List<string> selectedPermissions);
    }
}
