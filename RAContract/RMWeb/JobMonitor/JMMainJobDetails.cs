namespace AvePoint.RA.Contract.RMWeb.JobMonitor
{
    public class JMMainJobDetails : JMJobDetails
    {
        public string SubJobID { get; set; }
        public new JobStatus Status { get; set; }
        public string Scope { get; set; }
        public long SuccessfulCount { get; set; }
        public long FailedCount { get; set; }
        public long SkippedCount { get; set; }
        public bool IsSavedJobDetails { get; set; }
    }
}
