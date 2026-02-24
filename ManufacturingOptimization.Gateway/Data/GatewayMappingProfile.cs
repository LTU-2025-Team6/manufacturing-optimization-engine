using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Gateway.DTOs;
using System.Text.Json;

namespace ManufacturingOptimization.Gateway.Data
{
    public class GatewayMappingProfile : Profile
    {
        public GatewayMappingProfile()
        {
            ConfigureOptimizationMappings();
            ConfigureProviderMappings();
            ConfigureProcessMappings();
            ConfigureEntityToDtoMappings();
        }

        private void ConfigureOptimizationMappings()
        {
            // Optimization Request
            CreateMap<OptimizationRequestModel, OptimizationRequestDto>()
                .ForMember(dest => dest.MotorSpecs, opt => opt.MapFrom(src => src.MotorSpecs))
                .ForMember(dest => dest.Constraints, opt => opt.MapFrom(src => src.Constraints));
            CreateMap<OptimizationRequestDto, OptimizationRequestModel>()
                .ForMember(dest => dest.RequestId, opt => Guid.NewGuid())
                .ForMember(dest => dest.MotorSpecs, opt => opt.MapFrom(src => src.MotorSpecs))
                .ForMember(dest => dest.Constraints, opt => opt.MapFrom(src => src.Constraints));

            // Optimization Request Entity mappings
            CreateMap<OptimizationRequestDto, OptimizationRequestEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()));
            CreateMap<OptimizationRequestModel, OptimizationRequestEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RequestId));
            CreateMap<OptimizationRequestEntity, OptimizationRequestModel>()
                .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id));

            CreateMap<OptimizationRequestConstraintsModel, OptimizationRequestConstraintsDto>().ReverseMap();
            CreateMap<OptimizationRequestConstraintsModel, OptimizationRequestConstraintsEntity>().ReverseMap();
            CreateMap<OptimizationRequestConstraintsDto, OptimizationRequestConstraintsEntity>().ReverseMap();

            CreateMap<MotorSpecificationsModel, MotorSpecificationsDto>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => src.CurrentEfficiency.ToString()))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => src.TargetEfficiency.ToString()));
            CreateMap<MotorSpecificationsDto, MotorSpecificationsModel>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));
            CreateMap<MotorSpecificationsModel, MotorSpecificationsEntity>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => src.CurrentEfficiency.ToString()))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => src.TargetEfficiency.ToString()));
            CreateMap<MotorSpecificationsEntity, MotorSpecificationsModel>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));
            CreateMap<MotorSpecificationsDto, MotorSpecificationsEntity>().ReverseMap();

            CreateMap<TimeWindowModel, TimeWindowDto>().ReverseMap();
            CreateMap<TimeWindowModel, TimeWindowEntity>().ReverseMap();
            CreateMap<TimeWindowDto, TimeWindowEntity>().ReverseMap();

            // Optimization Plan & Strategy
            CreateMap<OptimizationPlanModel, OptimizationPlanDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<OptimizationPlanDto, OptimizationPlanModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OptimizationPlanStatus>(src.Status)));

            CreateMap<OptimizationPlanEntity, OptimizationPlanPreviewDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<OptimizationStrategyModel, OptimizationStrategyDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
            CreateMap<OptimizationStrategyDto, OptimizationStrategyModel>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<OptimizationPriority>(src.Priority)));

            CreateMap<OptimizationMetricsModel, OptimizationMetricsDto>().ReverseMap();
        }

        private void ConfigureProviderMappings()
        {
            // Provider Schedule
            CreateMap<ProviderScheduleModel, ProviderScheduleDto>();
            CreateMap<ProviderScheduleDto, ProviderScheduleModel>()
                .ForMember(dest => dest.StartWorkingTime, opt => opt.Ignore())
                .ForMember(dest => dest.EndWorkingTime, opt => opt.Ignore());
            CreateMap<ProviderDayScheduleModel, ProviderDayScheduleDto>().ReverseMap();
            CreateMap<ProviderScheduleSegmentModel, ProviderScheduleSegmentDto>()
                .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => src.SegmentType.ToString()));
            CreateMap<ProviderScheduleSegmentDto, ProviderScheduleSegmentModel>()
                .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => Enum.Parse<SegmentType>(src.SegmentType)));

            // Provider & Capabilities
            CreateMap<ProviderModel, ProviderDto>().ReverseMap();
            CreateMap<TechnicalCapabilitiesModel, TechnicalCapabilitiesDto>().ReverseMap();
            CreateMap<WarrantyTermsModel, WarrantyTermsDto>().ReverseMap();
        }

        private void ConfigureProcessMappings()
        {
            // Process Step & Estimate
            CreateMap<ProcessStepModel, ProcessStepDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()));
            CreateMap<ProcessStepDto, ProcessStepModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));
            CreateMap<ProcessEstimateModel, ProcessEstimateDto>().ReverseMap();

            // Process Capability
            CreateMap<ProcessCapabilityModel, ProcessCapabilityDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()));
            CreateMap<ProcessCapabilityDto, ProcessCapabilityModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));
        }

        private void ConfigureEntityToDtoMappings()
        {
            // Provider Entity
            CreateMap<ProviderEntity, ProviderDto>().ReverseMap();
            CreateMap<ProviderEntity, ProviderPreviewDto>().ReverseMap();

            // Process Capability Entity
            CreateMap<ProcessCapabilityEntity, ProcessCapabilityDto>().ReverseMap();

            // Technical Capabilities Entity
            CreateMap<TechnicalCapabilitiesEntity, TechnicalCapabilitiesDto>().ReverseMap();

            // OptimizationStrategy
            CreateMap<OptimizationStrategyEntity, OptimizationStrategyDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
                .ReverseMap();

            // OptimizationPlan
            CreateMap<OptimizationPlanEntity, OptimizationPlanDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // ProcessStep
            CreateMap<ProcessStepEntity, ProcessStepDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process))
                .ForMember(dest => dest.AllocatedSchedule, opt => opt.MapFrom(src => src.ProviderSchedule));

            // ProcessEstimate
            CreateMap<ProcessEstimateEntity, ProcessEstimateDto>();

            // OptimizationMetrics
            CreateMap<OptimizationMetricsEntity, OptimizationMetricsDto>()
                .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.TotalTime)));

            // WarrantyTerms
            CreateMap<WarrantyTermsEntity, WarrantyTermsDto>();

            // ProviderSchedule
            CreateMap<ProviderScheduleEntity, ProviderScheduleDto>();
            CreateMap<ProviderScheduleSegmentEntity, ProviderScheduleSegmentDto>();

            // Working Hours Entity with custom WorkingDays conversion
            CreateMap<ProviderWorkingHoursEntity, ProviderWorkingHoursDto>()
                .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src => DeserializeWorkingDays(src.WorkingDaysJson)));
            CreateMap<ProviderWorkingHoursDto, ProviderWorkingHoursEntity>()
                .ForMember(dest => dest.WorkingDaysJson, opt => opt.MapFrom(src => SerializeWorkingDays(src.WorkingDays)))
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.Provider, opt => opt.Ignore());

            // Break Period Entity
            CreateMap<ProviderBreakPeriodEntity, ProviderBreakPeriodDto>();
            CreateMap<ProviderBreakPeriodDto, ProviderBreakPeriodEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.WorkingHours, opt => opt.Ignore());

            // Notification
            CreateMap<NotificationEntity, NotificationDto>();
            CreateMap<NotificationEntity, NotificationPreviewDto>();
        }

        private static List<string> DeserializeWorkingDays(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<string>();

            var workingDays = JsonSerializer.Deserialize<HashSet<DayOfWeek>>(json) ?? new HashSet<DayOfWeek>();
            return workingDays.Select(d => d.ToString()).ToList();
        }

        private static string SerializeWorkingDays(List<string> workingDays)
        {
            if (workingDays == null || workingDays.Count == 0)
                return string.Empty;

            var daysOfWeek = workingDays.Select(d => Enum.Parse<DayOfWeek>(d)).ToHashSet();
            return JsonSerializer.Serialize(daysOfWeek);
        }
    }
}
