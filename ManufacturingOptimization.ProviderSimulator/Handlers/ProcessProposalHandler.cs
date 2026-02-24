using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.Common.Models.Extensions;
using ManufacturingOptimization.ProviderSimulator.Models;
using static ManufacturingOptimization.Common.Models.Extensions.ProviderScheduleSegmentExtensions;
using ManufacturingOptimization.Common.Messaging.Messages;

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

    public ProcessProposalHandler(
        ILogger<ProcessProposalHandler> logger,
        IMapper mapper,
        IProviderSimulationContext context,
        IMessagePublisher publisher,
        INotificationPublisher notificationPublisher,
        IProposalRepository proposalRepository,
        IExecutionRepository plannedProcessRepository)
    {
        _context = context;
        _publisher = publisher;
        _notificationPublisher = notificationPublisher;
        _proposalRepository = proposalRepository;
        _plannedProcessRepository = plannedProcessRepository;
        _logger = logger;
        _mapper = mapper;
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
            proposal.Estimate = BuildEstimateAsync(command, capability);

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

    private EstimateModel BuildEstimateAsync(ProposeProcessToProviderCommand command, ProcessCapabilityModel capability)
    {
        var motorSpecs = command.MotorSpecs;
        var complexity = CalculateComplexityFactor(motorSpecs, command.Process);
        var duration = CalculateDuration(command.Process, capability, motorSpecs, complexity);
        
        return new EstimateModel
        {
            Cost = CalculateCost(capability, duration, motorSpecs, complexity),
            QualityScore = CalculateQualityScore(capability, motorSpecs, command.Process),
            EmissionsKgCO2 = CalculateEmissions(capability, duration, motorSpecs),
            Duration = duration
        };
    }

    private double CalculateComplexityFactor(MotorSpecificationsModel motorSpecs, ProcessType process)
    {
        double complexity = 1.0;

        // Larger motors are more complex to handle
        if (motorSpecs.PowerKW > 100)
            complexity += 0.3;
        else if (motorSpecs.PowerKW > 50)
            complexity += 0.15;

        // Larger axis height increases complexity
        if (motorSpecs.AxisHeightMM > 315)
            complexity += 0.25;
        else if (motorSpecs.AxisHeightMM > 200)
            complexity += 0.1;

        // Efficiency upgrade complexity
        var efficiencyGap = (int)motorSpecs.TargetEfficiency - (int)motorSpecs.CurrentEfficiency;
        if (efficiencyGap > 2)
            complexity += 0.4;
        else if (efficiencyGap > 1)
            complexity += 0.2;
        else if (efficiencyGap > 0)
            complexity += 0.1;

        // Malfunction adds complexity for various processes
        if (!string.IsNullOrWhiteSpace(motorSpecs.MalfunctionDescription))
        {
            if (process == ProcessType.Disassembly || process == ProcessType.PartSubstitution)
                complexity += 0.5; // Major impact on disassembly and repair
            else if (process == ProcessType.Certification)
                complexity += 0.3; // Moderate impact on testing
            else if (process == ProcessType.Cleaning)
                complexity += 0.2; // Some impact on cleaning damaged parts
        }

        return complexity;
    }

    private double CalculateDuration(ProcessType process, ProcessCapabilityModel capability, MotorSpecificationsModel motorSpecs, double complexity)
    {
        var baseDuration = _context.StandardDurations
            .GetValueOrDefault(process, 8.0);

        var duration = baseDuration * capability.SpeedMultiplier * complexity;

        // Additional time for high-power motors in certain processes
        if (motorSpecs.PowerKW > 150)
        {
            if (process == ProcessType.Certification || process == ProcessType.Disassembly || process == ProcessType.Reassembly)
                duration += 2.0; // Extra 2 hours for large motor handling
            else if (process == ProcessType.Turning || process == ProcessType.Grinding)
                duration += 1.5; // Extra time for precision work on large components
        }

        return duration;
    }

    private decimal CalculateCost(ProcessCapabilityModel capability, double duration, MotorSpecificationsModel motorSpecs, double complexity)
    {
        var baseCost = capability.CostPerHour * (decimal)duration;

        // Premium for high-power motors (specialized equipment needed)
        if (motorSpecs.PowerKW > 100)
            baseCost *= 1.15m;

        // Premium for large motors (handling equipment)
        if (motorSpecs.AxisHeightMM > 280)
            baseCost *= 1.1m;

        // Additional cost for efficiency upgrade materials
        var efficiencyGap = (int)motorSpecs.TargetEfficiency - (int)motorSpecs.CurrentEfficiency;
        if (efficiencyGap > 0)
            baseCost += efficiencyGap * 500m; // Material costs per efficiency class

        return baseCost;
    }

    private double CalculateQualityScore(ProcessCapabilityModel capability, MotorSpecificationsModel motorSpecs, ProcessType process)
    {
        var baseQuality = capability.QualityScore;

        // Quality may vary based on motor size - larger motors are harder to work with
        if (motorSpecs.AxisHeightMM > 315)
            baseQuality -= 0.05; // Slightly lower quality for very large motors
        else if (motorSpecs.AxisHeightMM < 132)
            baseQuality += 0.03; // Easier to achieve high quality on smaller motors

        if (process == ProcessType.PartSubstitution || process == ProcessType.Reassembly)
            baseQuality -= 0.08; // Unknown issues may affect final quality
        else if (process == ProcessType.Certification)
            baseQuality -= 0.05; // May affect test results

        // Large efficiency jumps may be harder to guarantee
        var efficiencyGap = (int)motorSpecs.TargetEfficiency - (int)motorSpecs.CurrentEfficiency;
        if (efficiencyGap > 2)
            baseQuality -= 0.1;

        // Precision processes benefit from smaller motors
        if (process == ProcessType.Grinding || process == ProcessType.Turning)
        {
            if (motorSpecs.AxisHeightMM < 160)
                baseQuality += 0.05;
        }

        // Clamp between 0 and 1
        return Math.Max(0.0, Math.Min(1.0, baseQuality));
    }

    private double CalculateEmissions(ProcessCapabilityModel capability, double duration, MotorSpecificationsModel motorSpecs)
    {
        var baseEmissions = capability.EnergyConsumptionKwhPerHour
             * capability.CarbonIntensityKgCO2PerKwh
             * duration;

        // Higher power motors require more energy to process (testing, handling equipment)
        var powerFactor = 1.0 + (motorSpecs.PowerKW / 1000.0); // Small increase based on motor power

        return baseEmissions * powerFactor;
    }

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