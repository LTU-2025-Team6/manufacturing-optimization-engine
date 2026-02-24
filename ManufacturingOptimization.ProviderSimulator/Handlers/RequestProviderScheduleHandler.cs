using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using static ManufacturingOptimization.Common.Models.Extensions.ProviderScheduleSegmentExtensions;
using static ManufacturingOptimization.Common.Models.Extensions.WorkingHoursExtensions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class RequestProviderScheduleHandler : IMessageHandler<RequestProviderScheduleCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IExecutionRepository _executionRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<ProcessConfirmationHandler> _logger;

    public RequestProviderScheduleHandler(
        IProviderSimulationContext providerContext,
        IExecutionRepository executionRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        ILogger<ProcessConfirmationHandler> logger)
    {
        _providerContext = providerContext;
        _executionRepository = executionRepository;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(RequestProviderScheduleCommand command)
    {
        if (command.ProviderId != _providerContext.Provider.Id)
            return;

        // Notify request received
        _notificationPublisher.NotifyProviderReceivedScheduleRequest(_providerContext.Provider.Name, command.Start, command.End);

        var schedule = await BuildSchedule(command);
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderScheduleCreated, new ProviderScheduleCreatedEvent
        {
            ProviderId = _providerContext.Provider.Id,
            Start = command.Start,
            End = command.End,
            Schedules = schedule
        });

        // Notify request completed
        _notificationPublisher.NotifyProviderCompletedScheduleRequest(_providerContext.Provider.Name, command.Start, command.End);
    }

    public async Task<List<ProviderDayScheduleModel>> BuildSchedule(RequestProviderScheduleCommand command)
    {
        List<ProviderDayScheduleModel> dailySchedules = [];
        var date = command.Start.Date;

        while (date < command.End.Date)
        {
            var dailySchedule = await BuildDailySchedule(date);
            dailySchedules.Add(dailySchedule);
            date = date.AddDays(1);
        }

        return dailySchedules;
    }

    private async Task<ProviderDayScheduleModel> BuildDailySchedule(DateTime day)
    {
        var startOfDay = day.Date;
        var endOfDay = startOfDay.AddDays(1);

        var breaks = _providerContext.Provider.WorkingHours.GetBreakSegments(startOfDay, endOfDay);
        var occupied = await GetOccupiedSegmentsAsync(startOfDay, endOfDay);

        var baseTimeline = new List<ProviderScheduleSegmentModel>
        {
            new ProviderScheduleSegmentModel
            {
                StartTime = startOfDay,
                EndTime = endOfDay,
                SegmentType = SegmentType.FreeSpace
            }
        };

        return new ProviderDayScheduleModel
        {
            Date = day,
            Segments = baseTimeline
                .Overlay(breaks)
                .Overlay(occupied, OverlayMode.ForceOverlay)
                .ToList()
        };
    }

    private async Task<List<ProviderScheduleSegmentModel>> GetOccupiedSegmentsAsync(DateTime start, DateTime end)
    {
        var planned = await _executionRepository.GetAllExecutionScheduleSegmentsInTimeWindowAsync(_providerContext.Provider.Id, start, end);

        return planned
            .Select(s => new ProviderScheduleSegmentModel
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SegmentType = SegmentType.Occupied
            })
            .ToList();
    }
}