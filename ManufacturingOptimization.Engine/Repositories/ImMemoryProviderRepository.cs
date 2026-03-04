using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace ManufacturingOptimization.Engine.Repositories;
    
internal sealed class InMemoryProviderRepository : IProviderRepository
{
    private readonly ConcurrentDictionary<Guid, ProviderModel> _providers = new();

    public int Count => _providers.Count;

    public Task<ProviderModel> AddAsync(ProviderModel entity, CancellationToken cancellationToken = default)
    {
        _providers[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<IEnumerable<ProviderModel>> AddRangeAsync(IEnumerable<ProviderModel> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            _providers[entity.Id] = entity;
        }

        return Task.FromResult(entities);
    }

    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        _providers.Clear();
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProviderModel entity, CancellationToken cancellationToken = default)
    {
        _providers.TryRemove(entity.Id, out _);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ProviderModel>> FindAsync(Expression<Func<ProviderModel, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var compiled = predicate.Compile();

        var result = _providers.Values
            .Where(compiled)
            .ToList();

        return Task.FromResult<IEnumerable<ProviderModel>>(result);
    }

    public Task<List<(ProviderModel Provider, ProcessCapabilityModel Capability)>> FindByProcess(ProcessType process, CancellationToken cancellationToken = default)
    {
        var result = _providers.Values
            .SelectMany(
                provider => provider.ProcessCapabilities
                    .Where(c => c.Process == process)
                    .Select(capability => (provider, capability)))
            .ToList();

        return Task.FromResult(result);
    }

    public Task<int> GetActiveProvidersCountAsync(CancellationToken cancellationToken = default)
    {
        var count = _providers.Values.Count(p => p.IsRunning);
        return Task.FromResult(count);
    }

    public Task<IEnumerable<ProviderModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<ProviderModel>>(_providers.Values.ToList());
    }

    public Task<ProviderModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _providers.TryGetValue(id, out var provider);
        return Task.FromResult(provider);
    }

    public Task<List<ProviderModel>> GetProvidersWithCapabilityAsync(ProcessType process, Guid? excludedProviderId = null)
    {
        var result = _providers.Values
            .Where(p =>
                (excludedProviderId == null || p.Id != excludedProviderId) &&
                p.ProcessCapabilities.Any(c => c.Process == process))
            .ToList();

        return Task.FromResult(result);
    }

    public Task<List<ProviderModel>> GetRunningProvidersAsync()
    {
        var result = _providers.Values
            .Where(p => p.IsRunning)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task UpdateAsync(ProviderModel entity, CancellationToken cancellationToken = default)
    {
        _providers[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateRunningStateAsync(Guid providerId, bool isRunning, CancellationToken cancellationToken = default)
    {
        if (_providers.TryGetValue(providerId, out var provider))
        {
            provider.IsRunning = isRunning;
        }

        return Task.CompletedTask;
    }
}