using Jobeats.Core.Entities;

namespace Jobeats.Core.Interfaces;

/// <summary>
/// Repository interface for Channel entities
/// </summary>
public interface IChannelRepository
{
    Task<Channel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Channel>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Channel>> GetByCheckIdAsync(Guid checkId, CancellationToken cancellationToken = default);
    Task<Channel> AddAsync(Channel channel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Channel channel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
