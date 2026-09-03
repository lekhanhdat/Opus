using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Multi_Geo.Model
{
    [DataContract]
    public class MultiGeoDCInfo
    {
        [DataMember]
        public string MainDC { get; set; }

        [DataMember]
        public string CurrentDC { get; set; }

        [DataMember]
        public List<DataCenterInfo> DCsSupported { get; set; }
    }
}
