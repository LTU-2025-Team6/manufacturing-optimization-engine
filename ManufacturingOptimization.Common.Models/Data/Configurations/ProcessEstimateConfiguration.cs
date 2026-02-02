using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;
public class ProcessEstimateConfiguration : IEntityTypeConfiguration<ProcessEstimateEntity>
{
    public void Configure(EntityTypeBuilder<ProcessEstimateEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Cost).IsRequired().HasPrecision(18, 2);
        entity.Property(e => e.Duration).IsRequired();
        entity.Property(e => e.QualityScore).IsRequired();
        entity.Property(e => e.EmissionsKgCO2).IsRequired();
    }
}