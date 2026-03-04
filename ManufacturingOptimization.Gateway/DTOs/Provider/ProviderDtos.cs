namespace ManufacturingOptimization.Gateway.DTOs.Provider;

/// <summary>
/// Preview information about a provider for list views.
/// </summary>
public class ProviderPreviewDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
}

/// <summary>
/// Complete provider information including capabilities and working hours.
/// </summary>
public class ProviderDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool AutoStart { get; set; }
    public bool IsRunning { get; set; }
    public List<ProcessCapabilityDto> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilitiesDto TechnicalCapabilities { get; set; } = new();
    public ProviderWorkingHoursDto WorkingHours { get; set; } = new();
}

/// <summary>
/// Alternative provider option with estimate and schedule.
/// </summary>
public class AlternativeProviderDto
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public ProcessEstimateDto Estimate { get; set; } = null!;
    public ProviderScheduleDto Schedule { get; set; } = null!;
}

public class TechnicalCapabilitiesDto
{
    public double AxisHeight { get; set; }
    public double Power { get; set; }
    public double Tolerance { get; set; }
}

public class ProcessCapabilityDto
{
    public string Process { get; set; } = string.Empty;
    public double CostPerHour { get; set; }
    public double SpeedMultiplier { get; set; }
    public double QualityScore { get; set; }
    public double EnergyConsumptionKwhPerHour { get; set; }
    public double CarbonIntensityKgCO2PerKwh { get; set; }
    public bool UsesRenewableEnergy { get; set; }
}

public class ProviderWorkingHoursDto
{
    public List<string> WorkingDays { get; set; } = new();
    public int WorkDayStartHour { get; set; }
    public int WorkDayEndHour { get; set; }
    public bool Is24x7 { get; set; }
    public List<ProviderBreakPeriodDto> Breaks { get; set; } = new();
}

public class ProviderBreakPeriodDto
{
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int DurationMinutes { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Process estimate for a step.
/// </summary>
public class ProcessEstimateDto
{
    public Guid Id { get; set; }
    public decimal Cost { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public double Duration { get; set; }
}

/// <summary>
/// Provider schedule information.
/// </summary>
public class ProviderScheduleDto
{
    public DateTime StartWorkingTime { get; set; }
    public DateTime EndWorkingTime { get; set; }
    public List<ProviderScheduleSegmentDto> Segments { get; set; } = [];
}

public class ProviderDayScheduleDto
{
    public DateTime Date { get; set; }
    public List<ProviderScheduleSegmentDto> Segments { get; set; } = [];
}

public class ProviderScheduleSegmentDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string SegmentType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the execution if this segment represents an occupied time slot.
    /// Null for FreeSpace and Break segments.
    /// </summary>
    public Guid? ExecutionId { get; set; }
}
