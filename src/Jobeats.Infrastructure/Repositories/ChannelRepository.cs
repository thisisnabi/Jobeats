using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using Jobeats.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Jobeats.Infrastructure.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly JobeatsDbContext _context;
    
    public ChannelRepository(JobeatsDbContext context)
    {
        _context = context;
    }
    
    public async Task<Channel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Include(c => c.Project)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<Channel>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .Where(c => c.ProjectId == projectId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<Channel>> GetByCheckIdAsync(Guid checkId, CancellationToken cancellationToken = default)
    {
        return await _context.CheckChannels
            .Where(cc => cc.CheckId == checkId)
            .Include(cc => cc.Channel)
            .Select(cc => cc.Channel!)
            .Where(c => !c.IsDisabled)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Channel> AddAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        _context.Channels.Add(channel);
        await _context.SaveChangesAsync(cancellationToken);
        return channel;
    }
    
    public async Task UpdateAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        channel.UpdatedAt = DateTime.UtcNow;
        _context.Channels.Update(channel);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var channel = await _context.Channels.FindAsync([id], cancellationToken);
        if (channel != null)
        {
            _context.Channels.Remove(channel);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
