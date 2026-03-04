using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;
public class WarrantyTermsConfiguration : IEntityTypeConfiguration<WarrantyTermsEntity>
{
    public void Configure(EntityTypeBuilder<WarrantyTermsEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Level).IsRequired().HasMaxLength(50);
        entity.Property(e => e.DurationMonths).IsRequired();
        entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
        entity.Property(e => e.IncludesInsurance).IsRequired();
    }
}