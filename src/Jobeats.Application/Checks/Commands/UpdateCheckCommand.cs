using MediatR;

namespace Jobeats.Application.Checks.Commands;

/// <summary>
/// Command to update an existing health check
/// </summary>
public record UpdateCheckCommand(
    Guid Id,
    string Name,
    string? Description = null,
    int PeriodSeconds = 86400,
    int GraceSeconds = 3600,
    string? Timezone = null,
    string? CronExpression = null,
    string? Tags = null
) : IRequest<UpdateCheckResult>;

public record UpdateCheckResult(
    bool Success,
    string? ErrorMessage = null
);
