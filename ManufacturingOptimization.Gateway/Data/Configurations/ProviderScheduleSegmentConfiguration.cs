using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;

public class ProviderScheduleSegmentConfiguration : IEntityTypeConfiguration<ProviderScheduleSegmentEntity>
{
    public void Configure(EntityTypeBuilder<ProviderScheduleSegmentEntity> builder)
    {
        builder.HasKey(e => e.Id); 
        builder.Property(e => e.StartTime).IsRequired();  
        builder.Property(e => e.EndTime).IsRequired(); 
        builder.Property(e => e.SegmentType).IsRequired().HasMaxLength(20);

        builder.HasOne(e => e.ProviderSchedule)
            .WithMany(p => p.Segments)
            .HasForeignKey(e => e.ProviderScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
