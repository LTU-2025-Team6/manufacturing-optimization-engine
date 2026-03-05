using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Data.Abstractions;
using ManufacturingOptimization.Gateway.Data.Configurations;
using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManufacturingOptimization.Gateway.Data;

/// <summary>
/// Database context for Gateway service.
/// Stores optimization plans and strategies.
/// </summary>
public class GatewayDbContext : DbContext, IGatewayDbContext
{
    public DbSet<ProviderEntity> Providers => Set<ProviderEntity>();
    public DbSet<ProcessCapabilityEntity> ProcessCapabilities => Set<ProcessCapabilityEntity>();
    public DbSet<TechnicalCapabilitiesEntity> TechnicalCapabilities => Set<TechnicalCapabilitiesEntity>();
    public DbSet<ProviderWorkingHoursEntity> WorkingHours => Set<ProviderWorkingHoursEntity>();
    public DbSet<ProviderBreakPeriodEntity> BreakPeriods => Set<ProviderBreakPeriodEntity>();
    public DbSet<OptimizationRequestEntity> OptimizationRequests => Set<OptimizationRequestEntity>();
    public DbSet<OptimizationPlanEntity> OptimizationPlans => Set<OptimizationPlanEntity>();
    public DbSet<OptimizationStrategyEntity> OptimizationStrategies => Set<OptimizationStrategyEntity>();
    public DbSet<ProcessStepEntity> ProcessSteps => Set<ProcessStepEntity>();
    public DbSet<ProcessEstimateEntity> ProcessEstimates => Set<ProcessEstimateEntity>();
    public DbSet<OptimizationMetricsEntity> OptimizationMetrics => Set<OptimizationMetricsEntity>();
    public DbSet<WarrantyTermsEntity> WarrantyTerms => Set<WarrantyTermsEntity>();
    public DbSet<ProviderScheduleEntity> ProviderSchedules => Set<ProviderScheduleEntity>();
    public DbSet<ProviderScheduleSegmentEntity> ProviderScheduleSegments => Set<ProviderScheduleSegmentEntity>();
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    public GatewayDbContext(DbContextOptions<GatewayDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessCapabilityConfiguration());
        modelBuilder.ApplyConfiguration(new TechnicalCapabilitiesConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderWorkingHoursConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationRequestConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationPlanConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationStrategyConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessStepConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new ProviderScheduleSegmentConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessEstimateConfiguration());
        modelBuilder.ApplyConfiguration(new OptimizationMetricsConfiguration());
        modelBuilder.ApplyConfiguration(new WarrantyTermsConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                            // SpecifyKind instead of ToUniversalTime: ToUniversalTime treats
                            // Kind=Unspecified as Local and subtracts the host timezone offset,
                            // silently corrupting datetime values on non-UTC servers.
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        )
                    );
                }
            }
        }
    }
}

public class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext>
{
    public GatewayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();

        // Use SQLite with a design-time connection string matching runtime path
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "gateway.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new GatewayDbContext(optionsBuilder.Options);
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "gateway.db");

        services.AddDbContext<GatewayDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IDbContext, GatewayDbContext>();
        services.AddScoped<IGatewayDbContext, GatewayDbContext>();

        return services;
    }
}
