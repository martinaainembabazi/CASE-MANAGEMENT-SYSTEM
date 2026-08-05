using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template.Data.Entities;

namespace Template.Core.Services.AdAuthentication
{
    public class AdUserResult
    {
        public UserPrincipal User { get; set; }
        public ApplicationUser AppUser { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public string ResultType { get; set; }
    }
}
