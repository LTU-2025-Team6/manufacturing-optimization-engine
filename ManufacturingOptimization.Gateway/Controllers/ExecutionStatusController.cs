using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
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
    /// Get summary of all execution plans with their progress.
    /// </summary>
    [HttpGet("plans")]
    public async Task<ActionResult<List<ExecutionPlanSummaryDto>>> GetAllPlans()
    {
        var plans = await _executionStatusService.GetAllPlansAsync();
        return Ok(plans);
    }

    /// <summary>
    /// Get detailed information about a specific execution plan.
    /// </summary>
    [HttpGet("plans/{id:guid}")]
    public async Task<ActionResult<ExecutionPlanDetailDto>> GetPlanDetail(Guid id)
    {
        var detail = await _executionStatusService.GetPlanDetailAsync(id);
        return Ok(detail);
    }

    /// <summary>
    /// Get all plans currently in progress.
    /// </summary>
    [HttpGet("plans/in-progress")]
    public async Task<ActionResult<List<ExecutionPlanSummaryDto>>> GetInProgressPlans()
    {
        var plans = await _executionStatusService.GetInProgressPlansAsync();
        return Ok(plans);
    }

    /// <summary>
    /// Get execution steps for a specific plan.
    /// </summary>
    [HttpGet("plans/{id:guid}/steps")]
    public async Task<ActionResult<List<ExecutionStepDto>>> GetPlanSteps(Guid id)
    {
        var steps = await _executionStatusService.GetPlanStepsAsync(id);
        return Ok(steps);
    }

    /// <summary>
    /// Get overall execution statistics and summary.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ExecutionSummaryDto>> GetExecutionSummary()
    {
        var summary = await _executionStatusService.GetExecutionSummaryAsync();
        return Ok(summary);
    }
}
