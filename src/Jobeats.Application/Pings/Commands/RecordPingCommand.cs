using Jobeats.Core.Enums;
using MediatR;

namespace Jobeats.Application.Pings.Commands;

/// <summary>
/// Command to record a ping for a health check
/// </summary>
public record RecordPingCommand(
    Guid CheckId,
    PingType PingType,
    string? Body = null,
    string? UserAgent = null,
    string? RemoteIp = null,
    string? Method = null,
    string? RunId = null,
    int? ExitStatus = null
) : IRequest<RecordPingResult>;

public record RecordPingResult(
    bool Success,
    string? ErrorMessage = null,
    Guid? PingId = null,
    CheckStatus? NewStatus = null
);
