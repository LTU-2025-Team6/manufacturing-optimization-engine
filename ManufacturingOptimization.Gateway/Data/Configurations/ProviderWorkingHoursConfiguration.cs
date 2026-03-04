using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;

public class ProviderWorkingHoursConfiguration : IEntityTypeConfiguration<ProviderWorkingHoursEntity>
{
    public void Configure(EntityTypeBuilder<ProviderWorkingHoursEntity> entity)
    {
        entity.HasKey(e => e.ProviderId);
        
        entity.Property(e => e.WorkDayStartHour).IsRequired();
        entity.Property(e => e.WorkDayEndHour).IsRequired();
        entity.Property(e => e.Is24x7).IsRequired();
        entity.Property(e => e.WorkingDaysJson).IsRequired().HasMaxLength(200);

        entity.HasOne(e => e.Provider)
            .WithOne(p => p.WorkingHours)
            .HasForeignKey<ProviderWorkingHoursEntity>(e => e.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.Breaks)
            .WithOne(b => b.WorkingHours)
            .HasForeignKey(b => b.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
