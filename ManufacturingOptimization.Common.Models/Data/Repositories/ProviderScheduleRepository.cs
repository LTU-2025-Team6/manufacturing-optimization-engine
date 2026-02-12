using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

public class ProviderScheduleRepository : Repository<ProviderScheduleEntity>, IProviderScheduleRepository
{
    public ProviderScheduleRepository(IProviderDbContext context) : base(context)
    {
    }
}
