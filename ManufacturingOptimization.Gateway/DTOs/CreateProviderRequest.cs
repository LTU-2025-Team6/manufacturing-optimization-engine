namespace ManufacturingOptimization.Gateway.DTOs;

public record CreateProviderRequest(
    string Type,
    string Name,
    bool AutoStart,
    List<CreateProcessCapabilityRequest> ProcessCapabilities,
    CreateTechnicalCapabilitiesRequest TechnicalCapabilities,
    CreateWorkingHoursRequest WorkingHours
);

public record CreateProcessCapabilityRequest(
    string Process,
    double CostPerHour,
    double SpeedMultiplier,
    double QualityScore,
    double EnergyConsumptionKwhPerHour,
    double CarbonIntensityKgCO2PerKwh,
    bool UsesRenewableEnergy
);

public record CreateTechnicalCapabilitiesRequest(
    double AxisHeight,
    double Power,
    double Tolerance
);

public record CreateWorkingHoursRequest(
    List<string> WorkingDays,
    int WorkDayStartHour,
    int WorkDayEndHour,
    bool Is24x7,
    List<CreateBreakPeriodRequest> Breaks
);

public record CreateBreakPeriodRequest(
    int StartHour,
    int StartMinute,
    int DurationMinutes,
    string Name
);
