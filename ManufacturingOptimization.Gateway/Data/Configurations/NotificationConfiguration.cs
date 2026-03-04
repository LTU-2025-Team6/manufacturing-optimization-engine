using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ManufacturingOptimization.Gateway.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
        entity.Property(e => e.Type).IsRequired(); // Enum stored as int
        entity.Property(e => e.IsRead).IsRequired();
        entity.Property(e => e.CreatedAt).IsRequired();
        entity.Property(e => e.ReadAt).IsRequired(false);
        entity.Property(e => e.Source).IsRequired().HasMaxLength(100);

        // Indexes for efficient querying
        entity.HasIndex(e => e.CreatedAt);
        entity.HasIndex(e => e.IsRead);
        entity.HasIndex(e => e.Type);
        entity.HasIndex(e => e.Source);
    }
}
