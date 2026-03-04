using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Gateway.Data.Repositories
{
    public class InMemoryAlternativeProvidersRepository : IAlternativeProvidersRepository
    {
        public static ConcurrentDictionary<Guid, List<AlternativeProviderModel>> AlternativeProvidersByStep { get; } = new();

        public Task<List<AlternativeProviderModel>?> GetAlternativesForStepAsync(Guid stepId, CancellationToken cancellationToken = default)
        {
            AlternativeProvidersByStep.TryGetValue(stepId, out var alternatives);
            return Task.FromResult(alternatives);
        }

        public Task AddAlternativesForStepAsync(Guid stepId, List<AlternativeProviderModel> alternatives, CancellationToken cancellationToken = default)
        {
            AlternativeProvidersByStep[stepId] = alternatives;
            return Task.CompletedTask;
        }

        public Task<AlternativeProviderModel?> GetOneForStepAsync(Guid stepId, Guid providerId)
        {
            if (AlternativeProvidersByStep.TryGetValue(stepId, out var alternatives))
                return Task.FromResult(alternatives.FirstOrDefault(a => a.ProviderId == providerId));

            return Task.FromResult<AlternativeProviderModel?>(null);
        }
    }
}
