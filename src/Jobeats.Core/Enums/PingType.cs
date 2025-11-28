namespace Jobeats.Core.Enums;

/// <summary>
/// Represents the type of ping received
/// </summary>
public enum PingType
{
    /// <summary>
    /// A successful ping indicating the job completed successfully
    /// </summary>
    Success = 0,

    /// <summary>
    /// A failure ping indicating the job failed
    /// </summary>
    Fail = 1,

    /// <summary>
    /// A start ping indicating the job has started
    /// </summary>
    Start = 2,

    /// <summary>
    /// A log-only ping that doesn't affect the check status
    /// </summary>
    Log = 3,

    /// <summary>
    /// A ping that was ignored due to configuration
    /// </summary>
    Ignored = 4,

    /// <summary>
    /// An exit status ping with status code 0-255
    /// </summary>
    ExitStatus = 5
}
