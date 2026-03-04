using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Extensions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;
using static ManufacturingOptimization.Common.Extensions.ProviderScheduleSegmentExtensions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Evaluates incoming process proposals and responds with feasibility estimates.
/// </summary>
public sealed class ProcessProposalHandler : IMessageHandler<ProposeProcessToProviderCommand>
{
    private readonly ILogger<ProcessProposalHandler> _logger;
    private readonly IMapper _mapper;
    private readonly IProviderSimulationContext _context;
    private readonly IMessagePublisher _publisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IProposalRepository _proposalRepository;
    private readonly IExecutionRepository _plannedProcessRepository;
    private readonly IEstimationService _estimationService;
    private readonly IProposalService _proposalService;

    public ProcessProposalHandler(
        ILogger<ProcessProposalHandler> logger,
        IMapper mapper,
        IProviderSimulationContext context,
        IMessagePublisher publisher,
        INotificationPublisher notificationPublisher,
        IProposalRepository proposalRepository,
        IExecutionRepository plannedProcessRepository,
        IEstimationService estimationService,
        IProposalService proposalService)
    {
        _context = context;
        _publisher = publisher;
        _notificationPublisher = notificationPublisher;
        _proposalRepository = proposalRepository;
        _plannedProcessRepository = plannedProcessRepository;
        _logger = logger;
        _mapper = mapper;
        _estimationService = estimationService;
        _proposalService = proposalService;
    }

    public async Task HandleAsync(ProposeProcessToProviderCommand command)
    {
        if (command.ProviderId != _context.Provider.Id)
            return;

        // Notify request received
        _notificationPublisher.NotifyProviderReceivedProposal(_context.Provider.Name, command.PlanId, command.Process);

        var proposal = CreateBaseProposal(command);
        var response = new ProcessProposalEstimatedEvent
        {
            ProviderId = _context.Provider.Id,
            ProviderName = _context.Provider.Name
        };

        try
        {
            var capability = GetProcessCapability(command.Process);
            if (capability is null)
                throw new InvalidOperationException($"{_context.Provider.Name} does not support process {command.Process}");

            proposal.Status = ProposalStatus.Accepted;
            
            // Calculate estimate using EstimationService
            var complexity = _estimationService.CalculateComplexityFactor(command.MotorSpecs, command.Process);
            var duration = _estimationService.CalculateDuration(command.Process, capability, command.MotorSpecs, complexity);
            
            proposal.Estimate = new EstimateModel
            {
                Cost = _estimationService.CalculateCost(capability, duration, command.MotorSpecs, complexity),
                QualityScore = _estimationService.CalculateQualityScore(capability, command.MotorSpecs, command.Process),
                EmissionsKgCO2 = _estimationService.CalculateEmissions(capability, duration, command.MotorSpecs),
                Duration = duration
            };

            response.Accepted = true;
            response.Estimate = _mapper.Map<ProcessEstimateModel>(proposal.Estimate);
            response.Schedule = await BuildSchedule(command);
        }
        catch (Exception ex)
        {
            proposal.Status = ProposalStatus.Declined;

            response.Accepted = false;
            response.DeclineReason = ex.Message;
        }
        finally
        {
            await SaveProposalAsync(proposal);
            response.ProposalId = proposal.Id;

            // Publish the estimate response
            _publisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Estimated}.{_context.Provider.Id}", response);

            // Notify request completed
            _notificationPublisher.NotifyProviderEstimatedProposal(_context.Provider.Name, command.PlanId, command.Process, response.Accepted, response.DeclineReason);
        }
    }

    public async Task<ProviderScheduleModel> BuildSchedule(ProposeProcessToProviderCommand command)
    {
        var breaks = _context.Provider.WorkingHours.GetBreakSegments(command.RequestedTimeWindow.StartTime, command.RequestedTimeWindow.EndTime);
        var occupied = await GetOccupiedSegmentsAsync(command.RequestedTimeWindow.StartTime, command.RequestedTimeWindow.EndTime);

        var baseTimeline = new List<ProviderScheduleSegmentModel>
        {
            new ProviderScheduleSegmentModel
            {
                StartTime = command.RequestedTimeWindow.StartTime,
                EndTime = command.RequestedTimeWindow.EndTime,
                SegmentType = SegmentType.FreeSpace
            }
        };

        return new ProviderScheduleModel
        {
            Id = Guid.NewGuid(),
            Segments = baseTimeline
                .Overlay(breaks)
                .Overlay(occupied, OverlayMode.ForceOverlay)
                .ToList()
        };
    }

    private ProposalModel CreateBaseProposal(ProposeProcessToProviderCommand command)
         => new()
        {
            PlanId = command.PlanId,
            ProviderId = _context.Provider.Id,
            Process = command.Process,
            MotorSpecs = command.MotorSpecs,
            ArrivedAt = DateTime.UtcNow
        };

    private async Task<List<ProviderScheduleSegmentModel>> GetOccupiedSegmentsAsync(DateTime start, DateTime end)
    {
        var planned = await _plannedProcessRepository.GetAllExecutionScheduleSegmentsInTimeWindowAsync(_context.Provider.Id, start, end);

        return planned
            .Select(s => new ProviderScheduleSegmentModel
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SegmentType = SegmentType.Occupied
            })
            .ToList();
    }

    private ProcessCapabilityModel? GetProcessCapability(ProcessType process)
         => _context.Provider.ProcessCapabilities
            .FirstOrDefault(p => p.Process == process);

    private async Task SaveProposalAsync(ProposalModel proposal)
    {
        proposal.ModifiedAt = DateTime.UtcNow;

        var entity = _mapper.Map<ProposalEntity>(proposal);

        await _proposalRepository.AddAsync(entity);
        await _proposalRepository.SaveChangesAsync();

        _mapper.Map(entity, proposal);
    }
}