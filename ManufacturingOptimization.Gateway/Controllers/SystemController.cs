using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs.System;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    private readonly ISystemReadinessService _readinessService;
    private readonly ISimulationTimeService _timeService;

    public SystemController(
        ISystemReadinessService readinessService,
        ISimulationTimeService timeService)
    {
        _readinessService = readinessService;
        _timeService = timeService;
    }

    /// <summary>
    /// Check if the system is started and ready
    /// </summary>
    [HttpGet("ready")]
    public IActionResult IsSystemReady()
    {
        var isReady = _readinessService.IsSystemReady && _readinessService.IsProvidersReady;
        return Ok(new { ready = isReady });
    }

    /// <summary>
    /// Get current simulation time and speed multiplier
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SimulationTimeDto), StatusCodes.Status200OK)]
    public IActionResult GetTime()
    {
        return Ok(_timeService.GetCurrentTime());
    }

    /// <summary>
    /// Set simulation time and/or speed multiplier.
    /// Updates all services (Engine, ProviderSimulators) via RabbitMQ.
    /// </summary>
    /// <param name="request">New time and/or speed. Null values keep current settings.</param>
    [HttpPut]
    [ProducesResponseType(typeof(SimulationTimeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult SetTime([FromBody] SetSimulationTimeRequest request)
    {
        _timeService.SetTime(request.SimulatedUtcNow?.UtcDateTime, request.SpeedMultiplier);
        return Ok(_timeService.GetCurrentTime());
    }
}
