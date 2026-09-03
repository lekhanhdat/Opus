using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class OtherDCSyncCommonDataJobDetails : JMJobDetails
    {
        public string ActionName { get; set; }
        public string Type { get; set; }
    }
}
