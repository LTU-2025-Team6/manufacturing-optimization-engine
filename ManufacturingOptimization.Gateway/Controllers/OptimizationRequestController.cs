using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;
using ManufacturingOptimization.Gateway.DTOs.OptimizationRequest;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/optimization-requests")]
public class OptimizationRequestController : ControllerBase
{
    private readonly IOptimizationRequestService _optimizationService;

    public OptimizationRequestController(IOptimizationRequestService optimizationService)
    {
        _optimizationService = optimizationService;
    }

    /// <summary>
    /// Submit optimization request
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> RequestOptimizationPlan([FromBody] OptimizationRequestDto request)
    {
        var requestId = await _optimizationService.RequestOptimizationPlanAsync(request);
        return Accepted(requestId);
    }

    /// <summary>
    /// Get optimization request by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OptimizationRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetRequest(Guid id)
    {
        var requestDto = await _optimizationService.GetRequestAsync(id);
        return Ok(requestDto);
    }
}