using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

/// <summary>
/// EF Core configuration for DemoDataStatus entity.
/// Tracks demo data generation status per provider.
/// </summary>
public class DemoDataStatusConfiguration : IEntityTypeConfiguration<DemoDataStatusEntity>
{
    public void Configure(EntityTypeBuilder<DemoDataStatusEntity> builder)
    {
        builder.ToTable("DemoDataStatus");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ProviderId)
            .IsRequired()
            .HasMaxLength(200);
        
        // ProviderId must be unique - one record per provider
        builder.HasIndex(e => e.ProviderId)
            .IsUnique();
        
        builder.Property(e => e.IsGenerated)
            .IsRequired();
        
        builder.Property(e => e.GeneratedAt);
    }
}
