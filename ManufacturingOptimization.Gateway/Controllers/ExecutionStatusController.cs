using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Execution;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

/// <summary>
/// Controller for monitoring execution status of optimization plans.
/// </summary>
[ApiController]
[Route("api/execution")]
public class ExecutionStatusController : ControllerBase
{
    private readonly IExecutionStatusService _executionStatusService;

    public ExecutionStatusController(IExecutionStatusService executionStatusService)
    {
        _executionStatusService = executionStatusService;
    }

    /// <summary>
    /// Get all execution plans with their progress and pagination
    /// </summary>
    [HttpGet("plans")]
    [ProducesResponseType(typeof(PagedResult<ExecutionPlanSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPlans([FromQuery] PaginationRequest pagination)
    {
        var plans = await _executionStatusService.GetAllPlansAsync(pagination);
        return Ok(plans);
    }

    /// <summary>
    /// Get detailed information about a specific execution plan
    /// </summary>
    [HttpGet("plans/{id:guid}")]
    [ProducesResponseType(typeof(ExecutionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanDetail(Guid id)
    {
        var detail = await _executionStatusService.GetPlanDetailAsync(id);
        return Ok(detail);
    }

    /// <summary>
    /// Get all plans currently in progress
    /// </summary>
    [HttpGet("plans/in-progress")]
    [ProducesResponseType(typeof(PagedResult<ExecutionPlanSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInProgressPlans([FromQuery] PaginationRequest pagination)
    {
        var plans = await _executionStatusService.GetInProgressPlansAsync(pagination);
        return Ok(plans);
    }

    /// <summary>
    /// Get execution steps for a specific plan
    /// </summary>
    [HttpGet("plans/{id:guid}/steps")]
    [ProducesResponseType(typeof(List<ExecutionStepDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanSteps(Guid id)
    {
        var steps = await _executionStatusService.GetPlanStepsAsync(id);
        return Ok(steps);
    }

    /// <summary>
    /// Get overall execution statistics and summary
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ExecutionSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExecutionSummary()
    {
        var summary = await _executionStatusService.GetExecutionSummaryAsync();
        return Ok(summary);
    }
}
