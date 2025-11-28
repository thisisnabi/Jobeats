using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using Jobeats.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobeats.Infrastructure.Repositories;

public class PingRepository : IPingRepository
{
    private readonly JobeatsDbContext _context;
    
    public PingRepository(JobeatsDbContext context)
    {
        _context = context;
    }
    
    public async Task<Ping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Pings.FindAsync([id], cancellationToken);
    }
    
    public async Task<IEnumerable<Ping>> GetByCheckIdAsync(Guid checkId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Pings
            .Where(p => p.CheckId == checkId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Ping> AddAsync(Ping ping, CancellationToken cancellationToken = default)
    {
        _context.Pings.Add(ping);
        await _context.SaveChangesAsync(cancellationToken);
        return ping;
    }
    
    public async Task<int> DeleteOldPingsAsync(Guid checkId, int keepCount, CancellationToken cancellationToken = default)
    {
        var pingsToDelete = await _context.Pings
            .Where(p => p.CheckId == checkId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(keepCount)
            .ToListAsync(cancellationToken);
        
        if (pingsToDelete.Count != 0)
        {
            _context.Pings.RemoveRange(pingsToDelete);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        return pingsToDelete.Count;
    }
}
