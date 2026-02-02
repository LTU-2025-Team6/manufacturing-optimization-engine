using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.DTOs;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Gateway.Data
{
    public class GatewayMappingProfile : Profile
    {
        public GatewayMappingProfile()
        {
            CreateMap<OptimizationRequestModel, OptimizationRequestDto>()
                .ForMember(dest => dest.MotorSpecs, opt => opt.MapFrom(src => src.MotorSpecs))
                .ForMember(dest => dest.Constraints, opt => opt.MapFrom(src => src.Constraints));
            CreateMap<OptimizationRequestDto, OptimizationRequestModel>()
                .ForMember(dest => dest.RequestId, opt => Guid.NewGuid())
                .ForMember(dest => dest.MotorSpecs, opt => opt.MapFrom(src => src.MotorSpecs))
                .ForMember(dest => dest.Constraints, opt => opt.MapFrom(src => src.Constraints));

            CreateMap<OptimizationRequestConstraintsModel, OptimizationRequestConstraintsDto>().ReverseMap();

            CreateMap<TimeWindowModel, TimeWindowDto>().ReverseMap();

            CreateMap<ProviderScheduleSegmentModel, ProviderScheduleSegmentDto>()
                .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => src.SegmentType.ToString()));
            CreateMap<ProviderScheduleSegmentDto, ProviderScheduleSegmentModel>()
                .ForMember(dest => dest.SegmentType, opt => opt.MapFrom(src => Enum.Parse<SegmentType>(src.SegmentType)));

            CreateMap<MotorSpecificationsModel, MotorSpecificationsDto>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => src.CurrentEfficiency.ToString()))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => src.TargetEfficiency.ToString()));
            CreateMap<MotorSpecificationsDto, MotorSpecificationsModel>()
                .ForMember(dest => dest.CurrentEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.CurrentEfficiency)))
                .ForMember(dest => dest.TargetEfficiency, opt => opt.MapFrom(src => Enum.Parse<MotorEfficiencyClass>(src.TargetEfficiency)));

            CreateMap<OptimizationPlanModel, OptimizationPlanDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<OptimizationPlanDto, OptimizationPlanModel>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<OptimizationPlanStatus>(src.Status)));

            CreateMap<OptimizationStrategyModel, OptimizationStrategyDto>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
            CreateMap<OptimizationStrategyDto, OptimizationStrategyModel>()
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<OptimizationPriority>(src.Priority)));

            CreateMap<OptimizationMetricsModel, OptimizationMetricsDto>().ReverseMap();

            CreateMap<ProviderScheduleModel, ProviderScheduleDto>().ReverseMap();

            CreateMap<ProcessStepModel, ProcessStepDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()));
            CreateMap<ProcessStepDto, ProcessStepModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

            CreateMap<ProcessEstimateModel, ProcessEstimateDto>().ReverseMap();

            CreateMap<ProcessCapabilityModel, ProcessCapabilityDto>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()));
            CreateMap<ProcessCapabilityDto, ProcessCapabilityModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

            CreateMap<ProviderModel, ProviderDto>().ReverseMap();

            CreateMap<TechnicalCapabilitiesModel, TechnicalCapabilitiesDto>().ReverseMap();

            CreateMap<WarrantyTermsModel, WarrantyTermsDto>().ReverseMap();
        }
    }
}
