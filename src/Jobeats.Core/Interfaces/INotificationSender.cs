using Jobeats.Core.Entities;
using Jobeats.Core.Enums;

namespace Jobeats.Core.Interfaces;

/// <summary>
/// Interface for notification senders
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// The kind of channel this sender supports
    /// </summary>
    ChannelKind Kind { get; }
    
    /// <summary>
    /// Send a notification through this channel
    /// </summary>
    Task<bool> SendAsync(Channel channel, Check check, Flip flip, CancellationToken cancellationToken = default);
}
