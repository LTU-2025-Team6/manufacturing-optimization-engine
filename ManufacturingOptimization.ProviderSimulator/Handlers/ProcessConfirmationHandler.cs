using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
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
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessConfirmationHandler> _logger;

    public ProcessConfirmationHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        IProposalRepository proposalRepository,
        IMapper mapper,
        ILogger<ProcessConfirmationHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _proposalRepository = proposalRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(ConfirmProcessProposalCommand command)
    {
        var response = new ProcessProposalReviewedEvent
        {
            ProposalId = command.ProposalId
        };

        try
        {
            var proposalEntity = await _proposalRepository.GetByIdAsync(command.ProposalId);
            if (proposalEntity == null)
                throw new InvalidOperationException("Proposal not found.");

            var proposal = _mapper.Map<ProposalModel>(proposalEntity);

            if (proposal.Estimate == null)
                throw new InvalidOperationException("No estimate available for the proposal.");

            ConfirmProposal(proposalEntity, command.SelectedSchedule);

            await _proposalRepository.UpdateAsync(proposalEntity);
            await _proposalRepository.SaveChangesAsync();

            response.IsAccepted = true;
        }
        catch (Exception ex)
        {
            response.IsAccepted = false;
            response.DeclineReason = ex.Message;
        }
        finally
        {
            _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Reviewed}.{_providerContext.Provider.Id}", response);
        }
    }

    private void ConfirmProposal(ProposalEntity proposalEntity, ProviderScheduleModel schedule)
    {
        var workingSegments = schedule.Segments
            .Where(s => s.SegmentType == SegmentType.WorkingTime)
            .Select(_mapper.Map<ExecutionScheduleSegmentEntity>)
            .ToList();

        proposalEntity.Status = ProposalStatus.Confirmed;
        proposalEntity.ModifiedAt = DateTime.UtcNow;
        proposalEntity.Execution = new ExecutionEntity
        {
            ProposalId = proposalEntity.Id,
            ScheduleSegments = workingSegments
        };

        _proposalRepository.UpdateAsync(proposalEntity);
        _proposalRepository.SaveChangesAsync();
    }
}