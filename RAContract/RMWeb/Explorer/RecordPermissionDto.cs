using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Explorer
{
    public class RecordPermissionDto
    {
        public Guid RecordId { get; set; }
        public bool HasDelegatedAdmin { get; set; }
    }
}
