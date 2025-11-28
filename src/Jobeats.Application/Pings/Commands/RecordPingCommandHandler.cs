using Jobeats.Core.Entities;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Pings.Commands;

public class RecordPingCommandHandler : IRequestHandler<RecordPingCommand, RecordPingResult>
{
    private readonly ICheckRepository _checkRepository;
    private readonly IPingRepository _pingRepository;
    private readonly IFlipRepository _flipRepository;
    
    public RecordPingCommandHandler(
        ICheckRepository checkRepository,
        IPingRepository pingRepository,
        IFlipRepository flipRepository)
    {
        _checkRepository = checkRepository;
        _pingRepository = pingRepository;
        _flipRepository = flipRepository;
    }
    
    public async Task<RecordPingResult> Handle(RecordPingCommand request, CancellationToken cancellationToken)
    {
        var check = await _checkRepository.GetByIdAsync(request.CheckId, cancellationToken);
        if (check == null)
        {
            return new RecordPingResult(false, "Check not found");
        }
        
        if (check.Status == CheckStatus.Paused)
        {
            return new RecordPingResult(false, "Check is paused");
        }
        
        var now = DateTime.UtcNow;
        var oldStatus = check.Status;
        
        // Calculate duration if this is a success/fail ping and there was a start
        int? durationMs = null;
        if ((request.PingType == PingType.Success || request.PingType == PingType.Fail) && 
            check.LastStartAt.HasValue)
        {
            // Only calculate duration if run IDs match (or no run ID specified)
            if (string.IsNullOrEmpty(request.RunId) || request.RunId == check.LastRunId)
            {
                durationMs = (int)(now - check.LastStartAt.Value).TotalMilliseconds;
            }
        }
        
        // Create the ping record
        var ping = new Ping
        {
            Id = Guid.NewGuid(),
            CheckId = request.CheckId,
            Type = request.PingType,
            Body = request.Body,
            UserAgent = request.UserAgent,
            RemoteIp = request.RemoteIp,
            Method = request.Method,
            RunId = request.RunId,
            DurationMs = durationMs,
            ExitStatus = request.ExitStatus,
            CreatedAt = now
        };
        
        await _pingRepository.AddAsync(ping, cancellationToken);
        
        // Update check based on ping type
        if (request.PingType == PingType.Start)
        {
            check.LastRunId = request.RunId;
        }
        
        check.RecordPing(request.PingType, now);
        
        // Handle exit status pings - status code > 0 means failure
        if (request.PingType == PingType.ExitStatus && request.ExitStatus.HasValue && request.ExitStatus.Value > 0)
        {
            check.Status = CheckStatus.Down;
        }
        
        await _checkRepository.UpdateAsync(check, cancellationToken);
        
        // Create flip record if status changed
        if (oldStatus != check.Status && ShouldCreateFlip(oldStatus, check.Status))
        {
            var flip = new Flip
            {
                Id = Guid.NewGuid(),
                CheckId = request.CheckId,
                OldStatus = oldStatus,
                NewStatus = check.Status,
                CreatedAt = now
            };
            await _flipRepository.AddAsync(flip, cancellationToken);
        }
        
        return new RecordPingResult(true, PingId: ping.Id, NewStatus: check.Status);
    }
    
    private static bool ShouldCreateFlip(CheckStatus oldStatus, CheckStatus newStatus)
    {
        // Only create flips for significant status changes
        return (oldStatus != CheckStatus.New && newStatus == CheckStatus.Down) || // down transition
               (oldStatus == CheckStatus.Down && newStatus == CheckStatus.Up); // recovery
    }
}
