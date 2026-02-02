using AutoMapper;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class ProposalMappingProfile : Profile
{
    public ProposalMappingProfile()
    {
        CreateMap<ProposalEntity, ProposalModel>().ReverseMap();
        CreateMap<ExecutionEntity, ExecutionModel>().ReverseMap();
    }
}