using ManufacturingOptimization.Common.Data.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Configurations;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManufacturingOptimization.ProviderRegistry.Data;

/// <summary>
/// Database context for provider registry.
/// </summary>
public class ProviderSimulatorDbContext : DbContext, IProviderSimulatorDbContext
{
    public DbSet<ProposalEntity> Proposals => Set<ProposalEntity>();
    public DbSet<EstimateEntity> Estimates => Set<EstimateEntity>();
    public DbSet<ExecutionEntity> Executions => Set<ExecutionEntity>();
    public DbSet<ExecutionScheduleSegmentEntity> ExecutionScheduleSegments => Set<ExecutionScheduleSegmentEntity>();
    public DbSet<DemoDataStatusEntity> DemoDataStatus => Set<DemoDataStatusEntity>();


    public ProviderSimulatorDbContext(DbContextOptions<ProviderSimulatorDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProposalConfiguration());
        modelBuilder.ApplyConfiguration(new EstimateConfiguration());
        modelBuilder.ApplyConfiguration(new ExecutionConfiguration());
        modelBuilder.ApplyConfiguration(new ExecutionScheduleSegmentConfiguration());
        modelBuilder.ApplyConfiguration(new DemoDataStatusConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            // Never call ToUniversalTime() — it treats Kind=Unspecified as Local
                            // and would silently subtract the host timezone offset.
                            // SpecifyKind on write guarantees the raw numeric value is preserved as-is.
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        )
                    );
                }
            }
        }
    }
}

public class ProviderSimulatorDbContextFactory : IDesignTimeDbContextFactory<ProviderSimulatorDbContext>
{
    public ProviderSimulatorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProviderSimulatorDbContext>();
        
        // Use SQLite with a design-time connection string matching runtime path
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "provider-simulator.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new ProviderSimulatorDbContext(optionsBuilder.Options);
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "provider-simulator.db");

        services.AddDbContext<ProviderSimulatorDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IProviderSimulatorDbContext, ProviderSimulatorDbContext>();

        return services;
    }
}