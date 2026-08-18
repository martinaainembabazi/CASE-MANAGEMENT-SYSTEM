
using AutoMapper;
using Template.Data.Entities;
using Template.Core.Models.Cases;

namespace Template.Core.Mappings;

public class CasesAutoMapperProfile : Profile
{
    public CasesAutoMapperProfile()
    {
        // Domain Entity -> View Model (Used for Details, Delete, and Index views)
        CreateMap<Case, CaseViewModel>()
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type != null ? src.Type.Name : string.Empty))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : string.Empty))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.ToString()));

        // View Model -> Domain Entity (Used for Create & Update actions)
        CreateMap<CaseViewModel, Case>();
    }
}