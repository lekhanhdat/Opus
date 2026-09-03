using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Discovery.Model.Configuration
{
    [DataContract]
    public class DiscoverySpecificSiteDto
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string SiteCollectionUrl { get; set; }
    }

    [DataContract]
    public class DiscoverySpecificSiteInfo
    {

        [DataMember]
        public IEnumerable<DiscoverySpecificSiteDto> SiteCollections { get; set; }

        [DataMember]
        public long TotalCount { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract]
    public class DiscoverySpecificPageRequest 
    {
        [DataMember]
        public int PageIndex { get; set; }
        [DataMember]
        public int PageSize { get; set; }
    }
}
