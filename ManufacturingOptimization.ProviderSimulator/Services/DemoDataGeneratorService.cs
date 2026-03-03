using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Extensions;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Models;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// Service for generating demo/test data for provider simulator.
/// Creates realistic execution schedules distributed over a time period.
/// </summary>
public class DemoDataGeneratorService
{
    private readonly ILogger<DemoDataGeneratorService> _logger;
    private readonly IProviderSimulationContext _context;
    private readonly IProposalService _proposalService;
    private readonly IExecutionRepository _executionRepository;
    private readonly DemoDataSettings _settings;
    private readonly Random _random;

    private static readonly MotorEfficiencyClass[] EfficiencyClasses = Enum.GetValues<MotorEfficiencyClass>();

    public DemoDataGeneratorService(
        ILogger<DemoDataGeneratorService> logger,
        IProviderSimulationContext context,
        IProposalService proposalService,
        IExecutionRepository executionRepository,
        IOptions<DemoDataSettings> settings)
    {
        _logger = logger;
        _context = context;
        _proposalService = proposalService;
        _executionRepository = executionRepository;
        _settings = settings.Value;
        _random = _settings.RandomSeed.HasValue 
            ? new Random(_settings.RandomSeed.Value) 
            : new Random();
    }

    /// <summary>
    /// Generate demo data for the configured time period.
    /// </summary>
    public async Task GenerateAsync()
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Demo data generation is disabled");
            return;
        }

        var totalExecutions = _settings.MonthsAhead * _settings.ExecutionsPerMonth;
        var generatedCount = 0;

        _logger.LogInformation("Starting demo data generation: {TotalExecutions} executions for provider {ProviderId}", 
            totalExecutions, _context.Provider.Id);

        for (int i = 0; i < totalExecutions; i++)
        {
            try
            {
                var success = await GenerateExecutionAsync();
                if (success)
                    generatedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate execution {Index}", i + 1);
            }
        }

        _logger.LogInformation("Demo data generation completed: {GeneratedCount}/{TotalExecutions} executions created for provider {ProviderId}", 
            generatedCount, totalExecutions, _context.Provider.Id);
    }

    /// <summary>
    /// Build schedule with available time slots.
    /// Same logic as ProcessProposalHandler.BuildSchedule.
    /// </summary>
    private async Task<ProviderScheduleModel> BuildScheduleAsync(DateTime startTime, DateTime endTime)
    {
        var breaks = _context.Provider.WorkingHours.GetBreakSegments(startTime, endTime);
        var occupied = await GetOccupiedSegmentsAsync(startTime, endTime);

        var baseTimeline = new List<ProviderScheduleSegmentModel>
        {
            new()
            {
                StartTime = startTime,
                EndTime = endTime,
                SegmentType = SegmentType.FreeSpace
            }
        };

        return new ProviderScheduleModel
        {
            Id = Guid.NewGuid(),
            Segments = baseTimeline
                .Overlay(breaks)
                .Overlay(occupied, ProviderScheduleSegmentExtensions.OverlayMode.ForceOverlay)
                .ToList()
        };
    }

    /// <summary>
    /// Get occupied segments from existing executions.
    /// Same logic as ProcessProposalHandler.GetOccupiedSegmentsAsync.
    /// </summary>
    private async Task<List<ProviderScheduleSegmentModel>> GetOccupiedSegmentsAsync(DateTime start, DateTime end)
    {
        var planned = await _executionRepository.GetAllExecutionScheduleSegmentsInTimeWindowAsync(
            _context.Provider.Id, start, end);

        return planned
            .Select(s => new ProviderScheduleSegmentModel
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SegmentType = SegmentType.Occupied,
                ExecutionId = s.ExecutionId
            })
            .ToList();
    }

    /// <summary>
    /// Generate a single execution with random data.
    /// </summary>
    private async Task<bool> GenerateExecutionAsync()
    {
        // Random process
        var supportedProcesses = _context.Provider.ProcessCapabilities.Select(c => c.Process).ToList();
        if (!supportedProcesses.Any())
            return false;

        var process = supportedProcesses[_random.Next(supportedProcesses.Count)];

        // Random duration (2-12 hours)
        var durationHours = _random.Next(2, 13);

        // Random time window
        var now = DateTime.UtcNow;
        var maxDaysAhead = _settings.MonthsAhead * 30;
        
        // Try up to 10 random time slots
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomDaysOffset = _random.Next(0, maxDaysAhead);
            var randomHour = _random.Next(8, 17);
            var randomStart = now.AddDays(randomDaysOffset).Date.AddHours(randomHour);
            var windowEnd = randomStart.AddDays(7);

            // Build schedule with breaks and occupied segments
            var schedule = await BuildScheduleAsync(randomStart, windowEnd);
            
            // Try to build a work slot (handles breaks automatically)
            var workSlot = schedule.Segments.TryBuildWorkSlot(randomStart, durationHours);
            
            if (workSlot != null && workSlot.Any())
            {
                // Create proposal with random motor specs
                var proposal = await _proposalService.CreateProposalAsync(
                    planId: Guid.NewGuid(),
                    process: process,
                    motorSpecs: GenerateMotorSpecifications());

                // Confirm execution
                await _proposalService.ConfirmProposalAsync(proposal.Id, new ProviderScheduleModel
                {
                    Id = Guid.NewGuid(),
                    Segments = workSlot.ToList()
                }, isDemo: true); // Mark as demo execution

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Generate realistic motor specifications.
    /// </summary>
    private MotorSpecificationsModel GenerateMotorSpecifications()
    {
        var hasMalfunction = _random.Next(100) < 30; // 30% chance of malfunction

        return new MotorSpecificationsModel
        {
            PowerKW = _random.Next(5, 200),
            CurrentEfficiency = EfficiencyClasses[_random.Next(Math.Max(0, EfficiencyClasses.Length - 4), EfficiencyClasses.Length)],
            TargetEfficiency = EfficiencyClasses[_random.Next(Math.Min(2, EfficiencyClasses.Length), EfficiencyClasses.Length)],
            AxisHeightMM = new[] { 90, 112, 132, 160, 180, 200, 225, 250, 280, 315 }[_random.Next(10)],
            MalfunctionDescription = hasMalfunction 
                ? new[] { "Bearing noise", "Overheating", "Vibration", "Low efficiency" }[_random.Next(4)]
                : null
        };
    }
}
