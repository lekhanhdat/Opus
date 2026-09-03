using AvePoint.GCommon.Utility;
using AvePoint.Media.Storage;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemJobProgressTracker
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMFileSystemJobProgressTracker));

        private readonly RMFileSystemJobExecutionInfo _executionInfo;

        private readonly string _jobId;

        private readonly IReportService<JMJobDetails> _reportService;

        private readonly IProgressService _progressService;

        private int _failedCount = 0;

        private int _succeedCount = 0;

        public RMFileSystemJobProgressTracker(RMFileSystemJobExecutionInfo executionInfo)
        {
            _executionInfo = executionInfo;
            _jobId = JobContext.Current.JobId;
            _reportService = JobContext.Current.JobDetailManager.Create();
            _progressService = JobContext.Current.mProgressManager.Create();
            _progressService.IncreaseBase(100);
            _progressService.Increase(1);
        }

        public void IncreseBaseProgress(int count)
        {
            if (count <= 0) return;
            _progressService.IncreaseBase(count);
        }

        public void IncreaseFailedCount()
        {
            Interlocked.Add(ref _failedCount, 1);
        }

        public void AddJobDetail(FileSystemRecordDto item, bool succeed)
        {
            _reportService.Commit(new FSDataSyncJobReportDetail
            {
                AgentName = OSInformation.HostName,
                ObjectName = item.LeafName,
                FullPath = item.FullPath,
                Status = succeed ? JobDetailsStatus.Successful : JobDetailsStatus.Failed,
                Comment = succeed ? "" : "RM_JM_FSFailedAddToExplorer",
            });
            _progressService.Increase();
            if (succeed)
            {
                Interlocked.Increment(ref _succeedCount);
            }
            else
            {
                Interlocked.Increment(ref _failedCount);
            }
        }

        public void AddJobDetails(List<FileSystemRecordDto> items, bool succeed)
        {
            items.ForEach(item => AddJobDetail(item, succeed));
        }

        public void AddFailedJobDetail(RMFileSystemItemMetadata item)
        {
            _reportService.Commit(new FSDataSyncJobReportDetail
            {
                AgentName = OSInformation.HostName,
                ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(item.FullPath),
                FullPath = item.FullPath,
                Status = JobDetailsStatus.Failed,
            });
            _progressService.Increase();
            Interlocked.Increment(ref _failedCount);
        }

        public void AddFailedJobDetail(XDirectoryInfo directory)
        {
            var fullPath = ExternalUtil.CombinePath(_executionInfo.ConnectionPath, directory.HighName, directory.LowName);
            _reportService.Commit(new FSDataSyncJobReportDetail
            {
                AgentName = OSInformation.HostName,
                ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(fullPath),
                FullPath = fullPath,
                Status = JobDetailsStatus.Failed,
            });
            _progressService.Increase();
            Interlocked.Increment(ref _failedCount);
        }

        public void AddFailedJobDetails(List<XFileInfo> files)
        {
            files.ForEach(file => AddFailedJobDetail(file));
            Interlocked.Increment(ref _failedCount);
        }

        public void AddFailedJobDetail(XFileInfo file)
        {
            var fullPath = ExternalUtil.CombinePath(_executionInfo.ConnectionPath, file.HighName, file.LowName);
            _reportService.Commit(new FSDataSyncJobReportDetail
            {
                AgentName = OSInformation.HostName,
                ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(fullPath),
                FullPath = fullPath,
                Status = JobDetailsStatus.Failed,
            });
            _progressService.Increase();
            Interlocked.Increment(ref _failedCount);
        }

        public void AddCannotAccessJobDetail()
        {
            _reportService.Commit(new FSDataSyncJobReportDetail
            {
                AgentName = OSInformation.HostName,
                ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(_executionInfo.DirectoryFullPath),
                FullPath = _executionInfo.DirectoryFullPath,
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JS_JMD_FS_PathCanNotAccess"
            });
            Interlocked.Increment(ref _failedCount);
        }

        public void NotfiyJobStatus()
        {
            _logger.Info($"[JobProgress] Job {_jobId} progress: SucceedCount={_succeedCount}, FailedCount={_failedCount}");

            try
            {
                JobContext.Current.Cleanup();
            }
            catch(Exception e)
            {
                _logger.Error($"[JobProgress] Job {_jobId} cleanup failed: {e.Message}", e);
                Interlocked.Increment(ref _failedCount);
            }


            var status = JobStatus.Finished;
            if (_succeedCount > 0 && _failedCount > 0)
            {
                status = JobStatus.FinishWithException;
            }
            else if (_failedCount > 0)
            {
                status = JobStatus.Failed;
            }

            _logger.Info($"[JobProgress] Job {_jobId} final status: {status}");
            JobContext.Current.JobSummaryService.NotifyManager((int)status, JobContext.Current.JobId);
        }
    }
}
