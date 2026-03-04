using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles cancellation of a confirmed process proposal.
/// </summary>
public sealed class CancelProcessHandler : IMessageHandler<CancelProcessCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProposalService _proposalService;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<CancelProcessHandler> _logger;

    public CancelProcessHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        IProposalService proposalService,
        INotificationPublisher notificationPublisher,
        ILogger<CancelProcessHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _proposalService = proposalService;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(CancelProcessCommand command)
    {
        _notificationPublisher.NotifyProviderReceivedCancellationRequest(_providerContext.Provider.Name, command.ProposalId);

        var response = new ProcessCancelledEvent
        {
            ProposalId = command.ProposalId
        };

        try
        {
            await _proposalService.CancelProposalAsync(command.ProposalId);

            response.Success = true;

            _logger.LogInformation("✓ Proposal {ProposalId} cancelled successfully for provider {ProviderName}", 
                command.ProposalId, _providerContext.Provider.Name);
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = ex.Message;

            _logger.LogError(ex, "✗ Failed to cancel proposal {ProposalId} for provider {ProviderName}", 
                command.ProposalId, _providerContext.Provider.Name);
        }
        finally
        {
            // Publish the result of the cancellation process
            _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Cancelled}.{_providerContext.Provider.Id}", response);

            _notificationPublisher.NotifyProviderCompletedCancellationRequest(
                _providerContext.Provider.Name, 
                command.ProposalId, 
                response.Success, 
                response.ErrorMessage);
        }
    }
}
