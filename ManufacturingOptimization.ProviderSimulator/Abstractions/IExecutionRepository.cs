using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions; 

public interface IExecutionRepository : IRepository<ExecutionEntity>
{
    Task<List<ExecutionScheduleSegmentEntity>> GetAllExecutionScheduleSegmentsInTimeWindowAsync(Guid providerId, DateTime startTime, DateTime endTime);
    Task<ExecutionEntity?> GetByIdWithDetailsAsync(Guid id);
    Task<List<ExecutionEntity>> GetExecutionsByProviderAndStatusAsync(Guid providerId, StepExecutionStatus status);
}
