using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for managing process estimate entities.
/// </summary>
public class ProcessEstimateRepository : Repository<ProcessEstimateEntity>, IProcessEstimateRepository
{
    public ProcessEstimateRepository(IGatewayDbContext context) : base(context)
    {
    }
}