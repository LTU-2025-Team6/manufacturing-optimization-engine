namespace ManufacturingOptimization.Gateway.DTOs;

public record ProviderScheduleRequest(
    DateTime Start,
    DateTime End
);