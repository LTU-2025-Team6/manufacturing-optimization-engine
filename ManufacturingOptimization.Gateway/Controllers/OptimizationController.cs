using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/optimization-requests")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOptimizationService _optimizationService;

        public OptimizationController(IOptimizationService optimizationService)
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
        /// Select preferred optimization strategy
        /// </summary>
        [HttpPut("{requestId}/strategy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SelectStrategy(Guid requestId, [FromBody] Guid strategyId)
        {
            await _optimizationService.SelectStrategyAsync(requestId, strategyId);
            return Ok();
        }

        /// <summary>
        /// Get optimization plan by request ID
        /// </summary>
        [HttpGet("{requestId}/plan")]
        [ProducesResponseType(typeof(OptimizationPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlan(Guid requestId)
        {
            var planDto = await _optimizationService.GetPlanAsync(requestId);
            return Ok(planDto);
        }
    }
}