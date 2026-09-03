using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JobMonitorStatisticsDto
    {
        public long StartTime { get; set; }
        public long FinishTime { get; set; }
        public int JobType { get; set; }
    }
}
