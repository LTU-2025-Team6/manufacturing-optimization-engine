using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;
public class TechnicalCapabilitiesConfiguration : IEntityTypeConfiguration<TechnicalCapabilitiesEntity>
{
    public void Configure(EntityTypeBuilder<TechnicalCapabilitiesEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.AxisHeight).IsRequired();
        entity.Property(e => e.Power).IsRequired();
        entity.Property(e => e.Tolerance).IsRequired();
    }
}