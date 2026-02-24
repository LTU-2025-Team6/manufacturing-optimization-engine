namespace ManufacturingOptimization.Gateway.DTOs;

public class ProviderPreviewDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
}

public class ProviderDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool AutoStart { get; set; }
    public bool IsRunning { get; set; }
    public bool Enabled { get; set; }
    public List<ProcessCapabilityDto> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilitiesDto TechnicalCapabilities { get; set; } = new();
    public ProviderWorkingHoursDto WorkingHours { get; set; } = new();
}

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