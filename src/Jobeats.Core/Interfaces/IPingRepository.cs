using Jobeats.Core.Entities;

namespace Jobeats.Core.Interfaces;

/// <summary>
/// Repository interface for Ping entities
/// </summary>
public interface IPingRepository
{
    Task<Ping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ping>> GetByCheckIdAsync(Guid checkId, int limit = 100, CancellationToken cancellationToken = default);
    Task<Ping> AddAsync(Ping ping, CancellationToken cancellationToken = default);
    Task<int> DeleteOldPingsAsync(Guid checkId, int keepCount, CancellationToken cancellationToken = default);
}
