using ManufacturingOptimization.Common.Data.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class ExecutionRepository : Repository<ExecutionEntity>, IExecutionRepository
{
    private readonly IProviderSimulatorDbContext _providerDbContext;

    public ExecutionRepository(IProviderSimulatorDbContext context) : base(context)
    {
        _providerDbContext = context;
    }

    public async Task<List<ExecutionScheduleSegmentEntity>> GetAllExecutionScheduleSegmentsInTimeWindowAsync(Guid providerId, DateTime startTime, DateTime endTime)
    {
        // Query segments directly from DbSet to ensure ExecutionId is populated
        return await _providerDbContext.ExecutionScheduleSegments
            .Include(s => s.Execution)
                .ThenInclude(e => e.Proposal)
            .Where(s => s.Execution.Proposal.ProviderId == providerId)
            .Where(s => (s.StartTime >= startTime && s.EndTime <= endTime) || s.EndTime > startTime || s.StartTime < endTime)
            .ToListAsync();
    }

    public async Task<ExecutionEntity?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(e => e.Proposal)
                .ThenInclude(p => p.Estimate)
            .Include(e => e.ScheduleSegments)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<ExecutionEntity>> GetExecutionsByProviderAndStatusAsync(Guid providerId, StepExecutionStatus status)
    {
        return await _dbSet
            .Include(e => e.Proposal)
            .Include(e => e.ScheduleSegments)
            .Where(e => e.Proposal.ProviderId == providerId)
            .Where(e => e.Status == status)
            .Where(e => !e.IsDemo) // Exclude demo executions - they are for display only
            .ToListAsync();
    }
}
