namespace Jobeats.Core.Entities;

/// <summary>
/// Junction table for many-to-many relationship between checks and channels
/// </summary>
public class CheckChannel
{
    public Guid CheckId { get; set; }
    public Check? Check { get; set; }
    
    public Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }
}
