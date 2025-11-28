using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using Jobeats.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobeats.Infrastructure.Repositories;

public class CheckRepository : ICheckRepository
{
    private readonly JobeatsDbContext _context;
    
    public CheckRepository(JobeatsDbContext context)
    {
        _context = context;
    }
    
    public async Task<Check?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Checks
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    
    public async Task<Check?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Checks
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }
    
    public async Task<IEnumerable<Check>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Checks
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Check>> GetChecksNeedingAlertAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await _context.Checks
            .Where(c => c.Status != Core.Enums.CheckStatus.Paused &&
                       c.Status != Core.Enums.CheckStatus.Down &&
                       c.AlertAfterAt.HasValue &&
                       c.AlertAfterAt < now)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Check> AddAsync(Check check, CancellationToken cancellationToken = default)
    {
        _context.Checks.Add(check);
        await _context.SaveChangesAsync(cancellationToken);
        return check;
    }
    
    public async Task UpdateAsync(Check check, CancellationToken cancellationToken = default)
    {
        check.UpdatedAt = DateTime.UtcNow;
        _context.Checks.Update(check);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var check = await _context.Checks.FindAsync([id], cancellationToken);
        if (check != null)
        {
            _context.Checks.Remove(check);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
