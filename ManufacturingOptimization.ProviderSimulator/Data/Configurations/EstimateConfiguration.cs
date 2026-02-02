using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.ProviderSimulator.Data.Configurations;

public class EstimateConfiguration : IEntityTypeConfiguration<EstimateEntity>
{
    public void Configure(EntityTypeBuilder<EstimateEntity> builder)
    {
        builder.ToTable("Estimates");
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Cost).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.QualityScore).IsRequired();
        builder.Property(x => x.EmissionsKgCO2).IsRequired();
        builder.Property(x => x.Duration).IsRequired();

        builder.HasOne(x => x.Proposal)
            .WithOne(x => x.Estimate)
            .HasForeignKey<EstimateEntity>(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
