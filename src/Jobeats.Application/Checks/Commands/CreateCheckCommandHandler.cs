using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using MediatR;

namespace Jobeats.Application.Checks.Commands;

public class CreateCheckCommandHandler : IRequestHandler<CreateCheckCommand, CreateCheckResult>
{
    private readonly ICheckRepository _checkRepository;
    private readonly IProjectRepository _projectRepository;
    
    public CreateCheckCommandHandler(
        ICheckRepository checkRepository,
        IProjectRepository projectRepository)
    {
        _checkRepository = checkRepository;
        _projectRepository = projectRepository;
    }
    
    public async Task<CreateCheckResult> Handle(CreateCheckCommand request, CancellationToken cancellationToken)
    {
        // Verify project exists
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
        {
            return new CreateCheckResult(false, "Project not found");
        }
        
        var check = new Check
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            PeriodSeconds = request.PeriodSeconds,
            GraceSeconds = request.GraceSeconds,
            Timezone = request.Timezone,
            CronExpression = request.CronExpression,
            Tags = request.Tags,
            Slug = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        // Calculate initial next ping time
        check.CalculateNextPing();
        
        await _checkRepository.AddAsync(check, cancellationToken);
        
        return new CreateCheckResult(
            true,
            CheckId: check.Id,
            Slug: check.Slug,
            PingUrl: $"/ping/{check.Id}"
        );
    }
}
