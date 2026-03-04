using ManufacturingOptimization.Common.Data.Abstractions;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class ProposalRepository : Repository<ProposalEntity>, IProposalRepository
{
    public ProposalRepository(IProviderSimulatorDbContext context) : base(context)
    {
    }

    public async Task<ProposalEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Estimate)
            .Include(p => p.Execution)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
