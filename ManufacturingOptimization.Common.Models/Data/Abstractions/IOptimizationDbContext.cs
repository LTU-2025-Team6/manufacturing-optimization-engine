using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IOptimizationDbContext : IDbContext
{
    DbSet<OptimizationPlanEntity> OptimizationPlans { get; }
    DbSet<OptimizationStrategyEntity> OptimizationStrategies { get; }
    DbSet<ProcessStepEntity> ProcessSteps { get; }
    DbSet<ProcessEstimateEntity> ProcessEstimates { get; }
    DbSet<OptimizationMetricsEntity> OptimizationMetrics { get; }
    DbSet<WarrantyTermsEntity> WarrantyTerms { get; }
    DbSet<ProviderScheduleEntity> ProviderSchedules { get; }
}
