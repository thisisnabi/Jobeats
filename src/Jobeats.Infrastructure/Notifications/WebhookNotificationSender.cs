using System.Net.Http.Json;
using System.Text.Json;
using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;

namespace Jobeats.Infrastructure.Notifications;

/// <summary>
/// Webhook notification configuration
/// </summary>
public class WebhookChannelConfig
{
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Sends notifications via HTTP webhook
/// </summary>
public class WebhookNotificationSender : INotificationSender
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WebhookNotificationSender(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public ChannelKind Kind => ChannelKind.Webhook;

    public async Task<bool> SendAsync(Channel channel, Check check, Flip flip, CancellationToken cancellationToken = default)
    {
        var config = JsonSerializer.Deserialize<WebhookChannelConfig>(channel.Configuration ?? "{}");
        if (config == null || string.IsNullOrEmpty(config.Url))
        {
            return false;
        }

        var payload = new
        {
            name = check.Name,
            slug = check.Slug,
            status = flip.NewStatus.ToString().ToLowerInvariant(),
            previous_status = flip.OldStatus.ToString().ToLowerInvariant(),
            timestamp = flip.CreatedAt,
            description = check.Description,
            tags = check.Tags,
            ping_url = $"/ping/{check.Id}",
            is_up = flip.NewStatus == CheckStatus.Up
        };

        try
        {
            var client = _httpClientFactory.CreateClient("webhook");
            
            // Add custom headers if configured
            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            var response = await client.PostAsJsonAsync(config.Url, payload, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WEBHOOK] Failed to send to {config.Url}: {ex.Message}");
            return false;
        }
    }
}
