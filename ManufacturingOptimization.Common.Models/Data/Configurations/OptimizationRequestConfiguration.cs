using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;

public class OptimizationRequestConfiguration : IEntityTypeConfiguration<OptimizationRequestEntity>
{
    public void Configure(EntityTypeBuilder<OptimizationRequestEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(200);
        entity.Property(e => e.CreatedAt).IsRequired();

        // Configure owned entity: MotorSpecs
        entity.OwnsOne(e => e.MotorSpecs, motorSpecs =>
        {
            motorSpecs.Property(m => m.PowerKW).IsRequired();
            motorSpecs.Property(m => m.AxisHeightMM).IsRequired();
            motorSpecs.Property(m => m.CurrentEfficiency).IsRequired().HasMaxLength(50);
            motorSpecs.Property(m => m.TargetEfficiency).IsRequired().HasMaxLength(50);
            motorSpecs.Property(m => m.MalfunctionDescription).HasMaxLength(1000);
        });

        // Configure owned entity: Constraints
        entity.OwnsOne(e => e.Constraints, constraints =>
        {
            constraints.Property(c => c.MaxBudget).HasPrecision(18, 2);

            // Configure nested owned entity: TimeWindow
            constraints.OwnsOne(c => c.TimeWindow, timeWindow =>
            {
                timeWindow.Property(t => t.StartTime).IsRequired();
                timeWindow.Property(t => t.EndTime).IsRequired();
            });
        });
    }
}
