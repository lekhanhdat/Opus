using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMPublicAPI.JPMC
{
    public class JobReportParam
    {
        public long? StartTime { get; set; }

        public long? EndTime { get; set; }

        public JobType? JobType { get; set; }

        public JobStatus? Status { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public string PartitionKey { get; set; }
    }

    public class JobReportItem
    {
        public string JobId { get; set; }
        public JobType JobType { get; set; }
        public JobStatus Status { get; set; }
        public long Duration { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }
    }

    public class JobReportResult
    {
        public List<JobReportItem> Items { get; set; }
        public int TotalCount { get; set; }
    }

   
}
