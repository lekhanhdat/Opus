using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Multi_Geo.Model.QueryRequest
{
    public class UpdateLastSyncTimeRequest
    {
        public Guid ConnectionId { get; set; }
        public long LastSyncTime { get; set; }
    }
}
