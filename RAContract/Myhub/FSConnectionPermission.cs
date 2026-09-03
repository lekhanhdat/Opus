using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub
{
    public class FSConnectionPermission
    {
        public FSConnectionOwnerType OwnerType { get; set; }

        public List<Guid> ConnectionIds { get; set; } = new();
    }

    public enum FSConnectionOwnerType
    {
        None = 0,
        RecordOwner = 1,
        InformationOwner = 2
    }
}
