using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;
public class ProcessCapabilityConfiguration : IEntityTypeConfiguration<ProcessCapabilityEntity>
{
    public void Configure(EntityTypeBuilder<ProcessCapabilityEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Process).IsRequired().HasMaxLength(50);
        entity.Property(e => e.CostPerHour).IsRequired().HasPrecision(18, 2);
        entity.Property(e => e.SpeedMultiplier).IsRequired();
        entity.Property(e => e.QualityScore).IsRequired();
        entity.Property(e => e.EnergyConsumptionKwhPerHour).IsRequired();
        entity.Property(e => e.CarbonIntensityKgCO2PerKwh).IsRequired();
        entity.Property(e => e.UsesRenewableEnergy).IsRequired();
    }
}