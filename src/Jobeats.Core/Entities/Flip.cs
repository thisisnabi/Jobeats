using Jobeats.Core.Enums;

namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a status change (flip) for a check
/// </summary>
public class Flip
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The check this flip belongs to
    /// </summary>
    public Guid CheckId { get; set; }
    public Check? Check { get; set; }
    
    /// <summary>
    /// Previous status before the flip
    /// </summary>
    public CheckStatus OldStatus { get; set; }
    
    /// <summary>
    /// New status after the flip
    /// </summary>
    public CheckStatus NewStatus { get; set; }
    
    /// <summary>
    /// Timestamp when the flip occurred
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Timestamp when notifications for this flip were processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    
    /// <summary>
    /// Whether notifications have been sent for this flip
    /// </summary>
    public bool IsProcessed => ProcessedAt.HasValue;
}
