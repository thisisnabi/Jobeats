using Jobeats.Core.Enums;

namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a health check that monitors scheduled jobs
/// </summary>
public class Check
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Human-readable name for the check
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional description of what this check monitors
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Project this check belongs to
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    /// <summary>
    /// Current status of the check
    /// </summary>
    public CheckStatus Status { get; set; } = CheckStatus.New;
    
    /// <summary>
    /// Expected time between pings in seconds (e.g., 3600 for hourly)
    /// </summary>
    public int PeriodSeconds { get; set; } = 86400; // Default: 24 hours
    
    /// <summary>
    /// Grace time in seconds before alerting after expected ping time
    /// </summary>
    public int GraceSeconds { get; set; } = 3600; // Default: 1 hour
    
    /// <summary>
    /// Optional timezone for the check (IANA timezone name)
    /// </summary>
    public string? Timezone { get; set; }
    
    /// <summary>
    /// Optional cron expression for more complex schedules
    /// </summary>
    public string? CronExpression { get; set; }
    
    /// <summary>
    /// Timestamp of the last received ping
    /// </summary>
    public DateTime? LastPingAt { get; set; }
    
    /// <summary>
    /// Expected timestamp for the next ping
    /// </summary>
    public DateTime? NextPingAt { get; set; }
    
    /// <summary>
    /// Timestamp when an alert should be triggered if no ping is received
    /// </summary>
    public DateTime? AlertAfterAt { get; set; }
    
    /// <summary>
    /// Unique slug for the ping URL
    /// </summary>
    public string Slug { get; set; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// Comma-separated tags for organizing checks
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// Timestamp of the last start ping (for tracking execution duration)
    /// </summary>
    public DateTime? LastStartAt { get; set; }
    
    /// <summary>
    /// Run ID of the last start ping (for concurrent execution tracking)
    /// </summary>
    public string? LastRunId { get; set; }
    
    /// <summary>
    /// Number of pings received
    /// </summary>
    public int TotalPings { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property for related pings
    /// </summary>
    public ICollection<Ping> Pings { get; set; } = new List<Ping>();
    
    /// <summary>
    /// Navigation property for related flips (status changes)
    /// </summary>
    public ICollection<Flip> Flips { get; set; } = new List<Flip>();
    
    /// <summary>
    /// Calculates the next expected ping time based on current time
    /// </summary>
    public void CalculateNextPing()
    {
        var now = DateTime.UtcNow;
        
        if (LastPingAt.HasValue)
        {
            NextPingAt = LastPingAt.Value.AddSeconds(PeriodSeconds);
            AlertAfterAt = NextPingAt.Value.AddSeconds(GraceSeconds);
        }
        else
        {
            NextPingAt = now.AddSeconds(PeriodSeconds);
            AlertAfterAt = NextPingAt.Value.AddSeconds(GraceSeconds);
        }
    }
    
    /// <summary>
    /// Records a ping and updates the check status
    /// </summary>
    public void RecordPing(PingType pingType, DateTime timestamp)
    {
        UpdatedAt = DateTime.UtcNow;
        TotalPings++;
        
        switch (pingType)
        {
            case PingType.Success:
            case PingType.ExitStatus:
                LastPingAt = timestamp;
                LastStartAt = null;
                LastRunId = null;
                Status = CheckStatus.Up;
                CalculateNextPing();
                break;
                
            case PingType.Fail:
                LastPingAt = timestamp;
                Status = CheckStatus.Down;
                break;
                
            case PingType.Start:
                LastStartAt = timestamp;
                Status = CheckStatus.Started;
                break;
                
            case PingType.Log:
            case PingType.Ignored:
                // Don't change status for log-only pings
                break;
        }
    }
}
