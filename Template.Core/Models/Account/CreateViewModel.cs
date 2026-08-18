using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Template.Core.Models.Account;

public class CreateViewModel
{
    [Required(ErrorMessage = "Active Directory Username is required.")]
    [Display(Name = "Active Directory Username")]
    public string UserName { get; set; } = string.Empty;
}
