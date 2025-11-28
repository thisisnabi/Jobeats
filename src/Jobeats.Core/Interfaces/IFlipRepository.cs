using Jobeats.Core.Entities;

namespace Jobeats.Core.Interfaces;

/// <summary>
/// Repository interface for Flip entities
/// </summary>
public interface IFlipRepository
{
    Task<Flip?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Flip>> GetByCheckIdAsync(Guid checkId, int limit = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<Flip>> GetUnprocessedFlipsAsync(int limit = 100, CancellationToken cancellationToken = default);
    Task<Flip> AddAsync(Flip flip, CancellationToken cancellationToken = default);
    Task UpdateAsync(Flip flip, CancellationToken cancellationToken = default);
}
