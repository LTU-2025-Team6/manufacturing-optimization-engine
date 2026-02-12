using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers
{
    [ApiController]
    [Route("api/strategies")]
    public class OptimizationStrategyController : ControllerBase
    {
        private readonly IOptimizationStrategyService _strategyService;

        public OptimizationStrategyController(IOptimizationStrategyService strategyService)
        {
            _strategyService = strategyService;
        }

        [HttpPost("{strategyId}/steps/{stepId}/alternative-providers")]
        [ProducesResponseType(typeof(IEnumerable<AlternativeProviderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetAlternativeProviders(Guid strategyId, Guid stepId, [FromBody] GetAlternativeProvidersRequest request)
        {
            var providers = await _strategyService.GetAlternativeProviders(strategyId, stepId, request.ScheduleStartTime, request.ScheduleEndTime);
            return Ok(providers);
        }

        [HttpPost("{strategyId}/steps/{stepId}/validate-time")]
        [ProducesResponseType(typeof(IEnumerable<AlternativeProviderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ValidateAlternativeProcessTime(Guid strategyId, Guid stepId, [FromBody] ValidateProcessTimeRequest request)
        {
            var response = await _strategyService.ValidateAlternativeProcessTime(strategyId, stepId, request);
            return Ok(response);
        }

        [HttpPut("{strategyId}")]
        [ProducesResponseType(typeof(UpdateStrategyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStrategy(Guid strategyId, [FromBody] UpdateStrategyRequest request)
        {
            var response = await _strategyService.UpdateStrategy(strategyId, request);
            return Ok(response);
        }

        [HttpPost("{strategyId}/confirm")]
        [ProducesResponseType(typeof(ConfirmStrategyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmStrategy(Guid strategyId)
        {
            var response = await _strategyService.ConfirmStrategy(strategyId);
            return Ok(response);
        }
    }
}
