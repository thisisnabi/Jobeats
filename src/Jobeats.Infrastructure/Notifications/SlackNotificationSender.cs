using System.Net.Http.Json;
using System.Text.Json;
using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;

namespace Jobeats.Infrastructure.Notifications;

/// <summary>
/// Slack notification configuration
/// </summary>
public class SlackChannelConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string? Channel { get; set; }
}

/// <summary>
/// Sends notifications via Slack webhook
/// </summary>
public class SlackNotificationSender : INotificationSender
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SlackNotificationSender(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ChannelKind Kind => ChannelKind.Slack;

    public async Task<bool> SendAsync(Channel channel, Check check, Flip flip, CancellationToken cancellationToken = default)
    {
        var config = JsonSerializer.Deserialize<SlackChannelConfig>(channel.Configuration ?? "{}");
        if (config == null || string.IsNullOrEmpty(config.WebhookUrl))
        {
            return false;
        }

        var isUp = flip.NewStatus == CheckStatus.Up;
        var color = isUp ? "#36a64f" : "#dc3545";
        var emoji = isUp ? ":white_check_mark:" : ":rotating_light:";
        var statusText = isUp ? "UP" : "DOWN";

        var payload = new
        {
            channel = config.Channel,
            attachments = new[]
            {
                new
                {
                    color,
                    fallback = $"{check.Name} is {statusText}",
                    title = $"{emoji} {check.Name} is {statusText}",
                    text = check.Description ?? "",
                    fields = new[]
                    {
                        new { title = "Status", value = statusText, @short = true },
                        new { title = "Previous", value = flip.OldStatus.ToString(), @short = true },
                        new { title = "Time", value = flip.CreatedAt.ToString("u"), @short = true },
                        new { title = "Tags", value = check.Tags ?? "None", @short = true }
                    },
                    footer = "Jobeats Health Check",
                    ts = ((DateTimeOffset)flip.CreatedAt).ToUnixTimeSeconds()
                }
            }
        };

        try
        {
            var client = _httpClientFactory.CreateClient("slack");
            var response = await client.PostAsJsonAsync(config.WebhookUrl, payload, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SLACK] Failed to send notification: {ex.Message}");
            return false;
        }
    }
}
