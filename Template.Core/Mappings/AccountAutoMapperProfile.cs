using AutoMapper;
using Template.Core.Models.Account;
using Template.Data.Entities;

namespace Template.Core.Mappings
{
    public class AccountAutoMapperProfile : Profile
    {
        public AccountAutoMapperProfile()
        {
            CreateMap<ApplicationUser, ApplicationUserViewModel>().ReverseMap();
        }
    }
}
