using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Media.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class APStorageCostEvaluationJobInfo
    {
        [DataMember]
        public LogicalDeviceDto SourceDevice { get; set; }
    }
}
