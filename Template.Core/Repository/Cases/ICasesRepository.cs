
using Template.Data.Entities;

namespace Template.Core.Repository.Cases;

public interface ICaseRepository
{
    Task<ICollection<Case>> FindAll();
    Task<Case?> FindById(int id);
    Task Add(Case caseEntity);
    Task Update(Case caseEntity);
    Task Delete(int id);
}
