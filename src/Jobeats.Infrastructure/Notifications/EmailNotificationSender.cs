using System.Text.Json;
using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;

namespace Jobeats.Infrastructure.Notifications;

/// <summary>
/// Email notification configuration
/// </summary>
public class EmailChannelConfig
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Sends notifications via email
/// </summary>
public class EmailNotificationSender : INotificationSender
{
    public ChannelKind Kind => ChannelKind.Email;

    public Task<bool> SendAsync(Channel channel, Check check, Flip flip, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would use an email service like SendGrid, SMTP, etc.
        var config = JsonSerializer.Deserialize<EmailChannelConfig>(channel.Configuration ?? "{}");
        if (config == null || string.IsNullOrEmpty(config.Email))
        {
            return Task.FromResult(false);
        }

        var subject = flip.NewStatus == CheckStatus.Down
            ? $"🔴 Alert: {check.Name} is DOWN"
            : $"✅ Recovery: {check.Name} is UP";

        var body = $"""
            Check: {check.Name}
            Status: {flip.NewStatus}
            Previous Status: {flip.OldStatus}
            Time: {flip.CreatedAt:u}
            
            Description: {check.Description ?? "N/A"}
            Tags: {check.Tags ?? "N/A"}
            """;

        // Log the notification for now (in production, send actual email)
        Console.WriteLine($"[EMAIL] To: {config.Email}, Subject: {subject}");
        Console.WriteLine(body);

        return Task.FromResult(true);
    }
}
