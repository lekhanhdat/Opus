using AvePoint.RA.Contract.MyHub.Items.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Items.Views
{
    public class RMMyhubPendingDisposalFolderFilterItem
    {
        public Guid NodeId { get; set; }
        public string PartitionKeyId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class RMMyhubPendingDisposalFolderFilterResult
    {
        public List<RMMyhubPendingDisposalFolderFilterItem> Items { get; set; }
        public string ContinuationToken { get; set; }
    }
    public class RMMyhubParameterBeforePendingDisposalQuery
    {
        public string DriveName { get; set; }
        public Guid FolderNodeId { get; set; }
        public string FolderPath { get; set; }
        public int IsPause { get; set; }
        public bool IsValid { get; set; } = true;
    }
}
