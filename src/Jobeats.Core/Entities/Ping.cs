using Jobeats.Core.Enums;

namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a ping received from a monitored job
/// </summary>
public class Ping
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The check this ping belongs to
    /// </summary>
    public Guid CheckId { get; set; }
    public Check? Check { get; set; }
    
    /// <summary>
    /// Type of the ping (success, fail, start, log)
    /// </summary>
    public PingType Type { get; set; }
    
    /// <summary>
    /// Optional request body content
    /// </summary>
    public string? Body { get; set; }
    
    /// <summary>
    /// Path to external storage if body is too large
    /// </summary>
    public string? ExternalBodyPath { get; set; }
    
    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Remote IP address of the request
    /// </summary>
    public string? RemoteIp { get; set; }
    
    /// <summary>
    /// HTTP method used (GET, POST, etc.)
    /// </summary>
    public string? Method { get; set; }
    
    /// <summary>
    /// Optional run ID for concurrent execution tracking
    /// </summary>
    public string? RunId { get; set; }
    
    /// <summary>
    /// Duration in milliseconds between start and success (if applicable)
    /// </summary>
    public int? DurationMs { get; set; }
    
    /// <summary>
    /// Exit status code (0-255) if this is an exit status ping
    /// </summary>
    public int? ExitStatus { get; set; }
    
    /// <summary>
    /// Timestamp when the ping was received
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
