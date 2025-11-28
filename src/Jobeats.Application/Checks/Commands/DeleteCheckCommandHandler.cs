using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Checks.Commands;

public class DeleteCheckCommandHandler : IRequestHandler<DeleteCheckCommand, DeleteCheckResult>
{
    private readonly ICheckRepository _checkRepository;
    
    public DeleteCheckCommandHandler(ICheckRepository checkRepository)
    {
        _checkRepository = checkRepository;
    }
    
    public async Task<DeleteCheckResult> Handle(DeleteCheckCommand request, CancellationToken cancellationToken)
    {
        var check = await _checkRepository.GetByIdAsync(request.Id, cancellationToken);
        if (check == null)
        {
            return new DeleteCheckResult(false, "Check not found");
        }
        
        await _checkRepository.DeleteAsync(request.Id, cancellationToken);
        
        return new DeleteCheckResult(true);
    }
}
