using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

/// <summary>
/// Repository for managing process estimate entities.
/// </summary>
public class ProcessEstimateRepository : Repository<ProcessEstimateEntity>, IProcessEstimateRepository
{
    public ProcessEstimateRepository(IOptimizationDbContext context) : base(context)
    {
    }
}
