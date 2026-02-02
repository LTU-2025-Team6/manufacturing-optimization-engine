using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class ExecutionRepository : Repository<ExecutionEntity>, IExecutionRepository
{
    public ExecutionRepository(IProviderSimulatorDbContext context) : base(context)
    {
    }

    public async Task<List<ExecutionScheduleSegmentEntity>> GetAllExecutionScheduleSegmentsInTimeWindowAsync(Guid providerId, DateTime startTime, DateTime endTime)
    {
        return await _dbSet
            .Include(p => p.Proposal)
            .Include(p => p.ScheduleSegments)
            .Where(p => p.Proposal.ProviderId == providerId)
            .SelectMany(p => p.ScheduleSegments)
            .Where(p => (p.StartTime >= startTime && p.EndTime <= endTime) || p.EndTime > startTime || p.StartTime < endTime)
            .ToListAsync();
    }
}
