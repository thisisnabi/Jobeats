using System.Net.Http.Json;
using System.Text.Json;
using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;

namespace Jobeats.Infrastructure.Notifications;

/// <summary>
/// Discord notification configuration
/// </summary>
public class DiscordChannelConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
}

/// <summary>
/// Sends notifications via Discord webhook
/// </summary>
public class DiscordNotificationSender : INotificationSender
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DiscordNotificationSender(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ChannelKind Kind => ChannelKind.Discord;

    public async Task<bool> SendAsync(Channel channel, Check check, Flip flip, CancellationToken cancellationToken = default)
    {
        var config = JsonSerializer.Deserialize<DiscordChannelConfig>(channel.Configuration ?? "{}");
        if (config == null || string.IsNullOrEmpty(config.WebhookUrl))
        {
            return false;
        }

        var isUp = flip.NewStatus == CheckStatus.Up;
        var color = isUp ? 3066993 : 15158332; // Green or Red in decimal
        var emoji = isUp ? "✅" : "🔴";
        var statusText = isUp ? "UP" : "DOWN";

        var payload = new
        {
            embeds = new[]
            {
                new
                {
                    title = $"{emoji} {check.Name} is {statusText}",
                    description = check.Description ?? "",
                    color,
                    fields = new[]
                    {
                        new { name = "Status", value = statusText, inline = true },
                        new { name = "Previous", value = flip.OldStatus.ToString(), inline = true },
                        new { name = "Tags", value = check.Tags ?? "None", inline = true }
                    },
                    timestamp = flip.CreatedAt.ToString("o"),
                    footer = new { text = "Jobeats Health Check" }
                }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("discord");
            var response = await client.PostAsJsonAsync(config.WebhookUrl, payload, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DISCORD] Failed to send notification: {ex.Message}");
            return false;
        }
    }
}
