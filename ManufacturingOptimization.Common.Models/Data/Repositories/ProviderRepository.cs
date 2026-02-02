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

    public async Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process)
    {
        var entities = await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Include(p => p.WorkingHours)
                .ThenInclude(wh => wh.Breaks)
            .Where(p => p.ProcessCapabilities.Any(cap => cap.Process == process.ToString()))
            .ToListAsync();

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
}
