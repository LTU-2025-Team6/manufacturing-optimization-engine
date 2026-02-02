using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

public class ExecutionScheduleSegmentConfiguration : IEntityTypeConfiguration<ExecutionScheduleSegmentEntity>
{
    public void Configure(EntityTypeBuilder<ExecutionScheduleSegmentEntity> builder)
    {
        builder.ToTable("ExecutionScheduleSegments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.StartTime).IsRequired();
        builder.Property(e => e.EndTime).IsRequired();

        builder.HasOne(e => e.Execution)
            .WithMany(p => p.ScheduleSegments)
            .HasForeignKey(e => e.ExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}