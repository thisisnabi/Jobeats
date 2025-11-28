using Jobeats.Core.Entities;

namespace Jobeats.Core.Interfaces;

/// <summary>
/// Repository interface for Check entities
/// </summary>
public interface ICheckRepository
{
    Task<Check?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Check?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IEnumerable<Check>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Check>> GetChecksNeedingAlertAsync(DateTime now, CancellationToken cancellationToken = default);
    Task<Check> AddAsync(Check check, CancellationToken cancellationToken = default);
    Task UpdateAsync(Check check, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
