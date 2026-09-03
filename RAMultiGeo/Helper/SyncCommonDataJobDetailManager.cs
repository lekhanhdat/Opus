using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace RAMultiGeo.Helper
{
    public class SyncCommonDataJobDetailManager
    {
        private static IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;
        public static bool HasSucceed { get; set; }
        public static bool HasFailed { get; set; }

        public static string? JobComment { get; set; }

        public static void Init(string jobId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ReportMangerFactory.Instance.Init(jobId, jobType);
            ReportManager.StartUpdateJobProgress(60);
        }

        public static void AddFailedJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);
            HasFailed = true;
        }

        public static void AddExceptionJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);
            HasFailed = true;
            HasSucceed = true;
        }

        public static void AddSucceedJobDetail(JMJobDetails detail)
        {
            ReportManager.SendJobDetail(detail);
            HasSucceed = true;
        }

        public static void SetJobFinished()
        {
            var status = JobStatus.Finished;
            if (HasFailed && HasSucceed)
            {
                status = JobStatus.FinishWithException;
            }
            else if (HasFailed)
            {
                status = JobStatus.Failed;
            }

            ReportManager.SetJobFinished(status, JobComment);
        }
    }
}
