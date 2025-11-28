using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Checks.Commands;

public class UpdateCheckCommandHandler : IRequestHandler<UpdateCheckCommand, UpdateCheckResult>
{
    private readonly ICheckRepository _checkRepository;
    
    public UpdateCheckCommandHandler(ICheckRepository checkRepository)
    {
        _checkRepository = checkRepository;
    }
    
    public async Task<UpdateCheckResult> Handle(UpdateCheckCommand request, CancellationToken cancellationToken)
    {
        var check = await _checkRepository.GetByIdAsync(request.Id, cancellationToken);
        if (check == null)
        {
            return new UpdateCheckResult(false, "Check not found");
        }
        
        check.Name = request.Name;
        check.Description = request.Description;
        check.PeriodSeconds = request.PeriodSeconds;
        check.GraceSeconds = request.GraceSeconds;
        check.Timezone = request.Timezone;
        check.CronExpression = request.CronExpression;
        check.Tags = request.Tags;
        check.UpdatedAt = DateTime.UtcNow;
        
        // Recalculate next ping if schedule changed
        check.CalculateNextPing();
        
        await _checkRepository.UpdateAsync(check, cancellationToken);
        
        return new UpdateCheckResult(true);
    }
}
