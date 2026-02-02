using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Configurations;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderRegistry.Data;

/// <summary>
/// Database context for provider registry.
/// </summary>
public class ProviderRegistryDbContext : DbContext, IProviderDbContext
{

    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities => Set<ProcessCapabilityEntity>();
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities => Set<TechnicalCapabilitiesEntity>();
    public DbSet<ProviderWorkingHoursEntity> WorkingHours => Set<ProviderWorkingHoursEntity>();
    public DbSet<ProviderBreakPeriodEntity> BreakPeriods => Set<ProviderBreakPeriodEntity>();
    
    public ProviderRegistryDbContext(DbContextOptions<ProviderRegistryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessCapabilityConfiguration());
        modelBuilder.ApplyConfiguration(new TechnicalCapabilitiesConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderWorkingHoursConfiguration());
    }
}
