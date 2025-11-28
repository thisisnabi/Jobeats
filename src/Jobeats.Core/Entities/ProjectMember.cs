namespace Jobeats.Core.Entities;

/// <summary>
/// Represents a team member's association with a project
/// </summary>
public class ProjectMember
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The project
    /// </summary>
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    
    /// <summary>
    /// The user who is a member
    /// </summary>
    public Guid UserId { get; set; }
    public User? User { get; set; }
    
    /// <summary>
    /// Role of the member (owner, member, readonly)
    /// </summary>
    public string Role { get; set; } = "member";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
