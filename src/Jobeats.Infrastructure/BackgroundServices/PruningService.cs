using Jobeats.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jobeats.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that prunes old pings and flips
/// </summary>
public class PruningService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PruningService> _logger;
    private readonly TimeSpan _pruneInterval = TimeSpan.FromHours(1);
    private readonly int _keepPingsPerCheck = 100;

    public PruningService(
        IServiceProvider serviceProvider,
        ILogger<PruningService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Pruning Service started");

        // Wait a bit before first prune
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PruneOldDataAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pruning old data");
            }

            await Task.Delay(_pruneInterval, stoppingToken);
        }

        _logger.LogInformation("Pruning Service stopped");
    }

    private async Task PruneOldDataAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var checkRepository = scope.ServiceProvider.GetRequiredService<ICheckRepository>();
        var pingRepository = scope.ServiceProvider.GetRequiredService<IPingRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<Jobeats.Infrastructure.Data.JobeatsDbContext>();

        _logger.LogInformation("Starting data pruning, keeping last {Count} pings per check", _keepPingsPerCheck);

        // Get all check IDs from the database
        var checkIds = await dbContext.Checks.Select(c => c.Id).ToListAsync(cancellationToken);
        
        var totalPruned = 0;
        foreach (var checkId in checkIds)
        {
            try
            {
                var pruned = await pingRepository.DeleteOldPingsAsync(checkId, _keepPingsPerCheck, cancellationToken);
                totalPruned += pruned;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error pruning pings for check {CheckId}", checkId);
            }
        }

        _logger.LogInformation("Data pruning completed, removed {Count} old pings", totalPruned);
    }
}
