using MediatR;

namespace Jobeats.Application.Checks.Commands;

/// <summary>
/// Command to create a new health check
/// </summary>
public record CreateCheckCommand(
    Guid ProjectId,
    string Name,
    string? Description = null,
    int PeriodSeconds = 86400,
    int GraceSeconds = 3600,
    string? Timezone = null,
    string? CronExpression = null,
    string? Tags = null
) : IRequest<CreateCheckResult>;

public record CreateCheckResult(
    bool Success,
    string? ErrorMessage = null,
    Guid? CheckId = null,
    string? Slug = null,
    string? PingUrl = null
);
