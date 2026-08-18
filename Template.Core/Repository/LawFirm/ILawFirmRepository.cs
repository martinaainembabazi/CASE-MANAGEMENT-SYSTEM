using Template.Data.Entities;

namespace Template.Core.Repository;

public interface ILawFirmRepository
{
    Task<IEnumerable<LawFirm>> GetAllAsync();
    Task<LawFirm?> FindByIdAsync(int id);
    Task<bool> CreateAsync(LawFirm entity);
    Task<bool> UpdateAsync(LawFirm entity);
    Task<bool> SoftDeleteAsync(int id);
}