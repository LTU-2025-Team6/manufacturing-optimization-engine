using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

public class OptimizationRequestRepository : Repository<OptimizationRequestEntity>, IOptimizationRequestRepository
{
    public OptimizationRequestRepository(IProviderDbContext context) : base(context)
    {
    }

    public async Task<OptimizationRequestEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Owned entities are loaded automatically, so no need for Include
        return await _dbSet.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<OptimizationRequestEntity>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
