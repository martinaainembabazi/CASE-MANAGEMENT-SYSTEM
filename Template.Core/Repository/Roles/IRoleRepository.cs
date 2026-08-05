using Template.Core.Repository.Common;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Template.Core.Repository.Roles
{
public interface IRoleRepository: IRepositoryBase<IdentityRole<Guid>, string>
    {
        //Task<bool> Create(ApplicationRole model);
        Task<bool> Delete(string id);
    }
}
