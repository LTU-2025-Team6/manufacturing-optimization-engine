namespace ManufacturingOptimization.Gateway.DTOs;

public record ToggleProviderRequest(bool IsRunning);

public record UpdateProviderRequest(
    string Name,
    bool AutoStart,
    List<UpdateProcessCapabilityRequest> ProcessCapabilities,
    UpdateTechnicalCapabilitiesRequest TechnicalCapabilities,
    UpdateWorkingHoursRequest WorkingHours
);

public record UpdateProcessCapabilityRequest(
    string Process,
    double CostPerHour,
    double SpeedMultiplier,
    double QualityScore,
    double EnergyConsumptionKwhPerHour,
    double CarbonIntensityKgCO2PerKwh,
    bool UsesRenewableEnergy
);

public record UpdateTechnicalCapabilitiesRequest(
    double AxisHeight,
    double Power,
    double Tolerance
);

public record UpdateWorkingHoursRequest(
    List<string> WorkingDays,
    int WorkDayStartHour,
    int WorkDayEndHour,
    bool Is24x7,
    List<UpdateBreakPeriodRequest> Breaks
);

public record UpdateBreakPeriodRequest(
    int StartHour,
    int StartMinute,
    int DurationMinutes,
    string Name
);