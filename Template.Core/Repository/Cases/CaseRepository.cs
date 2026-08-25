using Microsoft.EntityFrameworkCore;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Core.Repository.Cases;

public class CaseRepository(ApplicationDbContext _context) : ICaseRepository
{
    public async Task<ICollection<Case>> FindAll()
    {
        return await _context.Cases
            .Include(c => c.Type)
            .Include(c => c.Status)
            .Include(c => c.CreatedByUser)
            .Include(c => c.Hearings)
            .Include(c => c.Documents)
            .Include(c => c.Assignments)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Case?> FindById(int id)
    {
        return await _context.Cases
            .Include(c => c.Type)
            .Include(c => c.Status)
            .Include(c => c.CreatedByUser)
            .Include(c => c.Documents)                
            .ThenInclude(d => d.UploadedByUser)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task Add(Case caseEntity)
    {
        await _context.Cases.AddAsync(caseEntity);
        await _context.SaveChangesAsync();
    }

    public async Task Update(Case caseEntity)
    {
        _context.Cases.Update(caseEntity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entity = await FindById(id);
        if (entity != null)
        {
            _context.Cases.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}