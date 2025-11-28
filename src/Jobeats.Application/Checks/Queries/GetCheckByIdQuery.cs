using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Checks.Queries;

public record GetCheckByIdQuery(Guid Id) : IRequest<Check?>;

public class GetCheckByIdQueryHandler : IRequestHandler<GetCheckByIdQuery, Check?>
{
    private readonly ICheckRepository _checkRepository;
    
    public GetCheckByIdQueryHandler(ICheckRepository checkRepository)
    {
        _checkRepository = checkRepository;
    }
    
    public async Task<Check?> Handle(GetCheckByIdQuery request, CancellationToken cancellationToken)
    {
        return await _checkRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}
