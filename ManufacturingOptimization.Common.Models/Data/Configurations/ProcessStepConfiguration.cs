using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Common.Models.Data.Configurations;
public class ProcessStepConfiguration : IEntityTypeConfiguration<ProcessStepEntity>
{
    public void Configure(EntityTypeBuilder<ProcessStepEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.StepNumber).IsRequired();
        entity.Property(e => e.Process).IsRequired().HasMaxLength(50);
        entity.Property(e => e.ProposalId).IsRequired();
        entity.Property(e => e.SelectedProviderId).IsRequired();
        entity.Property(e => e.SelectedProviderName).IsRequired().HasMaxLength(200);
        entity.Property(e => e.ProviderScheduleId).IsRequired(false);

        entity.HasOne(e => e.Estimate)
            .WithOne(est => est.ProcessStep)
            .HasForeignKey<ProcessEstimateEntity>(est => est.ProcessStepId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.ProviderSchedule)
            .WithOne()
            .HasForeignKey<ProcessStepEntity>(e => e.ProviderScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}