using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Model.QueryRequest.Views
{
    public class RMMyhubFolderStatisticsQueryInfo
    {
        public string PartitionKeyId { get; set; }

        public List<RMMyhubFolderNodeInfo> Nodes { get; set; } = new();
    }

    public class RMMyhubFolderNodeInfo
    {
        public string NodeId { get; set; }
        public string FolderPath { get; set; }
    }
}
