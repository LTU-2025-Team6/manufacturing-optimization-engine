using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderSimulator.Data.Repositories;

public class ProposalRepository : Repository<ProposalEntity>, IProposalRepository
{
    public ProposalRepository(IProviderSimulatorDbContext context) : base(context)
    {
    }

    public override async Task<ProposalEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Estimate)
            .Include(p => p.Execution)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
