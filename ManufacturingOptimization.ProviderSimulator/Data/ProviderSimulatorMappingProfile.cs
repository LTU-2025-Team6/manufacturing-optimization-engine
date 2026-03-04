using AutoMapper;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Data;

/// <summary>
/// Unified mapping profile for ProviderSimulator project.
/// Contains only actively used mappings, organized by logical domain.
/// </summary>
public class ProviderSimulatorMappingProfile : Profile
{
    public ProviderSimulatorMappingProfile()
    {
        ConfigureProposalMappings();
        ConfigureEstimateMappings();
        ConfigureScheduleMappings();
        ConfigureMotorSpecificationsMappings();
    }

    /// <summary>
    /// Proposal entity <-> model mappings.
    /// Used in: ProcessProposalHandler, ProcessConfirmationHandler, ProposalService
    /// </summary>
    private void ConfigureProposalMappings()
    {
        // ProposalEntity <-> ProposalModel (full bidirectional)
        CreateMap<ProposalEntity, ProposalModel>();
        CreateMap<ProposalModel, ProposalEntity>()
            .ForMember(dest => dest.Execution, opt => opt.Ignore());

        // ExecutionModel -> ExecutionEntity (used when mapping ProposalModel that has an Execution)
        CreateMap<ExecutionModel, ExecutionEntity>()
            .ForMember(dest => dest.Proposal, opt => opt.Ignore())
            .ForMember(dest => dest.ScheduleSegments, opt => opt.Ignore());

        // ExecutionEntity -> ExecutionModel
        CreateMap<ExecutionEntity, ExecutionModel>()
            .ForMember(dest => dest.ScheduleSegments, opt => opt.Ignore());
    }

    /// <summary>
    /// Estimate entity/model conversions.
    /// Used in: ProcessProposalHandler, RequestExecutionDetailsHandler
    /// </summary>
    private void ConfigureEstimateMappings()
    {
        // EstimateEntity <-> EstimateModel (bidirectional, used when mapping ProposalEntity <-> ProposalModel)
        CreateMap<EstimateEntity, EstimateModel>()
            .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration));
        CreateMap<EstimateModel, EstimateEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProposalId, opt => opt.Ignore())
            .ForMember(dest => dest.Proposal, opt => opt.Ignore());

        // EstimateEntity -> ProcessEstimateModel (for responses to Gateway)
        CreateMap<EstimateEntity, ProcessEstimateModel>();

        // EstimateModel -> ProcessEstimateModel (for internal conversions)
        CreateMap<EstimateModel, ProcessEstimateModel>();
    }

    /// <summary>
    /// Schedule segment mappings for execution.
    /// Used in: ProposalService.ConfirmProposalAsync
    /// </summary>
    private void ConfigureScheduleMappings()
    {
        // ProviderScheduleSegmentModel -> ExecutionScheduleSegmentEntity
        // (convert schedule slots from Gateway into execution schedule segments)
        CreateMap<ProviderScheduleSegmentModel, ExecutionScheduleSegmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ExecutionId, opt => opt.Ignore())
            .ForMember(dest => dest.Execution, opt => opt.Ignore());
    }

    /// <summary>
    /// Motor specifications entity <-> model mappings.
    /// Used in: RequestExecutionDetailsHandler
    /// </summary>
    private void ConfigureMotorSpecificationsMappings()
    {
        // Bidirectional mapping between entity and model
        CreateMap<MotorSpecificationsEntity, MotorSpecificationsModel>();
        CreateMap<MotorSpecificationsModel, MotorSpecificationsEntity>();
    }
}
