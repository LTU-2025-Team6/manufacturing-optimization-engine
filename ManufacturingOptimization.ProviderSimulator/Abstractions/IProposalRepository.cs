using System;
using System.Collections.Generic;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProposalRepository : IRepository<ProposalEntity>
{
    /// <summary>
    /// Gets proposal by ID with Estimate and Execution included.
    /// Use for: ConfirmProposalAsync, CancelProposalAsync, ProcessConfirmationHandler
    /// </summary>
    Task<ProposalEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
