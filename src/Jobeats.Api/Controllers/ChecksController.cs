using Jobeats.Application.Checks.Commands;
using Jobeats.Application.Checks.Queries;
using Jobeats.Core.Entities;
using Jobeats.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jobeats.Api.Controllers;

/// <summary>
/// API Controller for managing health checks
/// </summary>
[ApiController]
[Route("api/v1/checks")]
public class ChecksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IProjectRepository _projectRepository;
    private readonly IPingRepository _pingRepository;
    private readonly IFlipRepository _flipRepository;

    public ChecksController(
        IMediator mediator,
        IProjectRepository projectRepository,
        IPingRepository pingRepository,
        IFlipRepository flipRepository)
    {
        _mediator = mediator;
        _projectRepository = projectRepository;
        _pingRepository = pingRepository;
        _flipRepository = flipRepository;
    }

    /// <summary>
    /// List all checks for a project
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetChecks(CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var checks = await _mediator.Send(new GetChecksByProjectQuery(project.Id), cancellationToken);
        return Ok(new { checks = checks.Select(MapCheckToResponse) });
    }

    /// <summary>
    /// Create a new check
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCheck(
        [FromBody] CreateCheckRequest request,
        CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var command = new CreateCheckCommand(
            project.Id,
            request.Name,
            request.Description,
            request.Timeout ?? 86400,
            request.Grace ?? 3600,
            request.Timezone,
            request.Schedule,
            request.Tags
        );

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(result.CheckId!.Value), cancellationToken);
        return Created($"/api/v1/checks/{check!.Id}", MapCheckToResponse(check));
    }

    /// <summary>
    /// Get a specific check
    /// </summary>
    [HttpGet("{uuid}")]
    public async Task<IActionResult> GetCheck(Guid uuid, CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        return Ok(MapCheckToResponse(check));
    }

    /// <summary>
    /// Update a check
    /// </summary>
    [HttpPost("{uuid}")]
    public async Task<IActionResult> UpdateCheck(
        Guid uuid,
        [FromBody] UpdateCheckRequest request,
        CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        var command = new UpdateCheckCommand(
            uuid,
            request.Name ?? check.Name,
            request.Description ?? check.Description,
            request.Timeout ?? check.PeriodSeconds,
            request.Grace ?? check.GraceSeconds,
            request.Timezone ?? check.Timezone,
            request.Schedule ?? check.CronExpression,
            request.Tags ?? check.Tags
        );

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        return Ok(MapCheckToResponse(check!));
    }

    /// <summary>
    /// Delete a check
    /// </summary>
    [HttpDelete("{uuid}")]
    public async Task<IActionResult> DeleteCheck(Guid uuid, CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        var result = await _mediator.Send(new DeleteCheckCommand(uuid), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        return NoContent();
    }

    /// <summary>
    /// Pause monitoring for a check
    /// </summary>
    [HttpPost("{uuid}/pause")]
    public async Task<IActionResult> PauseCheck(Guid uuid, CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        var result = await _mediator.Send(new SetCheckPausedCommand(uuid, true), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        return Ok(MapCheckToResponse(check!));
    }

    /// <summary>
    /// Resume monitoring for a check
    /// </summary>
    [HttpPost("{uuid}/resume")]
    public async Task<IActionResult> ResumeCheck(Guid uuid, CancellationToken cancellationToken)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        var result = await _mediator.Send(new SetCheckPausedCommand(uuid, false), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }

        check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        return Ok(MapCheckToResponse(check!));
    }

    /// <summary>
    /// Get ping history for a check
    /// </summary>
    [HttpGet("{uuid}/pings")]
    public async Task<IActionResult> GetPings(
        Guid uuid,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        var pings = await _pingRepository.GetByCheckIdAsync(uuid, Math.Min(limit, 100), cancellationToken);
        return Ok(new
        {
            pings = pings.Select(p => new
            {
                type = p.Type.ToString().ToLowerInvariant(),
                date = p.CreatedAt,
                duration = p.DurationMs,
                body = p.Body
            })
        });
    }

    /// <summary>
    /// Get flip history for a check
    /// </summary>
    [HttpGet("{uuid}/flips")]
    public async Task<IActionResult> GetFlips(
        Guid uuid,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var apiKey = Request.Headers["X-Api-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(apiKey))
        {
            return Unauthorized("API key required");
        }

        var project = await _projectRepository.GetByApiKeyAsync(apiKey, cancellationToken);
        if (project == null)
        {
            return Unauthorized("Invalid API key");
        }

        var check = await _mediator.Send(new GetCheckByIdQuery(uuid), cancellationToken);
        if (check == null || check.ProjectId != project.Id)
        {
            return NotFound("Check not found");
        }

        var flips = await _flipRepository.GetByCheckIdAsync(uuid, Math.Min(limit, 100), cancellationToken);
        return Ok(new
        {
            flips = flips.Select(f => new
            {
                timestamp = f.CreatedAt,
                up = f.NewStatus == Core.Enums.CheckStatus.Up ? 1 : 0
            })
        });
    }

    private static object MapCheckToResponse(Check check)
    {
        return new
        {
            name = check.Name,
            slug = check.Slug,
            tags = check.Tags,
            desc = check.Description,
            grace = check.GraceSeconds,
            n_pings = check.TotalPings,
            status = check.Status.ToString().ToLowerInvariant(),
            last_ping = check.LastPingAt,
            next_ping = check.NextPingAt,
            manual_resume = false,
            methods = "",
            start_kw = "",
            success_kw = "",
            failure_kw = "",
            filter_subject = false,
            filter_body = false,
            ping_url = $"/ping/{check.Id}",
            update_url = $"/api/v1/checks/{check.Id}",
            pause_url = $"/api/v1/checks/{check.Id}/pause",
            resume_url = $"/api/v1/checks/{check.Id}/resume",
            channels = "*",
            schedule = check.CronExpression ?? $"every {check.PeriodSeconds} seconds",
            tz = check.Timezone ?? "UTC",
            subject = "",
            subject_fail = "",
            unique = Array.Empty<string>()
        };
    }
}

public record CreateCheckRequest(
    string Name,
    string? Description = null,
    string? Tags = null,
    int? Timeout = null,
    int? Grace = null,
    string? Schedule = null,
    string? Timezone = null,
    string[]? Channels = null,
    bool? Unique = null
);

public record UpdateCheckRequest(
    string? Name = null,
    string? Description = null,
    string? Tags = null,
    int? Timeout = null,
    int? Grace = null,
    string? Schedule = null,
    string? Timezone = null,
    string[]? Channels = null
);
