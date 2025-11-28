namespace Jobeats.Core.Enums;

/// <summary>
/// Represents the type of notification channel
/// </summary>
public enum ChannelKind
{
    Email = 0,
    Slack = 1,
    Discord = 2,
    Webhook = 3,
    Telegram = 4,
    MsTeams = 5,
    PagerDuty = 6,
    Pushover = 7,
    Sms = 8
}
