using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Checks.Commands;

/// <summary>
/// Command to pause/resume monitoring for a check
/// </summary>
public record SetCheckPausedCommand(Guid Id, bool Paused) : IRequest<SetCheckPausedResult>;

public record SetCheckPausedResult(bool Success, string? ErrorMessage = null);

public class SetCheckPausedCommandHandler : IRequestHandler<SetCheckPausedCommand, SetCheckPausedResult>
{
    private readonly ICheckRepository _checkRepository;
    
    public SetCheckPausedCommandHandler(ICheckRepository checkRepository)
    {
        _checkRepository = checkRepository;
    }
    
    public async Task<SetCheckPausedResult> Handle(SetCheckPausedCommand request, CancellationToken cancellationToken)
    {
        var check = await _checkRepository.GetByIdAsync(request.Id, cancellationToken);
        if (check == null)
        {
            return new SetCheckPausedResult(false, "Check not found");
        }
        
        if (request.Paused)
        {
            check.Status = CheckStatus.Paused;
            check.NextPingAt = null;
            check.AlertAfterAt = null;
        }
        else
        {
            check.Status = CheckStatus.New;
            check.CalculateNextPing();
        }
        
        check.UpdatedAt = DateTime.UtcNow;
        await _checkRepository.UpdateAsync(check, cancellationToken);
        
        return new SetCheckPausedResult(true);
    }
}
