using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for OptimizationPlan entities in Gateway.
/// Works directly with OptimizationPlanEntity.
/// </summary>
public class OptimizationPlanRepository : Repository<OptimizationPlanEntity>, IOptimizationPlanRepository
{
    public OptimizationPlanRepository(IGatewayDbContext context) : base(context)
    {
    }

    public async Task<OptimizationPlanEntity?> GetWithSelectedStrategyStepsForExecutionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<OptimizationPlanEntity?> GetWithAllStrategiesForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Strategies)
                .ThenInclude(s => s.Steps)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<OptimizationPlanEntity?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default)
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
                .ThenInclude(s => s!.Metrics)
            .Include(p => p.Strategies)
                .ThenInclude(s => s!.Warranty)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Warranty)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<OptimizationPlanEntity?> GetWithSelectedStrategyDetailsForExecutionStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
                    .ThenInclude(st => st.ProviderSchedule)
                        .ThenInclude(slot => slot!.Segments)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
                    .ThenInclude(st => st.Estimate)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<OptimizationPlanEntity>> GetAllWithSelectedStrategyStepsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<OptimizationPlanEntity> items, int totalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(List<OptimizationPlanEntity> items, int totalCount)> GetPagedWithSelectedStrategyStepsAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var baseQuery = _dbSet.Where(p => p.SelectedStrategyId != null);
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(List<OptimizationPlanEntity> items, int totalCount)> GetPagedInProgressAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var baseQuery = _dbSet
            .Where(p => p.SelectedStrategyId != null)
            .Where(p => p.SelectedStrategy!.Steps.Any(s => s.ExecutionStatus == StepExecutionStatus.InProgress));
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        var items = await baseQuery
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s!.Steps)
            .OrderBy(p => p.ConfirmedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
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
                 p.CompletedAt.HasValue &&
                 p.CompletedAt.Value >= firstDayOfMonth &&
                 p.CompletedAt.Value <= lastDayOfMonth,
            cancellationToken);
    }
}
