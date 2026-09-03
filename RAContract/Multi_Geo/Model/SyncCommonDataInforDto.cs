using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Multi_Geo.Model
{
    [DataContract]
    public class SyncCommonDataInforDto
    {
        [DataMember]
        public string SQLiteDownloadUrl { get; set; }
        [DataMember]
        public long NeedUpdateTable { get; set; }
        [DataMember]
        public List<SyncCommonAzureInfoDto> SyncCommonImages { get; set; }

    }
}
