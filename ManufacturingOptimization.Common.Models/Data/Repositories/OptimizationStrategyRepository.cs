using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

/// <summary>
/// Repository for temporary OptimizationStrategy entities in Gateway.
/// Works directly with OptimizationStrategyEntity.
/// </summary>
public class OptimizationStrategyRepository : Repository<OptimizationStrategyEntity>, IOptimizationStrategyRepository
{
    public OptimizationStrategyRepository(IOptimizationDbContext context) : base(context)
    {
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
