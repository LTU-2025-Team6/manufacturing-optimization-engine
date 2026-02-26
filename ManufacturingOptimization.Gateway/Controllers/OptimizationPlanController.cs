using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Services;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/plans")]
    public class OptimizationPlanController : ControllerBase
    {
        private readonly IOptimizationPlanService _optimizationPlanService;

        public OptimizationPlanController(IOptimizationPlanService optimizationPlanService)
        {
            _optimizationPlanService = optimizationPlanService;
        }

        /// <summary>
        /// Get list of all registered providers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OptimizationPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _optimizationPlanService.GetAllAsync();
            return Ok(response);
        }

        /// <summary>
        /// Get a single provider by Id
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OptimizationPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plan = await _optimizationPlanService.GetByIdAsync(id);
            return Ok(plan);
        }

        /// <summary>
        /// Cancel a confirmed optimization plan and revert it to Ready status
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(CancelPlanResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CancelPlan(Guid id)
        {
            var response = await _optimizationPlanService.CancelPlanAsync(id);
            return Ok(response);
        }

        /// <summary>
        /// Delete an optimization plan (only if not confirmed)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> DeletePlan(Guid id)
        {
            await _optimizationPlanService.DeletePlanAsync(id);
            return NoContent();
        }
    }
}