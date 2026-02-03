using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;
public class ProviderConfiguration : IEntityTypeConfiguration<ProviderEntity>
{
    public void Configure(EntityTypeBuilder<ProviderEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Type).IsRequired().HasMaxLength(100);
        entity.Property(e => e.AutoStart).IsRequired();
        entity.Property(e => e.IsRunning).IsRequired();
        entity.Property(e => e.EnvironmentSource).IsRequired();

        entity.HasMany(e => e.ProcessCapabilities)
            .WithOne(pc => pc.Provider)
            .HasForeignKey(pc => pc.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.TechnicalCapabilities)
            .WithOne(tc => tc.Provider)
            .HasForeignKey<TechnicalCapabilitiesEntity>(tc => tc.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.WorkingHours)
            .WithOne(wh => wh.Provider)
            .HasForeignKey<ProviderWorkingHoursEntity>(wh => wh.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}