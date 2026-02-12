using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using System.Text.Json;

namespace ManufacturingOptimization.Common.Models.Data.Mappings
{
    public class OptimizationMappingProfile : Profile
    {
        public OptimizationMappingProfile()
        {
            CreateMap<OptimizationPlanEntity, OptimizationPlanModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OptimizationPlanStatus>(src.Status)))
                .ReverseMap()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SelectedStrategy, opt => opt.Ignore());

            CreateMap<OptimizationStrategyModel, OptimizationStrategyEntity>()
                .ForMember(dest => dest.Steps, opt => opt.MapFrom(src => src.Steps))
                .ForMember(dest => dest.Metrics, opt => opt.MapFrom(src => src.Metrics))
                .ForMember(dest => dest.Warranty, opt => opt.MapFrom(src => src.Warranty))
                .ForMember(dest => dest.Plan, opt => opt.Ignore());

            CreateMap<OptimizationStrategyEntity, OptimizationStrategyModel>();

            CreateMap<WarrantyTermsModel, WarrantyTermsEntity>()
                .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
                .ForMember(dest => dest.Strategy, opt => opt.Ignore());

            CreateMap<WarrantyTermsEntity, WarrantyTermsModel>();

            CreateMap<ProcessStepModel, ProcessStepEntity>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
                .ForMember(dest => dest.Estimate, opt => opt.MapFrom(src => src.Estimate))
                .ForMember(dest => dest.ProviderSchedule, opt => opt.MapFrom(src => src.AllocatedSchedule))
                .ForMember(dest => dest.ProviderScheduleId, opt => opt.Ignore())
                .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
                .ForMember(dest => dest.Strategy, opt => opt.Ignore());

            CreateMap<ProcessStepEntity, ProcessStepModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)))
                .ForMember(dest => dest.AllocatedSchedule, opt => opt.MapFrom(src => src.ProviderSchedule));


            CreateMap<ProcessEstimateModel, ProcessEstimateEntity>()
                .ForMember(dest => dest.ProcessStepId, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessStep, opt => opt.Ignore());

            CreateMap<ProcessEstimateEntity, ProcessEstimateModel>();

            CreateMap<OptimizationMetricsModel, OptimizationMetricsEntity>()
                .ForMember(dest => dest.TotalTime, opt => opt.MapFrom(src => src.TotalDuration.Ticks))
                .ForMember(dest => dest.TotalEmissionsKgCO2, opt => opt.MapFrom(src => src.TotalEmissionsKgCO2))
                .ForMember(dest => dest.StrategyId, opt => opt.Ignore())
                .ForMember(dest => dest.Strategy, opt => opt.Ignore());

            CreateMap<OptimizationMetricsEntity, OptimizationMetricsModel>()
                .ForMember(dest => dest.TotalDuration, opt => opt.MapFrom(src => TimeSpan.FromTicks(src.TotalTime)))
                .ForMember(dest => dest.TotalEmissionsKgCO2, opt => opt.MapFrom(src => src.TotalEmissionsKgCO2));

            CreateMap<ProviderScheduleModel, ProviderScheduleEntity>()
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartWorkingTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndWorkingTime))
                .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Segments));

            CreateMap<ProviderScheduleEntity, ProviderScheduleModel>()
                .ForMember(dest => dest.Segments, opt => opt.MapFrom(src => src.Segments));

            CreateMap<ProviderScheduleSegmentModel, ProviderScheduleSegmentEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderScheduleId, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderSchedule, opt => opt.Ignore())
                .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => src.SegmentType.ToString()));

            CreateMap<ProviderScheduleSegmentEntity, ProviderScheduleSegmentModel>()
                .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => Enum.Parse<SegmentType>(src.SegmentType)));
        }
    }
}
