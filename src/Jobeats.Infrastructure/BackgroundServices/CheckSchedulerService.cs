using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;
using Jobeats.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jobeats.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that checks for missed pings and creates flips
/// </summary>
public class CheckSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CheckSchedulerService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public CheckSchedulerService(
        IServiceProvider serviceProvider,
        ILogger<CheckSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Check Scheduler Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateChecksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating checks");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Check Scheduler Service stopped");
    }

    private async Task EvaluateChecksAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var checkRepository = scope.ServiceProvider.GetRequiredService<ICheckRepository>();
        var flipRepository = scope.ServiceProvider.GetRequiredService<IFlipRepository>();

        var now = DateTime.UtcNow;
        var checksNeedingAlert = await checkRepository.GetChecksNeedingAlertAsync(now, cancellationToken);

        foreach (var check in checksNeedingAlert)
        {
            _logger.LogInformation(
                "Check {CheckName} has missed expected ping. Alert time was {AlertAfterAt}",
                check.Name, check.AlertAfterAt);

            var oldStatus = check.Status;
            check.Status = CheckStatus.Down;
            await checkRepository.UpdateAsync(check, cancellationToken);

            // Create flip record
            if (oldStatus != CheckStatus.Down)
            {
                var flip = new Flip
                {
                    Id = Guid.NewGuid(),
                    CheckId = check.Id,
                    OldStatus = oldStatus,
                    NewStatus = CheckStatus.Down,
                    CreatedAt = now
                };
                await flipRepository.AddAsync(flip, cancellationToken);
            }
        }
    }
}
