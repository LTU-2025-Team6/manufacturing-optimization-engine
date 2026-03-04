using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProposalService
{
    Task<ProposalModel> CreateProposalAsync(Guid planId, ProcessType process, MotorSpecificationsModel motorSpecs, DateTime? arrivedAt = null);
    Task ConfirmProposalAsync(Guid proposalId, ProviderScheduleModel schedule, bool isDemo = false);
    Task CancelProposalAsync(Guid proposalId);
}
