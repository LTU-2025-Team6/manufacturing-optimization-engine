using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

public class ProviderScheduleRepository : Repository<ProviderScheduleEntity>, IProviderScheduleRepository
{
    public ProviderScheduleRepository(IGatewayDbContext context) : base(context)
    {
    }
}
