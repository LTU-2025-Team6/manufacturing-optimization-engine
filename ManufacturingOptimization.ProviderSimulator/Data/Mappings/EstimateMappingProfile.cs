using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Data.Mappings;

public class EstimateMappingProfile : Profile
{
    public EstimateMappingProfile()
    {
        CreateMap<EstimateEntity, EstimateModel>();
        
        CreateMap<EstimateModel, EstimateEntity>()
            .ForMember(dest => dest.ProposalId, opt => opt.Ignore())
            .ForMember(dest => dest.Proposal, opt => opt.Ignore());

        CreateMap<ProcessEstimateModel, EstimateEntity>()
            .ForMember(dest => dest.ProposalId, opt => opt.Ignore())
            .ForMember(dest => dest.Proposal, opt => opt.Ignore());

        CreateMap<EstimateEntity, ProcessEstimateModel>();

        CreateMap<EstimateModel, ProcessEstimateModel>().ReverseMap();
    }
}