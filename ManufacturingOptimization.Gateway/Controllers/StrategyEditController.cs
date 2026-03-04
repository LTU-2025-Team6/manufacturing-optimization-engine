using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs.Provider;
using ManufacturingOptimization.Gateway.DTOs.Strategy;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/plans/{planId:guid}/strategy")]
public class StrategyEditController : ControllerBase
{
    private readonly IOptimizationStrategyService _strategyService;

    public StrategyEditController(IOptimizationStrategyService strategyService)
    {
        _strategyService = strategyService;
    }

    /// <summary>
    /// Get alternative providers capable of executing a step within the given schedule window.
    /// Results are cached server-side and used by validate-slot and update.
    /// </summary>
    [HttpPost("steps/{stepId:guid}/alternatives")]
    [ProducesResponseType(typeof(IEnumerable<AlternativeProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetAlternativeProviders(Guid planId, Guid stepId, [FromBody] GetAlternativesRequest request)
    {
        var providers = await _strategyService.GetAlternativeProvidersAsync(planId, stepId, request.ScheduleWindowStart, request.ScheduleWindowEnd);
        return Ok(providers);
    }

    /// <summary>
    /// Validate whether a proposed start time fits the provider's available schedule.
    /// Returns the built allocated schedule when valid.
    /// </summary>
    [HttpPost("steps/{stepId:guid}/validate-slot")]
    [ProducesResponseType(typeof(ValidateSlotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateSlot(Guid planId, Guid stepId, [FromBody] ValidateSlotRequest request)
    {
        var result = await _strategyService.ValidateSlotAsync(planId, stepId, request);
        return Ok(result);
    }

    /// <summary>
    /// Update provider and/or schedule for one or more steps in the plan's selected strategy.
    /// Returns the recalculated strategy.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(UpdateStrategyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStrategy(Guid planId, [FromBody] UpdateStrategyRequest request)
    {
        var response = await _strategyService.UpdateStrategyAsync(planId, request);
        return Ok(response);
    }
}
