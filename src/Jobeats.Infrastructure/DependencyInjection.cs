using Jobeats.Core.Interfaces;
using Jobeats.Infrastructure.BackgroundServices;
using Jobeats.Infrastructure.Data;
using Jobeats.Infrastructure.Notifications;
using Jobeats.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jobeats.Infrastructure;

/// <summary>
/// Dependency injection extensions for infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Database
        services.AddDbContext<JobeatsDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddScoped<ICheckRepository, CheckRepository>();
        services.AddScoped<IPingRepository, PingRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IFlipRepository, FlipRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();

        // HTTP client for notifications
        services.AddHttpClient("webhook");
        services.AddHttpClient("slack");
        services.AddHttpClient("discord");

        // Notification senders
        services.AddSingleton<INotificationSender, EmailNotificationSender>();
        services.AddSingleton<INotificationSender, WebhookNotificationSender>();
        services.AddSingleton<INotificationSender, SlackNotificationSender>();
        services.AddSingleton<INotificationSender, DiscordNotificationSender>();

        // Background services
        services.AddHostedService<AlertProcessorService>();
        services.AddHostedService<CheckSchedulerService>();
        services.AddHostedService<PruningService>();

        return services;
    }
}
