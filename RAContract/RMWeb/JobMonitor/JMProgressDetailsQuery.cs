using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMProgressDetailsQuery
    {
        [DataMember]
        public string JobID { get; set; }
        [DataMember]
        public int JobType { get; set; }
        [DataMember]
        public string SearchValue { get; set; }
        [DataMember]
        public string[] SearchKeys { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public int PageNumber { get; set; }
        [DataMember]
        public ProgressStatus[] StatusFilter { get; set; }
    }
}
