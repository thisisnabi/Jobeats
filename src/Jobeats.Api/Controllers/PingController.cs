using Jobeats.Application.Pings.Commands;
using Jobeats.Core.Enums;
using Jobeats.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jobeats.Api.Controllers;

/// <summary>
/// Controller for receiving pings from monitored jobs
/// </summary>
[ApiController]
[Route("ping")]
public class PingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICheckRepository _checkRepository;

    public PingController(IMediator mediator, ICheckRepository checkRepository)
    {
        _mediator = mediator;
        _checkRepository = checkRepository;
    }

    /// <summary>
    /// Record a success ping for a check
    /// </summary>
    [HttpGet("{uuid}")]
    [HttpPost("{uuid}")]
    public Task<IActionResult> SuccessPing(
        Guid uuid,
        [FromQuery] string? rid = null,
        CancellationToken cancellationToken = default)
        => RecordPing(uuid, PingType.Success, rid, cancellationToken);

    /// <summary>
    /// Record a failure ping for a check
    /// </summary>
    [HttpGet("{uuid}/fail")]
    [HttpPost("{uuid}/fail")]
    public Task<IActionResult> FailPing(
        Guid uuid,
        [FromQuery] string? rid = null,
        CancellationToken cancellationToken = default)
        => RecordPing(uuid, PingType.Fail, rid, cancellationToken);

    /// <summary>
    /// Record a start signal for a check (starts timer)
    /// </summary>
    [HttpGet("{uuid}/start")]
    [HttpPost("{uuid}/start")]
    public Task<IActionResult> StartPing(
        Guid uuid,
        [FromQuery] string? rid = null,
        CancellationToken cancellationToken = default)
        => RecordPing(uuid, PingType.Start, rid, cancellationToken);

    /// <summary>
    /// Record a log-only ping (doesn't affect check status)
    /// </summary>
    [HttpGet("{uuid}/log")]
    [HttpPost("{uuid}/log")]
    public Task<IActionResult> LogPing(
        Guid uuid,
        [FromQuery] string? rid = null,
        CancellationToken cancellationToken = default)
        => RecordPing(uuid, PingType.Log, rid, cancellationToken);

    /// <summary>
    /// Record an exit status ping (0 = success, 1-255 = failure)
    /// </summary>
    [HttpGet("{uuid}/{exitStatus:int}")]
    [HttpPost("{uuid}/{exitStatus:int}")]
    public async Task<IActionResult> ExitStatusPing(
        Guid uuid,
        int exitStatus,
        [FromQuery] string? rid = null,
        CancellationToken cancellationToken = default)
    {
        if (exitStatus < 0 || exitStatus > 255)
        {
            return BadRequest("Exit status must be between 0 and 255");
        }

        var body = await GetRequestBodyAsync();
        var command = new RecordPingCommand(
            uuid,
            PingType.ExitStatus,
            body,
            Request.Headers.UserAgent.FirstOrDefault(),
            GetRemoteIp(),
            Request.Method,
            rid,
            exitStatus
        );

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok("OK");
    }

    private async Task<IActionResult> RecordPing(
        Guid uuid,
        PingType pingType,
        string? runId,
        CancellationToken cancellationToken)
    {
        var body = await GetRequestBodyAsync();
        var command = new RecordPingCommand(
            uuid,
            pingType,
            body,
            Request.Headers.UserAgent.FirstOrDefault(),
            GetRemoteIp(),
            Request.Method,
            runId
        );

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok("OK");
    }

    private async Task<string?> GetRequestBodyAsync()
    {
        if (Request.ContentLength is null or 0)
        {
            return null;
        }

        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        
        // Limit body size to prevent storage issues
        return body.Length > 10000 ? body[..10000] : body;
    }

    private string? GetRemoteIp()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
