using AutoMapper;
using Template.Core.Models.LawFirm;
using Template.Data.Entities;

namespace Template.Core.Mappings;

public class LawFirmMappingProfile : Profile
{
    public LawFirmMappingProfile()
    {
        CreateMap<LawFirm, LawFirmViewModel>()
            .ForMember(dest => dest.TotalLawyersCount, opt => opt.MapFrom(src => src.Lawyers != null ? src.Lawyers.Count : 0))
            .ForMember(dest => dest.TotalCaseAssignmentsCount, opt => opt.MapFrom(src => src.CaseAssignments != null ? src.CaseAssignments.Count : 0));

        CreateMap<LawFirmViewModel, LawFirm>();
    }
}