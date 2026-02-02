using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

/// <summary>
/// Repository for OptimizationPlan entities in Gateway.
/// Works directly with OptimizationPlanEntity.
/// </summary>
public class OptimizationPlanRepository : Repository<OptimizationPlanEntity>, IOptimizationPlanRepository
{
    public OptimizationPlanRepository(IOptimizationDbContext context) : base(context)
    {
    }

    public async Task<OptimizationPlanEntity?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Warranty)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Warranty)
            .FirstOrDefaultAsync(p => p.RequestId == requestId, cancellationToken);
    }

    public override async Task<OptimizationPlanEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Warranty)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Warranty)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<OptimizationPlanEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Warranty)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Warranty)
            .ToListAsync(cancellationToken);
    }
}
