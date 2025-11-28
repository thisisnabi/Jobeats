namespace Jobeats.Core.Enums;

/// <summary>
/// Represents the current status of a health check
/// </summary>
public enum CheckStatus
{
    /// <summary>
    /// The check has been created but hasn't received any pings yet
    /// </summary>
    New = 0,

    /// <summary>
    /// The check is receiving pings on schedule
    /// </summary>
    Up = 1,

    /// <summary>
    /// The check has missed expected pings and is considered down
    /// </summary>
    Down = 2,

    /// <summary>
    /// The check is paused and not being monitored
    /// </summary>
    Paused = 3,

    /// <summary>
    /// The check has started but not yet completed (awaiting success/fail signal)
    /// </summary>
    Started = 4,

    /// <summary>
    /// The check is in its grace period
    /// </summary>
    Grace = 5
}
