using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;
public class OptimizationMetricsConfiguration : IEntityTypeConfiguration<OptimizationMetricsEntity>
{
    public void Configure(EntityTypeBuilder<OptimizationMetricsEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.TotalCost).IsRequired().HasPrecision(18, 2);
        entity.Property(e => e.TotalTime).IsRequired();
        entity.Property(e => e.AverageQuality).IsRequired();
        entity.Property(e => e.TotalEmissionsKgCO2).IsRequired();
        entity.Property(e => e.SolverStatus).HasMaxLength(50);
        entity.Property(e => e.ObjectiveValue).IsRequired();
    }
}