using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles confirmation of a selected execution schedule for a process proposal.
/// </summary>
public sealed class ProcessConfirmationHandler : IMessageHandler<ConfirmProcessProposalCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProposalRepository _proposalRepository;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessConfirmationHandler> _logger;
    private readonly IProposalService _proposalService;

    public ProcessConfirmationHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        IProposalRepository proposalRepository,
        INotificationPublisher notificationPublisher,
        IMapper mapper,
        ILogger<ProcessConfirmationHandler> logger,
        IProposalService proposalService)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _proposalRepository = proposalRepository;
        _notificationPublisher = notificationPublisher;
        _mapper = mapper;
        _logger = logger;
        _proposalService = proposalService;
    }

    public async Task HandleAsync(ConfirmProcessProposalCommand command)
    {
        // Notify request received
        _notificationPublisher.NotifyProviderReceivedConfirmationRequest(_providerContext.Provider.Name, command.ProposalId, _providerContext.Provider.Id);

        var response = new ProcessProposalConfirmedEvent
        {
            ProposalId = command.ProposalId
        };

        try
        {
            var proposalEntity = await _proposalRepository.GetByIdWithDetailsAsync(command.ProposalId);
            if (proposalEntity == null)
                throw new InvalidOperationException("Proposal not found.");

            var proposal = _mapper.Map<ProposalModel>(proposalEntity);

            if (proposal.Estimate == null)
                throw new InvalidOperationException("No estimate available for the proposal.");

            await _proposalService.ConfirmProposalAsync(command.ProposalId, command.SelectedSchedule);

            response.IsAccepted = true;
        }
        catch (Exception ex)
        {
            response.IsAccepted = false;
            response.DeclineReason = ex.Message;
        }
        finally
        {

            // Publish the result of the confirmation process
            _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Confirmed}.{_providerContext.Provider.Id}", response);

            // Notify request completed
            _notificationPublisher.NotifyProviderCompletedConfirmationRequest(_providerContext.Provider.Name, command.ProposalId, _providerContext.Provider.Id, response.IsAccepted, response.DeclineReason);
        }
    }
}