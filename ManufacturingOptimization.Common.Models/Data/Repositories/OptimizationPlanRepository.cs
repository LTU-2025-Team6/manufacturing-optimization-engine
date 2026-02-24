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

    public Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
        => _dbSet.CountAsync(cancellationToken);

    // ToDo: Just a placeholder
    public Task<int> GetRunningCountAsync(CancellationToken cancellationToken = default)
        => _dbSet.CountAsync(p => p.Status == "InProgress", cancellationToken);

    public Task<int> GetCompletedThisMonthCountAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);

        return _dbSet.CountAsync(
            p => p.Status == "Completed" &&
                 p.ConfirmedAt.HasValue &&
                 p.ConfirmedAt.Value >= firstDayOfMonth &&
                 p.ConfirmedAt.Value <= lastDayOfMonth,
            cancellationToken);
    }
}
