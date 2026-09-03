using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.MyHub.Items.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Items.Views
{
    // Query jop
    [DataContract]
    public class RMDisposalHistoryReportResult
    {
        [DataMember]
        public List<DisposalHistoryReportContentDto> Datas { get; set; }

        [DataMember]
        public DisposalHistoryPagingInfo PagingInfo { get; set; }

        [DataMember]
        public bool IsInProgress { get; set; }
    }

    [DataContract]
    public class RMDisposalHistoryReportInfo
    {
        [DataMember]
        public string FullPath { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string PartitionKeyId { get; set; }

        [DataMember]
        public DisposalHistoryPagingInfo PagingInfo { get; set; }
        [DataMember]
        public string OrderBy { get; set; }
        [DataMember]
        public bool IsDesc { get; set; }
    }

    [DataContract]
    public class DisposalHistoryPagingInfo
    {
        [DataMember]
        public string PageIndex { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int Total { get; set; }

        [DataMember]
        public bool HasNextPage { get; set; }
    }

    [DataContract]
    public class DisposalHistoryReportContentDto
    {
        [DataMember]
        public ArchivedContentDto ContentDto { get; set; }

        [DataMember]
        public int LastestExportType { get; set; }

        [DataMember]
        public ManualHistoryCustomDataTime TimeRange { get; set; }

        [DataMember]
        public string EndDateWithin { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
    }
}