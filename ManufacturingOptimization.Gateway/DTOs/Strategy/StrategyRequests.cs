using ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;
using ManufacturingOptimization.Gateway.DTOs.Provider;

namespace ManufacturingOptimization.Gateway.DTOs.Strategy;

/// <summary>
/// Request body for POST .../alternatives — search window for alternative providers.
/// </summary>
public record GetAlternativesRequest(
    DateTime ScheduleWindowStart,
    DateTime ScheduleWindowEnd
);

/// <summary>
/// Request body for POST .../validate-slot — checks whether a proposed start time fits the provider's schedule.
/// </summary>
public record ValidateSlotRequest(
    Guid ProviderId,
    DateTime RequestedStart,
    double DurationHours
);

/// <summary>
/// Response for validate-slot. Returns the built schedule when valid.
/// </summary>
public record ValidateSlotResponse(
    bool IsValid,
    ProviderScheduleDto? AllocatedSchedule,
    List<string>? Errors
);

/// <summary>
/// Single step change within UpdateStrategyRequest. ProviderId is omitted when only the schedule changes.
/// </summary>
public record StepUpdateDto(
    Guid StepId,
    Guid? ProviderId,
    DateTime ScheduledStart,
    DateTime ScheduledEnd
);

/// <summary>
/// Request body for PUT /api/plans/{planId}/strategy.
/// </summary>
public record UpdateStrategyRequest(
    List<StepUpdateDto> StepUpdates
);

/// <summary>
/// Response after updating strategy — includes the recalculated strategy.
/// </summary>
public record UpdateStrategyResponse(
    OptimizationStrategyDto UpdatedStrategy,
    List<string>? ValidationErrors
);

/// <summary>
/// Response after confirming strategy.
/// </summary>
public record ConfirmStrategyResponse(
    bool Success,
    string? ErrorMessage
);
