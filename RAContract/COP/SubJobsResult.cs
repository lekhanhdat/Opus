using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.COP
{
    public class SubJobsResult
    {
        public string JobType { get; set; }
        public string SubJobId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public JobStatus Status { get; set; }
        public string Duration { get; set; }
        public double Progress { get; set; }
    }
}
