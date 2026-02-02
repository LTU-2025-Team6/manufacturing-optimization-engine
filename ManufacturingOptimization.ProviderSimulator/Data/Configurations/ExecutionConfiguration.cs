using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

public class ExecutionConfiguration : IEntityTypeConfiguration<ExecutionEntity>
{
    public void Configure(EntityTypeBuilder<ExecutionEntity> entity)
    {
        entity.ToTable("Executions");

        entity.HasKey(e => e.Id);
        entity.Property(e => e.ProposalId).IsRequired();

        entity.HasOne(e => e.Proposal)
            .WithOne(p => p.Execution)
            .HasForeignKey<ExecutionEntity>(e => e.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.ScheduleSegments)
            .WithOne(s => s.Execution)
            .HasForeignKey(e => e.ExecutionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}