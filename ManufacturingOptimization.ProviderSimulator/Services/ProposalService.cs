using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// Service for creating and managing proposals.
/// Shared logic between ProcessProposalHandler and DemoDataGenerator.
/// </summary>
public class ProposalService : IProposalService
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IEstimationService _estimationService;
    private readonly IProposalRepository _proposalRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProposalService> _logger;

    public ProposalService(
        IProviderSimulationContext providerContext,
        IEstimationService estimationService,
        IProposalRepository proposalRepository,
        IMapper mapper,
        ILogger<ProposalService> logger)
    {
        _providerContext = providerContext;
        _estimationService = estimationService;
        _proposalRepository = proposalRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProposalModel> CreateProposalAsync(
        Guid planId,
        ProcessType process,
        MotorSpecificationsModel motorSpecs,
        DateTime? arrivedAt = null)
    {
        var capability = GetProcessCapability(process)
            ?? throw new InvalidOperationException($"{_providerContext.Provider.Name} does not support process {process}");

        var proposal = new ProposalModel
        {
            PlanId = planId,
            ProviderId = _providerContext.Provider.Id,
            Process = process,
            MotorSpecs = motorSpecs,
            ArrivedAt = arrivedAt ?? DateTime.UtcNow,
            Status = ProposalStatus.Accepted
        };

        // Build estimate
        var complexity = _estimationService.CalculateComplexityFactor(motorSpecs, process);
        var duration = _estimationService.CalculateDuration(process, capability, motorSpecs, complexity);

        proposal.Estimate = new EstimateModel
        {
            Cost = _estimationService.CalculateCost(capability, duration, motorSpecs, complexity),
            QualityScore = _estimationService.CalculateQualityScore(capability, motorSpecs, process),
            EmissionsKgCO2 = _estimationService.CalculateEmissions(capability, duration, motorSpecs),
            Duration = duration
        };

        // Save to database
        proposal.ModifiedAt = DateTime.UtcNow;
        var entity = _mapper.Map<ProposalEntity>(proposal);
        await _proposalRepository.AddAsync(entity);
        await _proposalRepository.SaveChangesAsync();

        // Map back to get generated IDs
        _mapper.Map(entity, proposal);

        return proposal;
    }

    public async Task ConfirmProposalAsync(Guid proposalId, ProviderScheduleModel schedule)
    {
        var proposalEntity = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposalEntity == null)
            throw new InvalidOperationException($"Proposal {proposalId} not found.");

        if (proposalEntity.Estimate == null)
            throw new InvalidOperationException($"Proposal {proposalId} has no estimate.");

        var workingSegments = schedule.Segments
            .Where(s => s.SegmentType == SegmentType.WorkingTime)
            .Select(_mapper.Map<ExecutionScheduleSegmentEntity>)
            .ToList();

        proposalEntity.Status = ProposalStatus.Confirmed;
        proposalEntity.ModifiedAt = DateTime.UtcNow;
        
        var executionEntity = new ExecutionEntity
        {
            ProposalId = proposalEntity.Id,
            ScheduleSegments = workingSegments
        };
        
        proposalEntity.Execution = executionEntity;
        
        foreach (var segment in workingSegments)
        {
            segment.ExecutionId = executionEntity.Id;
        }

        await _proposalRepository.UpdateAsync(proposalEntity);
        await _proposalRepository.SaveChangesAsync();
    }

    public async Task CancelProposalAsync(Guid proposalId)
    {
        var proposalEntity = await _proposalRepository.GetByIdAsync(proposalId);
        if (proposalEntity == null)
            throw new InvalidOperationException($"Proposal {proposalId} not found.");

        if (proposalEntity.Status != ProposalStatus.Confirmed)
            throw new InvalidOperationException($"Proposal {proposalId} is not confirmed and cannot be cancelled.");

        // Remove execution and revert to Accepted status
        proposalEntity.Execution = null;
        proposalEntity.Status = ProposalStatus.Accepted;
        proposalEntity.ModifiedAt = DateTime.UtcNow;

        await _proposalRepository.UpdateAsync(proposalEntity);
        await _proposalRepository.SaveChangesAsync();
    }

    private ProcessCapabilityModel? GetProcessCapability(ProcessType process)
        => _providerContext.Provider.ProcessCapabilities
            .FirstOrDefault(p => p.Process == process);
}
