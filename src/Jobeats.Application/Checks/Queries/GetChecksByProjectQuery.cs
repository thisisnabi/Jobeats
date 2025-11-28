using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Checks.Queries;

public record GetChecksByProjectQuery(Guid ProjectId) : IRequest<IEnumerable<Check>>;

public class GetChecksByProjectQueryHandler : IRequestHandler<GetChecksByProjectQuery, IEnumerable<Check>>
{
    private readonly ICheckRepository _checkRepository;
    
    public GetChecksByProjectQueryHandler(ICheckRepository checkRepository)
    {
        _checkRepository = checkRepository;
    }
    
    public async Task<IEnumerable<Check>> Handle(GetChecksByProjectQuery request, CancellationToken cancellationToken)
    {
        return await _checkRepository.GetByProjectIdAsync(request.ProjectId, cancellationToken);
    }
}
