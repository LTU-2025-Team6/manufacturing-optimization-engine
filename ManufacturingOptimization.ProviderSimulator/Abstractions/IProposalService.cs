using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProposalService
{
    Task<ProposalModel> CreateProposalAsync(Guid planId, ProcessType process, MotorSpecificationsModel motorSpecs, DateTime? arrivedAt = null);
    Task ConfirmProposalAsync(Guid proposalId, ProviderScheduleModel schedule);
    Task CancelProposalAsync(Guid proposalId);
}
