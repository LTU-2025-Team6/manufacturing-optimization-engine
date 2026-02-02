using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class MotorSpecificationsMappingProfile : Profile
{
    public MotorSpecificationsMappingProfile()
    {
        CreateMap<MotorSpecificationsEntity, MotorSpecificationsModel>().ReverseMap();
    }
}
