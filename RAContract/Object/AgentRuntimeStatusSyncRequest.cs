using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Object
{
    [DataContract]
    public class AgentRuntimeStatusSyncRequest
    {
        [DataMember]
        public RMAgentDto Agent { get; set; }

        [DataMember]
        public AgentRuntimeStatusSyncAction Action { get; set; }
    }

    [DataContract]
    public enum AgentRuntimeStatusSyncAction
    {
        [EnumMember]
        UpdateStatus,

        [EnumMember]
        UpdateResourceUsage,
    }
}