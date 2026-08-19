using AutoMapper;
using Template.Core.Models.Roles;
using Template.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Mappings
{
    public class ApplicationRoleAutoMapperProfile : Profile
    {
        public ApplicationRoleAutoMapperProfile()
        {
            CreateMap<Role, RoleViewModel>().ReverseMap();

            CreateMap<IdentityRole<Guid>, ApplicationRoleViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? Guid.NewGuid() : Guid.Parse(src.Id)));
            //CreateMap<IdentityRole, RoleListViewModel>();

            //CreateMap<RoleListViewModel, IdentityRole>().ReverseMap();
            CreateMap<RoleListViewModel, IdentityRole<Guid>>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.Id) ? Guid.NewGuid() : Guid.Parse(src.Id)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.NormalizedName, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
