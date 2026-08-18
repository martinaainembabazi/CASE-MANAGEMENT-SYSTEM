using Microsoft.EntityFrameworkCore;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Core.Repository;

public class LawFirmRepository : ILawFirmRepository
{
    private readonly ApplicationDbContext _context;

    public LawFirmRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LawFirm>> GetAllAsync()
    {
        return await _context.LawFirms
            .Include(lf => lf.Lawyers)
            .Include(lf => lf.CaseAssignments)
            .ToListAsync();
    }

    public async Task<LawFirm?> FindByIdAsync(int id)
    {
        return await _context.LawFirms
            .Include(lf => lf.Lawyers)
            .Include(lf => lf.CaseAssignments)
            .Include(lf => lf.Users)
            .FirstOrDefaultAsync(lf => lf.Id == id);
    }

    public async Task<bool> CreateAsync(LawFirm entity)
    {
        await _context.LawFirms.AddAsync(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(LawFirm entity)
    {
        _context.LawFirms.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var entity = await _context.LawFirms.FindAsync(id);
        if (entity == null) return false;

        entity.Status = "Inactive";
        _context.LawFirms.Update(entity);
        return await _context.SaveChangesAsync() > 0;
    }
}