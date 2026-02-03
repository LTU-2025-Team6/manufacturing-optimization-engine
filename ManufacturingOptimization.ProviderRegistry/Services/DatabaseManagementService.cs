using AutoMapper;
using ManufacturingOptimization.ProviderRegistry.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Background service that initializes the database with provider data on startup.
/// Reads from providers.json and seeds the database if empty.
/// </summary>
public class DatabaseManagementService : IHostedService
{
    // Toggle to recreate database on every startup (useful during development)
    private const bool RECREATE_DATABASE_ON_STARTUP = false;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseManagementService> _logger;
    private readonly IMapper _mapper;

    public DatabaseManagementService(
        IServiceProvider serviceProvider,
        ILogger<DatabaseManagementService> logger,
        IMapper mapper)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ProviderRegistryDbContext>();

            if (RECREATE_DATABASE_ON_STARTUP)
            {
                // Clear for development purposes
                await dbContext.Database.EnsureDeletedAsync(cancellationToken);
            }

            // Apply migrations
            await dbContext.Database.MigrateAsync(cancellationToken);

            // Check if database already has data
            var existingProvidersCount = await dbContext.Providers.CountAsync(cancellationToken);
            if (existingProvidersCount > 0)
            {
                return;
            }

            // Map providers to entities and add to database
            var staticProviders = GetStaticProviders();
            var providerEntities = _mapper.Map<List<ProviderEntity>>(staticProviders);
            await dbContext.Providers.AddRangeAsync(providerEntities, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public static List<ProviderModel> GetStaticProviders()
    {
        return new List<ProviderModel>();
    }
}

