using Template.Core.Repository.Common;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Template.Core.Repository.Accounts
{
    public interface IAccountRepository : IRepositoryBase<ApplicationUser, string>
    {
        //Task<bool> Create(ApplicationUser model);
        //Task<ICollection<ApplicationUser>> FindAll();
        Task<ApplicationUser> FindByName(string username);
        //Task<bool> Update(string id, ApplicationUser user);

        Task<bool> Delete(string id);
        //Task<bool> UserStatus(string id);
        //Task<bool> Assign(CombinedViewModel model);
        //Task<bool> Edit(CombinedViewModel model);
        Task<ICollection<string>> GetApplicationUserRoles(ApplicationUser user);
        Task<IdentityResult> RemoveApplicationUserFromRole(ApplicationUser user, string rolename);
        Task<IdentityResult> AddApplicationUserFromRole(ApplicationUser user, string rolename);
    }
}
