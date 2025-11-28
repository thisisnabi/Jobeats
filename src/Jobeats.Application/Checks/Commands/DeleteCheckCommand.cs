using MediatR;

namespace Jobeats.Application.Checks.Commands;

/// <summary>
/// Command to delete a health check
/// </summary>
public record DeleteCheckCommand(Guid Id) : IRequest<DeleteCheckResult>;

public record DeleteCheckResult(
    bool Success,
    string? ErrorMessage = null
);
