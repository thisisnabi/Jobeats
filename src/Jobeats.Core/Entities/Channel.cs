using Jobeats.Core.Enums;

namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a notification channel for sending alerts
/// </summary>
public class Channel
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Project this channel belongs to
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    /// <summary>
    /// Human-readable name for the channel
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of notification channel
    /// </summary>
    public ChannelKind Kind { get; set; }
    
    /// <summary>
    /// JSON configuration for the channel (varies by kind)
    /// </summary>
    public string? Configuration { get; set; }
    
    /// <summary>
    /// Whether the channel is disabled
    /// </summary>
    public bool IsDisabled { get; set; }
    
    /// <summary>
    /// Last error message from sending a notification
    /// </summary>
    public string? LastError { get; set; }
    
    /// <summary>
    /// Number of notifications sent through this channel
    /// </summary>
    public int NotifyCount { get; set; }
    
    /// <summary>
    /// Number of consecutive failures
    /// </summary>
    public int FailureCount { get; set; }
    
    /// <summary>
    /// Timestamp of the last notification sent
    /// </summary>
    public DateTime? LastNotifyAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property for check-channel associations
    /// </summary>
    public ICollection<CheckChannel> CheckChannels { get; set; } = new List<CheckChannel>();
}
