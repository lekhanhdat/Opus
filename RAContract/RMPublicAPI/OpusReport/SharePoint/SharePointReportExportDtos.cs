using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMPublicAPI.OpusReport.SharePoint
{
    [DataContract]
    public class SPReportExportRequest
    {
        [DataMember]
        public List<string> SiteCollectionUrls { get; set; } = new List<string>();

        [DataMember]
        public string DestinationLibraryUrl { get; set; }
    }

    [DataContract]
    public class SPReportExportResponse
    {
        [DataMember]
        public bool Success { get; set; }

        //[DataMember]
        //public string JobId { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}