using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs.Strategy;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/strategies")]
public class OptimizationStrategyController : ControllerBase
{
    private readonly IOptimizationStrategyService _strategyService;

    public OptimizationStrategyController(IOptimizationStrategyService strategyService)
    {
        _strategyService = strategyService;
    }

    /// <summary>
    /// Confirm an optimization strategy and schedule confirmed slots with providers
    /// </summary>
    [HttpPost("{strategyId}/confirm")]
    [ProducesResponseType(typeof(ConfirmStrategyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ConfirmStrategy(Guid strategyId)
    {
        var response = await _strategyService.ConfirmStrategy(strategyId);
        return Ok(response);
    }
}
