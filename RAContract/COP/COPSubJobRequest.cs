using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.COP
{
    public class COPSubJobRequest
    {
        public string JobId { get; set; }
        public int[] SubJobStatusFilters { get; set; }
        public string SearchKey { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
    }
}
