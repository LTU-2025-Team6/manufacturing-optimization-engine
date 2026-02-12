using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Configurations;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Engine.Data;

/// <summary>
/// Database context for Engine service.
/// </summary>
public class EngineDbContext : DbContext, IProviderDbContext
{
    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities => Set<ProcessCapabilityEntity>();
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities => Set<TechnicalCapabilitiesEntity>();
    public DbSet<ProviderWorkingHoursEntity> WorkingHours => Set<ProviderWorkingHoursEntity>();
    public DbSet<ProviderBreakPeriodEntity> BreakPeriods => Set<ProviderBreakPeriodEntity>();

    public EngineDbContext(DbContextOptions<EngineDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessCapabilityConfiguration());
        modelBuilder.ApplyConfiguration(new TechnicalCapabilitiesConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderWorkingHoursConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        )
                    );
                }
            }
        }
    }
}
