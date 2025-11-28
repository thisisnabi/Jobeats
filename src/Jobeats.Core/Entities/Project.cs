namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a project that groups health checks
/// </summary>
public class Project
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Human-readable name for the project
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// API key for accessing this project's checks
    /// </summary>
    public string ApiKey { get; set; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// Owner user ID
    /// </summary>
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }
    
    /// <summary>
    /// Unique key for generating public badge URLs
    /// </summary>
    public string BadgeKey { get; set; } = Guid.NewGuid().ToString("N")[..12];
    
    /// <summary>
    /// Optional description of the project
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Next date for sending nag reminders about down checks
    /// </summary>
    public DateTime? NextNagAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property for checks in this project
    /// </summary>
    public ICollection<Check> Checks { get; set; } = new List<Check>();
    
    /// <summary>
    /// Navigation property for channels configured for this project
    /// </summary>
    public ICollection<Channel> Channels { get; set; } = new List<Channel>();
    
    /// <summary>
    /// Navigation property for team members
    /// </summary>
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}
