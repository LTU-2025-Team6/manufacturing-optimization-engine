using System;
using System.Collections.Generic;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProposalRepository : IRepository<ProposalEntity>
{
}
