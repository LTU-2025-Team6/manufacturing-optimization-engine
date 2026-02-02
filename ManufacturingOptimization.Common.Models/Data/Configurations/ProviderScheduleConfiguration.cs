using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;

public class ProviderScheduleConfiguration : IEntityTypeConfiguration<ProviderScheduleEntity>
{
    public void Configure(EntityTypeBuilder<ProviderScheduleEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.StartTime).IsRequired();
        entity.Property(e => e.EndTime).IsRequired();

        entity.HasMany(e => e.Segments)
            .WithOne(s => s.ProviderSchedule)
            .HasForeignKey(s => s.ProviderScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
