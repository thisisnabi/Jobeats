using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jobeats.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that processes unprocessed flips and sends notifications
/// </summary>
public class AlertProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlertProcessorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

    public AlertProcessorService(
        IServiceProvider serviceProvider,
        ILogger<AlertProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Alert Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessUnprocessedFlipsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing flips");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Alert Processor Service stopped");
    }

    private async Task ProcessUnprocessedFlipsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var flipRepository = scope.ServiceProvider.GetRequiredService<IFlipRepository>();
        var channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();
        var notificationSenders = scope.ServiceProvider.GetServices<INotificationSender>();

        var unprocessedFlips = await flipRepository.GetUnprocessedFlipsAsync(100, cancellationToken);

        foreach (var flip in unprocessedFlips)
        {
            if (flip.Check == null)
            {
                continue;
            }

            _logger.LogInformation(
                "Processing flip for check {CheckName}: {OldStatus} -> {NewStatus}",
                flip.Check.Name, flip.OldStatus, flip.NewStatus);

            // Get channels configured for this check's project
            var channels = await channelRepository.GetByProjectIdAsync(
                flip.Check.ProjectId, cancellationToken);

            foreach (var channel in channels.Where(c => !c.IsDisabled))
            {
                var sender = notificationSenders.FirstOrDefault(s => s.Kind == channel.Kind);
                if (sender == null)
                {
                    _logger.LogWarning("No sender found for channel kind {Kind}", channel.Kind);
                    continue;
                }

                try
                {
                    var success = await sender.SendAsync(channel, flip.Check, flip, cancellationToken);
                    if (success)
                    {
                        channel.NotifyCount++;
                        channel.LastNotifyAt = DateTime.UtcNow;
                        channel.FailureCount = 0;
                        channel.LastError = null;
                    }
                    else
                    {
                        channel.FailureCount++;
                        channel.LastError = "Send failed";
                    }

                    await channelRepository.UpdateAsync(channel, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending notification via {ChannelKind}", channel.Kind);
                    channel.FailureCount++;
                    channel.LastError = ex.Message;
                    await channelRepository.UpdateAsync(channel, cancellationToken);
                }
            }

            // Mark flip as processed
            flip.ProcessedAt = DateTime.UtcNow;
            await flipRepository.UpdateAsync(flip, cancellationToken);
        }
    }
}
