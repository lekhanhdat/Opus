using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JPMC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Myhub.Items.Views
{
    [DataContract]
    public class RMRCCReportResult
    {
        [DataMember]
        public List<RCCReportContentDto> Datas { get; set; }

        [DataMember]
        public RCCPagingInfo PagingInfo { get; set; }

        [DataMember]
        public bool IsInProgress { get; set; }
        [DataMember]
        public bool IsEnableMultiGeo { get; set; }
    }

    [DataContract]
    public class RMRCCReportInfo
    {
        [DataMember]
        public List<string> Ids { get; set; }
        [DataMember]
        public List<string> PartitionKeyId { get; set; }
        [DataMember]
        public RCCPagingInfo PagingInfo { get; set; }
        [DataMember]
        public string OrderBy { get; set; }
        [DataMember]
        public bool IsDesc { get; set; }
    }

    [DataContract]
    public class RCCPagingInfo
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
    public class RCCReportContentDto
    {
        [DataMember]
        public ArchivedContentDto ContentDto { get; set; }

        [DataMember]
        public RCCReportTimeRange TimeRange { get; set; }

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