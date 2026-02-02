using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IProviderSimulatorDbContext : IDbContext
{
    DbSet<ProposalEntity> Proposals { get; }
    DbSet<EstimateEntity> Estimates { get; }
    DbSet<ExecutionEntity> Executions { get; }
    DbSet<ExecutionScheduleSegmentEntity> ExecutionScheduleSegments { get; }
}