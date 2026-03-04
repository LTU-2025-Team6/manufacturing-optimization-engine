using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Data.Abstractions;

public interface IProviderSimulatorDbContext : IDbContext
{
    DbSet<ProposalEntity> Proposals { get; }
    DbSet<EstimateEntity> Estimates { get; }
    DbSet<ExecutionEntity> Executions { get; }
    DbSet<ExecutionScheduleSegmentEntity> ExecutionScheduleSegments { get; }
}