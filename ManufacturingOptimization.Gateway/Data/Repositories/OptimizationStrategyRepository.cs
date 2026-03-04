using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for temporary OptimizationStrategy entities in Gateway.
/// Works directly with OptimizationStrategyEntity.
/// </summary>
public class OptimizationStrategyRepository : Repository<OptimizationStrategyEntity>, IOptimizationStrategyRepository
{
    public OptimizationStrategyRepository(IGatewayDbContext context) : base(context)
    {
    }

    public async Task<OptimizationStrategyEntity?> GetByIdWithStepsOnlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Steps)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public override async Task<OptimizationStrategyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Steps)
                .ThenInclude(st => st.ProviderSchedule)
                    .ThenInclude(slot => slot!.Segments)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }
}
