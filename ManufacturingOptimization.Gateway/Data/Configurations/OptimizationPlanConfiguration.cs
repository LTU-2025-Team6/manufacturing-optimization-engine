using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;
public class OptimizationPlanConfiguration : IEntityTypeConfiguration<OptimizationPlanEntity>
{
    public void Configure(EntityTypeBuilder<OptimizationPlanEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.RequestId).IsRequired();
        entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
        entity.Property(e => e.CreatedAt).IsRequired();

        entity.HasMany(e => e.Strategies)
            .WithOne(s => s.Plan)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.SelectedStrategy)
            .WithOne()
            .HasForeignKey<OptimizationPlanEntity>(p => p.SelectedStrategyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}