namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a user in the system
/// </summary>
public class User
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// User's email address (unique identifier)
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Hashed password (if using password authentication)
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// API token for the user
    /// </summary>
    public string ApiToken { get; set; } = Guid.NewGuid().ToString("N");
    
    /// <summary>
    /// Limit for ping log entries per check
    /// </summary>
    public int PingLogLimit { get; set; } = 100;
    
    /// <summary>
    /// Whether the user's email is verified
    /// </summary>
    public bool IsEmailVerified { get; set; }
    
    /// <summary>
    /// Whether the user has enabled two-factor authentication
    /// </summary>
    public bool IsTwoFactorEnabled { get; set; }
    
    /// <summary>
    /// TOTP secret for two-factor authentication
    /// </summary>
    public string? TwoFactorSecret { get; set; }
    
    /// <summary>
    /// Timestamp of the last login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Navigation property for owned projects
    /// </summary>
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    
    /// <summary>
    /// Navigation property for project memberships
    /// </summary>
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
}
