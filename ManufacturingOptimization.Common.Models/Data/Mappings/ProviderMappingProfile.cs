using AutoMapper;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using System.Text.Json;

namespace ManufacturingOptimization.Common.Models.Data.Mappings
{
    public class ProviderMappingProfile : Profile
    {
        public ProviderMappingProfile()
        {
            CreateMap<ProviderModel, ProviderEntity>()
                .ForMember(dest => dest.ProcessCapabilities, opt => opt.MapFrom(src => src.ProcessCapabilities))
                .ForMember(dest => dest.TechnicalCapabilities, opt => opt.MapFrom(src => src.TechnicalCapabilities))
                .ForMember(dest => dest.WorkingHours, opt => opt.MapFrom(src => src.WorkingHours));

            CreateMap<ProviderEntity, ProviderModel>();

            CreateMap<ProcessCapabilityModel, ProcessCapabilityEntity>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => src.Process.ToString()))
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.Provider, opt => opt.Ignore());

            CreateMap<ProcessCapabilityEntity, ProcessCapabilityModel>()
                .ForMember(dest => dest.Process, opt => opt.MapFrom(src => Enum.Parse<ProcessType>(src.Process)));

            CreateMap<TechnicalCapabilitiesModel, TechnicalCapabilitiesEntity>()
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.Provider, opt => opt.Ignore());

            CreateMap<TechnicalCapabilitiesEntity, TechnicalCapabilitiesModel>();

            CreateMap<ProviderWorkingHoursModel, ProviderWorkingHoursEntity>()
                .ForMember(dest => dest.WorkingDaysJson, opt => opt.MapFrom(src => SerializeWorkingDays(src.WorkingDays)))
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.Provider, opt => opt.Ignore())
                .ForMember(dest => dest.Breaks, opt => opt.MapFrom(src => src.Breaks));

            CreateMap<ProviderWorkingHoursEntity, ProviderWorkingHoursModel>()
                .ForMember(dest => dest.WorkingDays, opt => opt.MapFrom(src => DeserializeWorkingDays(src.WorkingDaysJson)));

            CreateMap<ProviderBreakPeriodModel, ProviderBreakPeriodEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderId, opt => opt.Ignore())
                .ForMember(dest => dest.WorkingHours, opt => opt.Ignore());

            CreateMap<ProviderBreakPeriodEntity, ProviderBreakPeriodModel>();
        }

        private static string SerializeWorkingDays(HashSet<DayOfWeek> workingDays)
        {
            return JsonSerializer.Serialize(workingDays);
        }

        private static HashSet<DayOfWeek> DeserializeWorkingDays(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new HashSet<DayOfWeek>();
            
            return JsonSerializer.Deserialize<HashSet<DayOfWeek>>(json) ?? new HashSet<DayOfWeek>();
        }
    }
}
