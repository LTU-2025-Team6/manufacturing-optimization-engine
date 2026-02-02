using ManufacturingOptimization.ProviderRegistry.Data;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// Background service for managing database lifecycle.
/// Clears providers on startup and shutdown since they re-register on every start.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    // Toggle to recreate database on every startup (useful during development)
    private const bool RECREATE_DATABASE_ON_STARTUP = true;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseManagementService> _logger;

    public DatabaseManagementService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseManagementService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProviderSimulatorDbContext>();

            if (RECREATE_DATABASE_ON_STARTUP)
            {
                // Clear for development purposes
                //await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            // Apply migrations (creates database if it doesn't exist)
            await dbContext.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize ProviderSimulator database");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
