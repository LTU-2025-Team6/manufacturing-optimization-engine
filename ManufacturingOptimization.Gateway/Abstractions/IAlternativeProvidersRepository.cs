using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IAlternativeProvidersRepository
    {
        Task AddAlternativesForStepAsync(Guid stepId, List<AlternativeProviderModel> alternatives, CancellationToken cancellationToken = default);
        Task<List<AlternativeProviderModel>?> GetAlternativesForStepAsync(Guid stepId, CancellationToken cancellationToken = default);
        Task<AlternativeProviderModel?> GetOneForStepAsync(Guid stepId, Guid providerId);
    }

    public record AlternativeProviderModel(
        Guid ProviderId,
        string ProviderName,
        Guid ProposalId,
        ProcessEstimateModel Estimate,
        ProviderScheduleModel Schedule);
}
