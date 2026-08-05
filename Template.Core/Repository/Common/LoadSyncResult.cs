using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.Core.Repository.Common
{
    public class LoadSyncResult
    {
        public int CountNewAdded { get; set; }
        public int CountDeactivated { get; set; }
        public int CountReactivated { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
