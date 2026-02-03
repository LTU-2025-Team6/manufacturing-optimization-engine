using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Gateway.Data;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Gateway.Services;

/// <summary>
/// Background service for managing database lifecycle.
/// Clears providers on startup and shutdown since they re-register on every start.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    // Toggle to recreate database on every startup (useful during development)
    private const bool RECREATE_DATABASE_ON_STARTUP = false;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseManagementService> _logger;
    private readonly IMapper _mapper;
    private readonly OrchestrationSettings _orchestrationSettings;

    public DatabaseManagementService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseManagementService> logger,
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

            if (RECREATE_DATABASE_ON_STARTUP)
            {
                // Clear for development purposes
                await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            await dbContext.Database.MigrateAsync(cancellationToken);
            await PrepareProviders(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Gateway database");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

            // Do cleanup here if necessary
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup Gateway database");
        }

        return Task.CompletedTask;
    }

    private async Task PrepareProviders(GatewayDbContext dbContext, CancellationToken cancellationToken)
    {
        if (_orchestrationSettings.IsDevelopmentMode)
        {
            await dbContext.Providers.ExecuteDeleteAsync(cancellationToken);
            return;
        }

        // Remove providers left from development mode
        await dbContext.Providers
            .Where(p => p.EnvironmentSource != _orchestrationSettings.Mode)
            .ExecuteDeleteAsync(cancellationToken);

        // Mark all providers as not running
        await dbContext.Providers
            .Where(p => p.IsRunning)
            .ExecuteUpdateAsync(p => p.SetProperty(p => p.IsRunning, false), cancellationToken);

        // If providers already exist, do not add static ones
        var existingProvidersCount = await dbContext.Providers.CountAsync(cancellationToken);
        if (existingProvidersCount > 0)
            return;

        var staticProviders = GetStaticProviders();
        var providerEntities = _mapper.Map<List<ProviderEntity>>(staticProviders);

        await dbContext.Providers.AddRangeAsync(providerEntities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<ProviderModel> GetStaticProviders()
    {
        return new List<ProviderModel>
        {
            new ProviderModel
            {
                Id = Guid.Parse("f93a2956-e430-4b50-b688-991b5c33f5f4"),
                Type = "MainRemanufacturingCenter",
                Name = "Main Remanufacturing Center",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Cleaning, CostPerHour = 50.0m, SpeedMultiplier = 1.0, QualityScore = 0.90, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.35, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Disassembly, CostPerHour = 75.0m, SpeedMultiplier = 0.95, QualityScore = 0.95, EnergyConsumptionKwhPerHour = 1.5, CarbonIntensityKgCO2PerKwh = 0.35, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.PartSubstitution, CostPerHour = 60.0m, SpeedMultiplier = 1.0, QualityScore = 0.85, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.35, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Reassembly, CostPerHour = 80.0m, SpeedMultiplier = 0.90, QualityScore = 0.92, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.35, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Certification, CostPerHour = 100.0m, SpeedMultiplier = 1.0, QualityScore = 0.98, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.35, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 500.0, Power = 1500.0, Tolerance = 0.01 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 7,
                    WorkDayEndHour = 18,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 9, StartMinute = 51, DurationMinutes = 17, Name = "Lunch Break" },
                        new ProviderBreakPeriodModel { StartHour = 13, StartMinute = 15, DurationMinutes = 43, Name = "Break 2" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("a1b2c3d4-1111-2222-3333-444444444444"),
                Type = "MainRemanufacturingCenter",
                Name = "Secondary Remanufacturing Center",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Cleaning, CostPerHour = 55.0m, SpeedMultiplier = 0.95, QualityScore = 0.88, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.32, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Disassembly, CostPerHour = 70.0m, SpeedMultiplier = 0.90, QualityScore = 0.90, EnergyConsumptionKwhPerHour = 1.5, CarbonIntensityKgCO2PerKwh = 0.32, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.PartSubstitution, CostPerHour = 65.0m, SpeedMultiplier = 0.95, QualityScore = 0.87, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.32, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Reassembly, CostPerHour = 75.0m, SpeedMultiplier = 0.92, QualityScore = 0.89, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.32, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Certification, CostPerHour = 95.0m, SpeedMultiplier = 1.05, QualityScore = 0.95, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.32, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 400.0, Power = 1200.0, Tolerance = 0.015 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 6,
                    WorkDayEndHour = 16,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 12, StartMinute = 0, DurationMinutes = 45, Name = "Lunch Break" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("b2c3d4e5-2222-3333-4444-555555555555"),
                Type = "MainRemanufacturingCenter",
                Name = "Budget Remanufacturing Shop",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Cleaning, CostPerHour = 45.0m, SpeedMultiplier = 0.85, QualityScore = 0.80, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.40, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Disassembly, CostPerHour = 60.0m, SpeedMultiplier = 0.80, QualityScore = 0.82, EnergyConsumptionKwhPerHour = 1.5, CarbonIntensityKgCO2PerKwh = 0.40, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.PartSubstitution, CostPerHour = 50.0m, SpeedMultiplier = 0.85, QualityScore = 0.78, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.40, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Reassembly, CostPerHour = 65.0m, SpeedMultiplier = 0.85, QualityScore = 0.83, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.40, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Certification, CostPerHour = 85.0m, SpeedMultiplier = 0.90, QualityScore = 0.88, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.40, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 350.0, Power = 1000.0, Tolerance = 0.02 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 8,
                    WorkDayEndHour = 17,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 10, StartMinute = 30, DurationMinutes = 15, Name = "Coffee Break" },
                        new ProviderBreakPeriodModel { StartHour = 12, StartMinute = 30, DurationMinutes = 30, Name = "Lunch Break" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("c3d4e5f6-3333-4444-5555-666666666666"),
                Type = "MainRemanufacturingCenter",
                Name = "Premium Remanufacturing Facility",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Cleaning, CostPerHour = 70.0m, SpeedMultiplier = 1.1, QualityScore = 0.95, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.30, UsesRenewableEnergy = true },
                    new ProcessCapabilityModel { Process = ProcessType.Disassembly, CostPerHour = 90.0m, SpeedMultiplier = 1.05, QualityScore = 0.97, EnergyConsumptionKwhPerHour = 1.5, CarbonIntensityKgCO2PerKwh = 0.30, UsesRenewableEnergy = true },
                    new ProcessCapabilityModel { Process = ProcessType.PartSubstitution, CostPerHour = 80.0m, SpeedMultiplier = 1.1, QualityScore = 0.93, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.30, UsesRenewableEnergy = true },
                    new ProcessCapabilityModel { Process = ProcessType.Reassembly, CostPerHour = 95.0m, SpeedMultiplier = 1.0, QualityScore = 0.96, EnergyConsumptionKwhPerHour = 2.0, CarbonIntensityKgCO2PerKwh = 0.30, UsesRenewableEnergy = true },
                    new ProcessCapabilityModel { Process = ProcessType.Certification, CostPerHour = 120.0m, SpeedMultiplier = 1.15, QualityScore = 0.99, EnergyConsumptionKwhPerHour = 1.0, CarbonIntensityKgCO2PerKwh = 0.30, UsesRenewableEnergy = true }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 600.0, Power = 2000.0, Tolerance = 0.005 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 0,
                    WorkDayEndHour = 24,
                    Is24x7 = true,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday },
                    Breaks = new List<ProviderBreakPeriodModel>()
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("d4e5f6a7-4444-5555-6666-777777777777"),
                Type = "EngineeringDesignFirm",
                Name = "Engineering Design Firm Alpha",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Redesign, CostPerHour = 150.0m, SpeedMultiplier = 1.0, QualityScore = 0.95, EnergyConsumptionKwhPerHour = 0.5, CarbonIntensityKgCO2PerKwh = 0.25, UsesRenewableEnergy = true }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 0.0, Power = 0.0, Tolerance = 0.001 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 8,
                    WorkDayEndHour = 17,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 11, StartMinute = 42, DurationMinutes = 28, Name = "Lunch Break" },
                        new ProviderBreakPeriodModel { StartHour = 14, StartMinute = 2, DurationMinutes = 31, Name = "Break 2" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("e5f6a7b8-5555-6666-7777-888888888888"),
                Type = "EngineeringDesignFirm",
                Name = "Engineering Design Firm Beta",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Redesign, CostPerHour = 130.0m, SpeedMultiplier = 0.95, QualityScore = 0.92, EnergyConsumptionKwhPerHour = 0.5, CarbonIntensityKgCO2PerKwh = 0.28, UsesRenewableEnergy = true }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 0.0, Power = 0.0, Tolerance = 0.0015 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 9,
                    WorkDayEndHour = 18,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 12, StartMinute = 15, DurationMinutes = 60, Name = "Lunch Break" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("f6a7b8c9-6666-7777-8888-999999999999"),
                Type = "PrecisionMachineShop",
                Name = "Precision Machine Shop A",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Turning, CostPerHour = 120.0m, SpeedMultiplier = 1.05, QualityScore = 0.92, EnergyConsumptionKwhPerHour = 15.0, CarbonIntensityKgCO2PerKwh = 0.55, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Grinding, CostPerHour = 130.0m, SpeedMultiplier = 1.0, QualityScore = 0.93, EnergyConsumptionKwhPerHour = 20.0, CarbonIntensityKgCO2PerKwh = 0.55, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 300.0, Power = 2000.0, Tolerance = 0.001 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 6,
                    WorkDayEndHour = 17,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 8, StartMinute = 23, DurationMinutes = 43, Name = "Lunch Break" },
                        new ProviderBreakPeriodModel { StartHour = 13, StartMinute = 26, DurationMinutes = 56, Name = "Break 2" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("a7b8c9d0-7777-8888-9999-000000000000"),
                Type = "PrecisionMachineShop",
                Name = "Precision Machine Shop B",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Turning, CostPerHour = 110.0m, SpeedMultiplier = 1.0, QualityScore = 0.88, EnergyConsumptionKwhPerHour = 15.0, CarbonIntensityKgCO2PerKwh = 0.60, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Grinding, CostPerHour = 115.0m, SpeedMultiplier = 0.95, QualityScore = 0.89, EnergyConsumptionKwhPerHour = 20.0, CarbonIntensityKgCO2PerKwh = 0.60, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 280.0, Power = 1800.0, Tolerance = 0.0015 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 7,
                    WorkDayEndHour = 16,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 12, StartMinute = 0, DurationMinutes = 30, Name = "Lunch Break" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("b8c9d0e1-8888-9999-0000-111111111111"),
                Type = "PrecisionMachineShop",
                Name = "Premium Machine Shop",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Turning, CostPerHour = 145.0m, SpeedMultiplier = 1.15, QualityScore = 0.96, EnergyConsumptionKwhPerHour = 15.0, CarbonIntensityKgCO2PerKwh = 0.50, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Grinding, CostPerHour = 150.0m, SpeedMultiplier = 1.2, QualityScore = 0.97, EnergyConsumptionKwhPerHour = 20.0, CarbonIntensityKgCO2PerKwh = 0.50, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 400.0, Power = 2500.0, Tolerance = 0.0005 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 6,
                    WorkDayEndHour = 18,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 10, StartMinute = 0, DurationMinutes = 15, Name = "Morning Break" },
                        new ProviderBreakPeriodModel { StartHour = 12, StartMinute = 30, DurationMinutes = 45, Name = "Lunch Break" },
                        new ProviderBreakPeriodModel { StartHour = 15, StartMinute = 0, DurationMinutes = 15, Name = "Afternoon Break" }
                    }
                }
            },
            new ProviderModel
            {
                Id = Guid.Parse("c9d0e1f2-9999-0000-1111-222222222222"),
                Type = "PrecisionMachineShop",
                Name = "Budget Machine Shop",
                AutoStart = true,
                EnvironmentSource = "Production",
                ProcessCapabilities = new List<ProcessCapabilityModel>
                {
                    new ProcessCapabilityModel { Process = ProcessType.Turning, CostPerHour = 85.0m, SpeedMultiplier = 0.85, QualityScore = 0.80, EnergyConsumptionKwhPerHour = 15.0, CarbonIntensityKgCO2PerKwh = 0.70, UsesRenewableEnergy = false },
                    new ProcessCapabilityModel { Process = ProcessType.Grinding, CostPerHour = 90.0m, SpeedMultiplier = 0.80, QualityScore = 0.78, EnergyConsumptionKwhPerHour = 20.0, CarbonIntensityKgCO2PerKwh = 0.70, UsesRenewableEnergy = false }
                },
                TechnicalCapabilities = new TechnicalCapabilitiesModel { AxisHeight = 250.0, Power = 1500.0, Tolerance = 0.003 },
                WorkingHours = new ProviderWorkingHoursModel
                {
                    WorkDayStartHour = 8,
                    WorkDayEndHour = 17,
                    Is24x7 = false,
                    WorkingDays = new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday },
                    Breaks = new List<ProviderBreakPeriodModel>
                    {
                        new ProviderBreakPeriodModel { StartHour = 12, StartMinute = 0, DurationMinutes = 30, Name = "Lunch Break" },
                        new ProviderBreakPeriodModel { StartHour = 15, StartMinute = 30, DurationMinutes = 15, Name = "Coffee Break" }
                    }
                }
            },
        };
    }
}