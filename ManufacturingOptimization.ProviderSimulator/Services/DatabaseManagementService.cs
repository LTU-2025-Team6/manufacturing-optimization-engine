using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Data;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// Background service for managing database lifecycle.
/// Clears providers on startup and shutdown since they re-register on every start.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseManagementService> _logger;
    private readonly DemoDataSettings _demoSettings;
    private readonly IProviderSimulationContext _providerContext;
    private readonly INotificationPublisher _notificationPublisher;

    public DatabaseManagementService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseManagementService> logger,
        IOptions<DemoDataSettings> demoSettings,
        IProviderSimulationContext providerContext,
        INotificationPublisher notificationPublisher)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _demoSettings = demoSettings.Value;
        _providerContext = providerContext;
        _notificationPublisher = notificationPublisher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run all database initialization in background to not block provider startup
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ProviderSimulatorDbContext>();

                // Apply migrations (creates database if it doesn't exist)
                await dbContext.Database.MigrateAsync(CancellationToken.None);

                // Generate demo data if enabled
                if (_demoSettings.Enabled)
                {
                    var shouldGenerate = await TryClaimDemoDataGenerationAsync(dbContext, CancellationToken.None);
                    
                    if (shouldGenerate)
                    {
                        _notificationPublisher.NotifyDemoDataGenerationStarted(
                            _providerContext.Provider.Name,
                            _providerContext.Provider.Id);
                        
                        var demoGenerator = scope.ServiceProvider.GetRequiredService<DemoDataGeneratorService>();
                        await demoGenerator.GenerateAsync();
                        
                        _notificationPublisher.NotifyDemoDataGenerationCompleted(
                            _providerContext.Provider.Name,
                            _providerContext.Provider.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization or demo data generation for provider {ProviderId}", 
                    _providerContext.Provider.Id);
                
                _notificationPublisher.NotifyDemoDataGenerationFailed(
                    _providerContext.Provider.Name,
                    _providerContext.Provider.Id,
                    ex.Message);
            }
        });

        return Task.CompletedTask;
    }

    private async Task<bool> TryClaimDemoDataGenerationAsync(ProviderSimulatorDbContext dbContext, CancellationToken cancellationToken)
    {
        var providerId = _providerContext.Provider.Id.ToString();
        
        try
        {
            // Check if this provider has already generated its data
            var status = await dbContext.DemoDataStatus
                .FirstOrDefaultAsync(s => s.ProviderId == providerId, cancellationToken);
            
            if (status == null)
            {
                // Create new status record for this provider
                status = new DemoDataStatusEntity
                {
                    ProviderId = providerId,
                    IsGenerated = true,
                    GeneratedAt = DateTime.UtcNow
                };
                
                dbContext.DemoDataStatus.Add(status);
                await dbContext.SaveChangesAsync(cancellationToken);
                
                return true; // This provider should generate its data
            }
            else if (!status.IsGenerated)
            {
                // Status exists but not marked as generated - claim it
                status.IsGenerated = true;
                status.GeneratedAt = DateTime.UtcNow;
                
                await dbContext.SaveChangesAsync(cancellationToken);

                return true; // This provider should generate its data
            }
            else
            {
                return false; // Skip generation
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error claiming demo data generation for provider {ProviderId}", providerId);
            return false; // Skip generation on error to allow provider to start
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
