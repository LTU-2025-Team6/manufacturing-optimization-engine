using AutoMapper;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Gateway.DTOs.Notification;
using ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;
using ManufacturingOptimization.Gateway.DTOs.OptimizationRequest;
using ManufacturingOptimization.Gateway.DTOs.Provider;
using System.Text.Json;

namespace ManufacturingOptimization.Gateway.Data;

/// <summary>
/// Unified mapping profile for Gateway project.
/// Contains only actively used mappings, organized by logical domain.
/// </summary>
public class GatewayMappingProfile : Profile
{
    public GatewayMappingProfile()
    {
        ConfigureProviderMappings();
        ConfigureOptimizationRequestMappings();
        ConfigureOptimizationPlanMappings();
        ConfigureOptimizationStrategyMappings();
        ConfigureScheduleMappings();
        ConfigureNotificationMappings();
    }

    /// <summary>
    /// Provider entity/model/DTO mappings.
    /// Used in: ProviderService, DatabaseManagementService, StartAllProvidersHandler
    /// </summary>
    private void ConfigureProviderMappings()
    {
        // ProviderModel -> ProviderEntity (for saving static providers to DB)
        CreateMap<ProviderModel, ProviderEntity>()
            .ForMember(dest => dest.ProcessCapabilities, opt => opt.MapFrom(src => src.ProcessCapabilities))
            .ForMember(dest => dest.TechnicalCapabilities, opt => opt.MapFrom(src => src.TechnicalCapabilities))
            .ForMember(dest => dest.WorkingHours, opt => opt.MapFrom(src => src.WorkingHours));

        // ProviderEntity -> ProviderModel (for messaging)
        CreateMap<ProviderEntity, ProviderModel>();

        // CreateProviderRequest -> ProviderEntity (API create)
        CreateMap<CreateProviderRequest, ProviderEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.IsRunning, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.EnvironmentSource, opt => opt.MapFrom(src => "manual"));

        // ProviderEntity -> ProviderDto (API response full details)
        CreateMap<ProviderEntity, ProviderDto>();

        // ProviderEntity -> ProviderPreviewDto (API response list)
        CreateMap<ProviderEntity, ProviderPreviewDto>();

        // ProcessCapability mappings
        CreateMap<ProcessCapabilityModel, ProcessCapabilityEntity>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<ProcessCapabilityEntity, ProcessCapabilityModel>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

        CreateMap<CreateProcessCapabilityRequest, ProcessCapabilityEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<ProcessCapabilityEntity, ProcessCapabilityDto>();

        // TechnicalCapabilities mappings
        CreateMap<TechnicalCapabilitiesModel, TechnicalCapabilitiesEntity>()
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<TechnicalCapabilitiesEntity, TechnicalCapabilitiesModel>();

