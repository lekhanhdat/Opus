using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Multi_Geo.Model
{
    [DataContract]
    public class MultiGeoSettingInfoDto
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string DCInternalName { get; set; }
        [DataMember]
        public string DCDisplayName { get; set; }
        [DataMember]
        public string IPAddresses { get; set; }
    }
}
