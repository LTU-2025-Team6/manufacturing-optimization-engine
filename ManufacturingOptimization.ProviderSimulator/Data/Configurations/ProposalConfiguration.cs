using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

public class ProposalConfiguration : IEntityTypeConfiguration<ProposalEntity>
{
    public void Configure(EntityTypeBuilder<ProposalEntity> builder)
    {
        builder.ToTable("Proposals");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PlanId).IsRequired();
        builder.Property(x => x.ProviderId).IsRequired();
        builder.Property(x => x.Process).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ArrivedAt).IsRequired();
        
        // Owned type
        builder.OwnsOne(x => x.MotorSpecs, ms =>
        {
            ms.Property(m => m.PowerKW).IsRequired();
            ms.Property(m => m.AxisHeightMM).IsRequired();
            ms.Property(m => m.CurrentEfficiency).IsRequired().HasMaxLength(50);
            ms.Property(m => m.TargetEfficiency).IsRequired().HasMaxLength(50);
            ms.Property(m => m.MalfunctionDescription).HasMaxLength(500);
        });
        
        builder.HasOne(x => x.Estimate)
            .WithOne(x => x.Proposal)
            .HasForeignKey<EstimateEntity>(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Execution)
            .WithOne(x => x.Proposal)
            .HasForeignKey<ExecutionEntity>(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