        CreateMap<CreateTechnicalCapabilitiesRequest, TechnicalCapabilitiesEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore());

        CreateMap<TechnicalCapabilitiesEntity, TechnicalCapabilitiesDto>();

        // WorkingHours mappings
        CreateMap<ProviderWorkingHoursModel, ProviderWorkingHoursEntity>()
            .ForMember(dest => dest.WorkingDaysJson, opt => opt.MapFrom(src => SerializeWorkingDays(src.WorkingDays)))
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.Breaks, opt => opt.MapFrom(src => src.Breaks));

        CreateMap<ProviderWorkingHoursEntity, ProviderWorkingHoursModel>()
            .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src => DeserializeWorkingDaysToModel(src.WorkingDaysJson)));

        CreateMap<CreateWorkingHoursRequest, ProviderWorkingHoursEntity>()
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.Provider, opt => opt.Ignore())
            .ForMember(dest => dest.WorkingDaysJson, opt => opt.MapFrom(src => SerializeWorkingDaysFromDto(src.WorkingDays)));

        CreateMap<ProviderWorkingHoursEntity, ProviderWorkingHoursDto>()
            .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src => DeserializeWorkingDaysToDto(src.WorkingDaysJson)));

        // BreakPeriod mappings
        CreateMap<ProviderBreakPeriodModel, ProviderBreakPeriodEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkingHours, opt => opt.Ignore());

        CreateMap<ProviderBreakPeriodEntity, ProviderBreakPeriodModel>();

        CreateMap<CreateBreakPeriodRequest, ProviderBreakPeriodEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
            .ForMember(dest => dest.WorkingHours, opt => opt.Ignore());

        CreateMap<ProviderBreakPeriodEntity, ProviderBreakPeriodDto>();
    }

    /// <summary>
    /// Optimization request mappings.
    /// Used in: OptimizationRequestService
    /// </summary>
    private void ConfigureOptimizationRequestMappings()
    {
        // OptimizationRequestDto -> OptimizationRequestEntity (API request -> DB)
        CreateMap<OptimizationRequestDto, OptimizationRequestEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));

        // OptimizationRequestEntity -> OptimizationRequestModel (DB -> messaging)
        CreateMap<OptimizationRequestEntity, OptimizationRequestModel>()
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id));

        // OptimizationRequestEntity -> OptimizationRequestDto (DB -> API response)
        CreateMap<OptimizationRequestEntity, OptimizationRequestDto>();

        // MotorSpecifications (DTO <-> Model for API and messaging)
        CreateMap<MotorSpecificationsDto, MotorSpecificationsModel>()
            .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
            .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));

        CreateMap<MotorSpecificationsModel, MotorSpecificationsDto>()
            .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => src.CurrentEfficiency.ToString()))
            .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => src.TargetEfficiency.ToString()));

        // MotorSpecifications (DTO <-> Entity for API and DB)
        CreateMap<MotorSpecificationsDto, MotorSpecificationsEntity>();
        CreateMap<MotorSpecificationsEntity, MotorSpecificationsDto>();
        // MotorSpecifications (Entity -> Model for DB to messaging)
        CreateMap<MotorSpecificationsEntity, MotorSpecificationsModel>()
            .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
            .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));
        // Constraints mappings
        CreateMap<OptimizationRequestConstraintsDto, OptimizationRequestConstraintsEntity>();
        CreateMap<OptimizationRequestConstraintsEntity, OptimizationRequestConstraintsModel>();
        CreateMap<OptimizationRequestConstraintsEntity, OptimizationRequestConstraintsDto>();

        // TimeWindow mappings
        CreateMap<TimeWindowDto, TimeWindowEntity>();
        CreateMap<TimeWindowEntity, TimeWindowModel>();
        CreateMap<TimeWindowEntity, TimeWindowDto>();
    }

    /// <summary>
    /// Optimization plan mappings.
    /// Used in: OptimizationRequestService, OptimizationPlanService
    /// </summary>
    private void ConfigureOptimizationPlanMappings()
    {
        // OptimizationPlanModel -> OptimizationPlanEntity (for creating new plan)
        CreateMap<OptimizationPlanModel, OptimizationPlanEntity>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.SelectedStrategy, opt => opt.Ignore());

        // OptimizationPlanEntity -> OptimizationPlanModel (DB -> service layer)
        CreateMap<OptimizationPlanEntity, OptimizationPlanModel>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OptimizationPlanStatus>(src.Status)));

        // OptimizationPlanModel -> OptimizationPlanDto (service -> API response)
        CreateMap<OptimizationPlanModel, OptimizationPlanDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // OptimizationPlanEntity -> OptimizationPlanDto (DB -> API response direct)
        CreateMap<OptimizationPlanEntity, OptimizationPlanDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // OptimizationPlanEntity -> OptimizationPlanPreviewDto (DB -> API list)
        CreateMap<OptimizationPlanEntity, OptimizationPlanPreviewDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }

    /// <summary>
    /// Optimization strategy, steps, estimates, metrics, warranty mappings.
    /// Used in: OptimizationPlanUpdatedHandler, OptimizationStrategyService, OptimizationPlanService
    /// </summary>
    private void ConfigureOptimizationStrategyMappings()
    {
        // OptimizationStrategyModel -> OptimizationStrategyEntity (message -> DB)
        CreateMap<OptimizationStrategyModel, OptimizationStrategyEntity>()
            .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps))
            .ForMember(dest => dest.Metrics, opt => opt.MapFrom(src => src.Metrics))
            .ForMember(dest => dest.Warranty, opt => opt.MapFrom(src => src.Warranty))
            .ForMember(dest => dest.Plan, opt => opt.Ignore());

        // OptimizationStrategyEntity -> OptimizationStrategyModel (DB -> service)
        CreateMap<OptimizationStrategyEntity, OptimizationStrategyModel>();

        // OptimizationStrategyEntity -> OptimizationStrategyDto (DB -> API response)
        CreateMap<OptimizationStrategyEntity, OptimizationStrategyDto>()
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

        // ProcessStepModel -> ProcessStepEntity (message -> DB)
        CreateMap<ProcessStepModel, ProcessStepEntity>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
            .ForMember(dest => dest.Estimate, opt => opt.MapFrom(src => src.Estimate))
            .ForMember(dest => dest.ProviderSchedule, opt => opt.MapFrom(src => src.AllocatedSchedule))
            .ForMember(dest => dest.ProviderScheduleId, opt => opt.Ignore())
            .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
            .ForMember(dest => dest.Strategy, opt => opt.Ignore());

        // ProcessStepEntity -> ProcessStepModel (DB -> service)
        CreateMap<ProcessStepEntity, ProcessStepModel>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)))
            .ForMember(dest => dest.AllocatedSchedule, opt => opt.MapFrom(src => src.ProviderSchedule));

        // ProcessStepEntity -> ProcessStepDto (DB -> API response)
        CreateMap<ProcessStepEntity, ProcessStepDto>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process))
            .ForMember(dest => dest.AllocatedSchedule, opt => opt.MapFrom(src => src.ProviderSchedule))
            .ForMember(dest => dest.ExecutionStatus, opt => opt.MapFrom(src => src.ExecutionStatus.ToString()));

        // ProcessEstimateModel -> ProcessEstimateEntity (message -> DB)
        CreateMap<ProcessEstimateModel, ProcessEstimateEntity>()
            .ForMember(dest => dest.ProcessStepId, opt => opt.Ignore())
            .ForMember(dest => dest.ProcessStep, opt => opt.Ignore());

        // ProcessEstimateEntity -> ProcessEstimateModel (DB -> service)
        CreateMap<ProcessEstimateEntity, ProcessEstimateModel>();

        // ProcessEstimateModel -> ProcessEstimateDto (service -> API)
        CreateMap<ProcessEstimateModel, ProcessEstimateDto>();

        // ProcessEstimateEntity -> ProcessEstimateDto (DB -> API)
        CreateMap<ProcessEstimateEntity, ProcessEstimateDto>();

        // OptimizationMetricsModel -> OptimizationMetricsEntity (message -> DB)
        CreateMap<OptimizationMetricsModel, OptimizationMetricsEntity>()
            .ForMember(dest => dest.TotalTime, opt => opt.MapFrom(src => src.TotalDuration.Ticks))
            .ForMember(dest => dest.TotalEmissionsKgCO2, opt => opt.MapFrom(src => src.TotalEmissionsKgCO2))
            .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
            .ForMember(dest => dest.Strategy, opt => opt.Ignore());

        // OptimizationMetricsEntity -> OptimizationMetricsModel (DB -> service)
        CreateMap<OptimizationMetricsEntity, OptimizationMetricsModel>()
            .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.TotalTime)))
            .ForMember(dest => dest.TotalEmissionsKgCO2, opt => opt.MapFrom(src => src.TotalEmissionsKgCO2));

        // OptimizationMetricsEntity -> OptimizationMetricsDto (DB -> API)
        CreateMap<OptimizationMetricsEntity, OptimizationMetricsDto>()
            .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.TotalTime)));

        // WarrantyTermsModel -> WarrantyTermsEntity (message -> DB)
        CreateMap<WarrantyTermsModel, WarrantyTermsEntity>()
            .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
            .ForMember(dest => dest.Strategy, opt => opt.Ignore());

        // WarrantyTermsEntity -> WarrantyTermsModel (DB -> service)
        CreateMap<WarrantyTermsEntity, WarrantyTermsModel>();

        // WarrantyTermsEntity -> WarrantyTermsDto (DB -> API)
        CreateMap<WarrantyTermsEntity, WarrantyTermsDto>();
    }

    /// <summary>
    /// Schedule and execution mappings.
    /// Used in: ProviderService, OptimizationStrategyService
    /// </summary>
    private void ConfigureScheduleMappings()
    {
        // ProviderScheduleModel -> ProviderScheduleEntity (message/service -> DB)
        CreateMap<ProviderScheduleModel, ProviderScheduleEntity>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartWorkingTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndWorkingTime))
            .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Segments));

        // ProviderScheduleEntity -> ProviderScheduleModel (DB -> service)
        CreateMap<ProviderScheduleEntity, ProviderScheduleModel>()
            .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Segments));

        // ProviderScheduleModel -> ProviderScheduleDto (service -> API)
        CreateMap<ProviderScheduleModel, ProviderScheduleDto>();

        // ProviderScheduleEntity -> ProviderScheduleDto (DB -> API)
        CreateMap<ProviderScheduleEntity, ProviderScheduleDto>()
            .ForMember(dest => dest.StartWorkingTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndWorkingTime,   opt => opt.MapFrom(src => src.EndTime));

        // ProviderDayScheduleModel -> ProviderDayScheduleDto (service -> API)
        CreateMap<ProviderDayScheduleModel, ProviderDayScheduleDto>();

        // ProviderScheduleSegmentModel -> ProviderScheduleSegmentEntity (message -> DB)
        CreateMap<ProviderScheduleSegmentModel, ProviderScheduleSegmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderScheduleId, opt => opt.Ignore())
            .ForMember(dest => dest.ProviderSchedule, opt => opt.Ignore())
            .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => src.SegmentType.ToString()));

        // ProviderScheduleSegmentEntity -> ProviderScheduleSegmentModel (DB -> service)
        CreateMap<ProviderScheduleSegmentEntity, ProviderScheduleSegmentModel>()
            .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => Enum.Parse<SegmentType>(src.SegmentType)));

        // ProviderScheduleSegmentModel -> ProviderScheduleSegmentDto (service -> API)
        CreateMap<ProviderScheduleSegmentModel, ProviderScheduleSegmentDto>()
            .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => src.SegmentType.ToString()));

        // ProviderScheduleSegmentEntity -> ProviderScheduleSegmentDto (DB -> API)
        CreateMap<ProviderScheduleSegmentEntity, ProviderScheduleSegmentDto>();

        // ExecutionDetailsModel -> ExecutionDetailsDto (message -> API)
        CreateMap<ExecutionDetailsModel, ExecutionDetailsDto>()
            .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // ExecutionTimeSlot -> ExecutionScheduleSegmentDto (message -> API)
        CreateMap<ExecutionTimeSlot, ExecutionScheduleSegmentDto>();
    }

    /// <summary>
    /// Notification mappings.
    /// Used in: NotificationService
    /// </summary>
    private void ConfigureNotificationMappings()
    {
        // NotificationEntity -> NotificationDto (DB -> API full details)
        CreateMap<NotificationEntity, NotificationDto>();
        
        // NotificationEntity -> NotificationPreviewDto (DB -> API list view)
        CreateMap<NotificationEntity, NotificationPreviewDto>();
    }

    // Helper methods for WorkingDays serialization
    private static string SerializeWorkingDays(HashSet<DayOfWeek> workingDays)
    {
        return JsonSerializer.Serialize(workingDays);
    }

    private static string SerializeWorkingDaysFromDto(List<string> workingDays)
    {
        if (workingDays == null || workingDays.Count == 0)
            return string.Empty;

        var daysOfWeek = workingDays.Select(d => Enum.Parse<DayOfWeek>(d)).ToHashSet();
        return JsonSerializer.Serialize(daysOfWeek);
    }

    private static HashSet<DayOfWeek> DeserializeWorkingDaysToModel(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new HashSet<DayOfWeek>();

        return JsonSerializer.Deserialize<HashSet<DayOfWeek>>(json) ?? new HashSet<DayOfWeek>();
    }

    private static List<string> DeserializeWorkingDaysToDto(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();

        var workingDays = JsonSerializer.Deserialize<HashSet<DayOfWeek>>(json) ?? new HashSet<DayOfWeek>();
        return workingDays.Select(d => d.ToString()).ToList();
    }
}
