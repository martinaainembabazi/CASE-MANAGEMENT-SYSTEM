using AutoMapper;
using Template.Core.Models.AuditLogs;
using Template.Core.Models.Permissions;
using Template.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Mappings
{
    public class AuditLogAutoMapperProfile : Profile
    {
        public AuditLogAutoMapperProfile()
        {
            CreateMap<AuditLog, ApplicationAuditLogViewModel>().ReverseMap();
        }
    }
}
