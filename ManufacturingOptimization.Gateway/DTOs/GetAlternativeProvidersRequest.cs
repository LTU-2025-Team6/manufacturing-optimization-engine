namespace ManufacturingOptimization.Gateway.DTOs;

public record GetAlternativeProvidersRequest(
    DateTime ScheduleStartTime,
    DateTime ScheduleEndTime,
    DateTime? RequestedStartTime
);

public record ValidateProcessTimeRequest(
    Guid ProviderId,
    DateTime RequestedStartTime
);

public record ValidateProcessTimeResponse(
    bool IsValid,
    ProviderScheduleDto? ValidatedSchedule = null,
    IEnumerable<string>? Errors = null
);

public record UpdateStrategyRequest(
    Guid StrategyId,
    IEnumerable<StrategyUpdate> Updates
);

public record StrategyUpdate(
    Guid StepId,
    Guid? NewProviderId = null,
    DateTime? NewStartTime = null,
    DateTime? NewEndTime = null
);

public record UpdateStrategyResponse(
    OptimizationStrategyDto UpdatedStrategy,
    IEnumerable<string>? ValidationErrors = null
);

public record ConfirmStrategyResponse(
    OptimizationPlanDto ConfirmedPlan,
    IEnumerable<string>? ConfirmationErrors = null
);