using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

/// <summary>
/// Repository for Provider entities in Gateway.
/// Works directly with ProviderEntity.
/// </summary>
public class ProviderRepository : Repository<ProviderEntity>, IProviderRepository
{
    public ProviderRepository(IProviderDbContext context) : base(context)
    {
    }

    public async Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process, CancellationToken cancellationToken = default)
    {
        var entities = await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Include(p => p.WorkingHours)
                .ThenInclude(wh => wh.Breaks)
            .Where(p => p.ProcessCapabilities.Any(cap => cap.Process == process.ToString()))
            .ToListAsync(cancellationToken);

        var result = new List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>();
        foreach (var entity in entities)
        {
            foreach (var cap in entity.ProcessCapabilities.Where(c => c.Process == process.ToString()))
            {
                result.Add((entity, cap));
            }
        }
        return result;
    }

    public override async Task<ProviderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Include(p => p.WorkingHours)
                .ThenInclude(wh => wh.Breaks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<ProviderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Include(p => p.WorkingHours)
                .ThenInclude(wh => wh.Breaks)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateRunningState(Guid providerId, bool isRunning, CancellationToken cancellationToken = default)
    {
        var provider = await _dbSet.FirstOrDefaultAsync(p => p.Id == providerId, cancellationToken);

        if (provider == null)
            throw new InvalidDataException($"Provider with ID {providerId} not found.");

        provider.IsRunning = isRunning;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAllRunningState(bool isRunning, CancellationToken cancellationToken = default)
    {
        var providers = await _dbSet.ToListAsync(cancellationToken);
        foreach (var provider in providers)
        {
            provider.IsRunning = isRunning;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AreAllRunning(CancellationToken cancellationToken = default)
        => await _dbSet.Where(p => p.AutoStart).AllAsync(p => p.IsRunning, cancellationToken);

    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
        => _dbSet.ExecuteDeleteAsync(cancellationToken);

    public Task<List<ProviderEntity>> GetRunningProvidersAsync()
        => _dbSet.Where(p => p.IsRunning).ToListAsync();

    public Task<List<ProviderEntity>> GetProvidersWithCapabilityAsync(ProcessType process, Guid? excludedProviderId)
        => _dbSet
            .Where(p => p.ProcessCapabilities.Any(cap => cap.Process == process.ToString()) && p.Id != excludedProviderId)
            .ToListAsync();
}
