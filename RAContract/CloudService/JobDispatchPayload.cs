using AvePoint.RA.Contract.JobMonitor;

namespace AvePoint.RA.Contract.CloudService
{
    public sealed class JobDispatchPayload
    {
        public JobType TargetJobType { get; set; }

        public string Parameters { get; set; }
        public string OriginalMessageId { get; set; }
        public string OriginalTenantId { get; set; }
    }
}
