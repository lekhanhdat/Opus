using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public class ManualPauseActionParam
    {
        [DataMember]
        public string PartitionKeyId { get; set; }
        [DataMember]
        public bool IsFolder { get; set; }
        [DataMember]
        public string Path { get; set; }
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public SOApproveDBStatus ActionType { get; set; }
    }
}
