using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for Provider entities in Gateway.
/// Works directly with ProviderEntity.
/// </summary>
public class ProviderRepository : Repository<ProviderEntity>, IProviderRepository
{
    public ProviderRepository(IGatewayDbContext context) : base(context)
    {
    }

    public async Task<ProviderEntity?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Include(p => p.WorkingHours)
                .ThenInclude(wh => wh.Breaks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<List<ProviderEntity>> GetAllWithFullDetailsAsync(CancellationToken cancellationToken = default)
        => _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Include(p => p.WorkingHours)
                .ThenInclude(wh => wh.Breaks)
            .ToListAsync(cancellationToken);

    public async Task<(List<ProviderEntity> items, int totalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .OrderBy(p => p.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task UpdateRunningStateAsync(Guid providerId, bool isRunning, CancellationToken cancellationToken = default)
    {
        var provider = await _dbSet.FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken)
            ?? throw new InvalidDataException($"Provider with ID {providerId} not found.");
        
        provider.IsRunning = isRunning;
        await _context.SaveChangesAsync();
    }

    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        => _dbSet.ExecuteDeleteAsync(cancellationToken);

    public Task<List<ProviderEntity>> GetRunningProvidersAsync()
        => _dbSet.Where(p => p.IsRunning).ToListAsync();

    public Task<List<ProviderEntity>> GetProvidersWithCapabilityAsync(ProcessType process, Guid? excludedProviderId)
        => _dbSet
            .Where(p => p.ProcessCapabilities.Any(cap => cap.Process == process.ToString()) && p.Id != excludedProviderId)
            .ToListAsync();

    public Task<int> GetActiveProvidersCountAsync(CancellationToken cancellationToken = default)
        => _dbSet.CountAsync(p => p.IsRunning, cancellationToken);
}
