using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Abstractions;

public interface IGatewayDbContext : IDbContext
{
    DbSet<ProviderEntity> Providers { get; }
    DbSet<ProcessCapabilityEntity> ProcessCapabilities { get; }
    DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities { get; }


    DbSet<OptimizationPlanEntity> OptimizationPlans { get; }
    DbSet<OptimizationStrategyEntity> OptimizationStrategies { get; }
    DbSet<ProcessStepEntity> ProcessSteps { get; }
    DbSet<ProcessEstimateEntity> ProcessEstimates { get; }
    DbSet<OptimizationMetricsEntity> OptimizationMetrics { get; }
    DbSet<WarrantyTermsEntity> WarrantyTerms { get; }
    DbSet<ProviderScheduleEntity> ProviderSchedules { get; }
}
