using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Configurations;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data;

/// <summary>
/// Database context for Gateway service.
/// Stores optimization plans and strategies.
/// </summary>
public class GatewayDbContext : DbContext, IOptimizationDbContext, IProviderDbContext
{
    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities => Set<ProcessCapabilityEntity>();
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities => Set<TechnicalCapabilitiesEntity>();
    public DbSet<ProviderWorkingHoursEntity> WorkingHours => Set<ProviderWorkingHoursEntity>();
    public DbSet<ProviderBreakPeriodEntity> BreakPeriods => Set<ProviderBreakPeriodEntity>();
    public DbSet<OptimizationRequestEntity> OptimizationRequests => Set<OptimizationRequestEntity>();
    public DbSet<OptimizationPlanEntity> OptimizationPlans => Set<OptimizationPlanEntity>();
    public DbSet<OptimizationStrategyEntity> OptimizationStrategies => Set<OptimizationStrategyEntity>();
    public DbSet<ProcessStepEntity> ProcessSteps => Set<ProcessStepEntity>();
    public DbSet<ProcessEstimateEntity> ProcessEstimates => Set<ProcessEstimateEntity>();
    public DbSet<OptimizationMetricsEntity> OptimizationMetrics => Set<OptimizationMetricsEntity>();
    public DbSet<WarrantyTermsEntity> WarrantyTerms => Set<WarrantyTermsEntity>();
    public DbSet<ProviderScheduleEntity> ProviderSchedules => Set<ProviderScheduleEntity>();
    public DbSet<ProviderScheduleSegmentEntity> ProviderScheduleSegments => Set<ProviderScheduleSegmentEntity>();

    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessCapabilityConfiguration());
        modelBuilder.ApplyConfiguration(new TechnicalCapabilitiesConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderWorkingHoursConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationRequestConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationPlanConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessStepConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderScheduleSegmentConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessEstimateConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationMetricsConfiguration());
        modelBuilder.ApplyConfiguration(new WarrantyTermsConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        )
                    );
                }
            }
        }
    }
}
