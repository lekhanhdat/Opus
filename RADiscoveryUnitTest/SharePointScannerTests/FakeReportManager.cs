using System;
using System.Collections.Generic;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;

namespace RADiscoveryUnitTest.SharePointScannerTests
{
    public class FakeReportManager : IRMReportManager
    {
        public List<JMArchiverActionJobDetails> FailedReports { get; } = new();
        public List<JMArchiverActionJobDetails> AllReports { get; } = new();

        public string JobId { get; } = Guid.NewGuid().ToString();
        public JobType JobType { get; } = JobType.ArchiverBackup;
        public int DetailBufferCount { get; set; }

        public void SendJobDetail(JMArchiverActionJobDetails detail)
        {
            AllReports.Add(detail);
            if (detail.Status == JobDetailsStatus.Failed)
            {
                FailedReports.Add(detail);
            }
        }

        public void SendJobDetail(JMJobDetails detail) { }
        public void SendJobReport(BaseReport report) { }
        
        public void IncreaseBase(long count) { }
        public void Increase() { }
        public void Increase(int count) { }
        public int GetProgress() => 0;
        public long GetFinished() => 0;
        public long GetTotal() => 0;
        public void SetTotal(long total) { }
        public void SetProgress(int progress) { }
        public void WeightCoefficient(double coefficient) { }
        public void StartUpdateJobProgress(int updateIntervalInMillionSeconds) { }
        public void SetJobFinished(JobStatus status, string comments) { }
        public void BatchSendJobDetail(IEnumerable<JMJobDetails> details) { }
        public void BatchSendJobReport(IEnumerable<BaseReport> reports) { }
        public void WaitReportFinish() { }
        public System.Threading.Tasks.Task MonitorExceptionAsync(string jobId, MonitorExceptionType type) => System.Threading.Tasks.Task.CompletedTask;
        public void WaitFlushAllDetail() { }
        public List<JMJobDetails> GetCacheJobDetails() => new();

        public void StartUpdateJobProgressByPhase(int totalPhases, int updateTime = 8) { }
        public void AdvanceToNextPhase() { }
        public void DecreaseTotalPhases(int count) { }
    }
}
