using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class ExecutionScheduleSegmentMappingProfile : Profile
{
    public ExecutionScheduleSegmentMappingProfile()
    {
        CreateMap<ProviderScheduleSegmentModel, ExecutionScheduleSegmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ExecutionScheduleSegmentEntity, ProviderScheduleSegmentModel>()
            .ForMember(dest => dest.SegmentType, opt => opt.Ignore());

        CreateMap<ProviderScheduleSegmentModel, ExecutionScheduleSegmentModel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<ExecutionScheduleSegmentModel, ProviderScheduleSegmentModel>()
            .ForMember(dest => dest.SegmentType, opt => opt.Ignore());
    }
}
