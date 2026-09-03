using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model.QueryRequest.Actions
{
    public class RMMyhubReportQueryInfo
    {
        public List<Guid> Ids { get; set; }
        public List<string> PartitionKeyId { get; set; }
        public int ReportType { get; set; }
    }

    public class RMMyHubDeleteReport
    {
        public List<Guid> JobIds { get; set; }

        public int ReportType { get; set; }
    }
}
