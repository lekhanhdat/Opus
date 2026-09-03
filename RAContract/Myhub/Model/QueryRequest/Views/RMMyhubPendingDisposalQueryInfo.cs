using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views
{
    public class RMMyhubPendingDisposalQueryInfo
    {
        public string PartitionKeyId { get; set; }
        public Guid NodeId { get; set; }
    }
    public class RMMyhubPendingDisposalFolderFilterQueryInfo
    {
        public string PartitionKeyId { get; set; }
        public string NodeId { get; set; }
        public string SearchValue { get; set; }
        public string ContinuationToken { get; set; }
        public int PageSize { get; set; }
    }
}
