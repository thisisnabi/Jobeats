using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using Jobeats.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobeats.Infrastructure.Repositories;

public class FlipRepository : IFlipRepository
{
    private readonly JobeatsDbContext _context;
    
    public FlipRepository(JobeatsDbContext context)
    {
        _context = context;
    }
    
    public async Task<Flip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Flips
            .Include(f => f.Check)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Flip>> GetByCheckIdAsync(Guid checkId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Flips
            .Where(f => f.CheckId == checkId)
            .OrderByDescending(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Flip>> GetUnprocessedFlipsAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Flips
            .Include(f => f.Check)
                .ThenInclude(c => c!.Project)
            .Where(f => f.ProcessedAt == null)
            .OrderBy(f => f.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Flip> AddAsync(Flip flip, CancellationToken cancellationToken = default)
    {
        _context.Flips.Add(flip);
        await _context.SaveChangesAsync(cancellationToken);
        return flip;
    }
    
    public async Task UpdateAsync(Flip flip, CancellationToken cancellationToken = default)
    {
        _context.Flips.Update(flip);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
