using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Provider;
using Microsoft.AspNetCore.Mvc;

namespace ManufacturingOptimization.Gateway.Controllers;

[ApiController]
[Route("api/providers")]
public class ProviderController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProviderController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    /// <summary>
    /// Get list of all registered providers with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProviderPreviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProviders([FromQuery] PaginationRequest pagination)
    {
        var response = await _providerService.GetProvidersAsync(pagination);
        return Ok(response);
    }

    /// <summary>
    /// Get a single provider by Id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProvider(Guid id)
    {
        var provider = await _providerService.GetProviderByIdAsync(id);
        return Ok(provider);
    }

    /// <summary>
    /// Create a new provider
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateProvider([FromBody] CreateProviderRequest request)
    {
        var provider = await _providerService.CreateProviderAsync(request);
        return CreatedAtAction(nameof(GetProvider), new { id = provider.Id }, provider);
    }

    /// <summary>
    /// Update a provider
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> UpdateProvider(Guid id, [FromBody] UpdateProviderRequest request)
    {
        var updatedProvider = await _providerService.UpdateProviderAsync(id, request);
        return Ok(updatedProvider);
    }

    /// <summary>
    /// Delete a provider
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> DeleteProvider(Guid id)
    {
        await _providerService.DeleteProviderAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Toggle provider running status
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(ProviderPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ToggleProvider(Guid id, [FromBody] ToggleProviderRequest request)
    {
        var updatedProvider = await _providerService.ToggleProviderAsync(id, request.IsRunning);
        return Ok(updatedProvider);
    }

    /// <summary>
    /// Get provider schedule for a given period
    /// </summary>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(List<ProviderDayScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetProviderSchedule(Guid id, [FromQuery] ProviderScheduleRequest request)
    {
        var schedule = await _providerService.GetProviderScheduleAsync(id, request);
        return Ok(schedule);
    }

    /// <summary>
    /// Get execution details from a provider
    /// </summary>
    [HttpGet("{providerId}/executions/{executionId}")]
    [ProducesResponseType(typeof(ExecutionDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetExecutionDetails(Guid providerId, Guid executionId)
    {
        var executionDetails = await _providerService.GetExecutionDetailsAsync(providerId, executionId);
        return Ok(executionDetails);
    }
}