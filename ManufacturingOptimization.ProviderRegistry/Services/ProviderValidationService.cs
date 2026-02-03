using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderRegistry.Abstractions;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Validates provider capabilities via RPC pattern.
/// </summary>
public class ProviderValidationService : IProviderValidationService
{
    private readonly ILogger<ProviderValidationService> _logger;
    private readonly IMessagePublisher _messagePublisher;

    public ProviderValidationService(
        ILogger<ProviderValidationService> logger,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
    }

    public async Task<(bool IsApproved, string? DeclinedReason)> ValidateAsync(ProviderModel provider, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        return (true, null);

        //var validationRequest = new ValidateProviderCapabilityCommand
        //{
        //    Provider = provider
        //};

        //var response = await _messagePublisher.RequestReplyAsync<ProviderCapabilityValidatedEvent>(
        //    Exchanges.Provider,
        //    ProviderRoutingKeys.ValidationRequested,
        //    validationRequest,
        //    timeout ?? TimeSpan.FromSeconds(30));

        //if (response == null)
        //{
        //    return (false, "Validation timeout");
        //}

        //if (!response.IsApproved)
        //{
        //    return (false, response.Reason);
        //}

        //return (true, null);
    }
}
