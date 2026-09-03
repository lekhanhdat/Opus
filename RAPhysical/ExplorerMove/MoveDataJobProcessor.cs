using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RAPhysical.Loan;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.ExplorerMove
{
    public class MoveDataJobProcessor
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(LoanBoxJobProcessor));
        #region interface
        private IJobInfoUpdater _jobInfoUpdater;
        protected IJobInfoUpdater JobInfoUpdater
        {
            get
            {
                if (_jobInfoUpdater == null)
                {
                    _jobInfoUpdater = (IJobInfoUpdater)PlatformWindsorManager.GetService(typeof(IJobInfoUpdater));
                }
                return _jobInfoUpdater;
            }
        }

        protected IRMReportManager mReportManager;
        protected IRMReportManager ReportManager
        {
            get
            {
                if (mReportManager == null)
                {
                    mReportManager = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManager;
            }
        }

        private IRecordLoanAllianceDao mRecordLoanAllianceDao;
        public IRecordLoanAllianceDao RecordLoanAllianceDao
        {
            get
            {
                if (mRecordLoanAllianceDao == null)
                {
                    mRecordLoanAllianceDao = (IRecordLoanAllianceDao)PlatformWindsorManager.GetService(typeof(IRecordLoanAllianceDao));
                }
                return mRecordLoanAllianceDao;
            }
        }

        private IRMSubJobDao mSubJobDao;
        public IRMSubJobDao SubJobDao
        {
            get
            {
                if (mSubJobDao == null)
                {
                    mSubJobDao = (IRMSubJobDao)PlatformWindsorManager.GetService(typeof(IRMSubJobDao));
                }
                return mSubJobDao;
            }
        }

        RMPhysicalExplorerMoveUtility RMPhysicalExplorer;
        #endregion
        private string mJobId = string.Empty;

        public MoveDataJobProcessor(string jobId)
        {
            mJobId = jobId;
            ReportMangerFactory.Instance.Init(mJobId, JobType.PhysicalMoveDataJob, true);
            JobInfoUpdater.UpdateJobState(mJobId, (int)JobStatus.InProgress);
            ReportManager.StartUpdateJobProgress();
            RMPhysicalExplorer = new RMPhysicalExplorerMoveUtility(isMoveRequestApprovalJob: true);

        }
        public async Task RunAsync()
        {
            logger.Info("Start to run move data job.");
            RMSubJob subJobWithContext = SubJobDao.GetSubJob(mJobId, true);
            logger.Info("Get job message:{0}", subJobWithContext.JobContext.Content);
            var jobParam = SerializerHelper.DeserializeByDataContractSerializer<List<PhysicalMoveRequest>>(subJobWithContext.JobContext.Content);
            bool hasSuccess = false;
            bool hasFailed = false;
            using (PerformanceScope pc0 = new PerformanceScope("RunAsync.Move", addToStatistics: true))
            {
                foreach (var request in jobParam)
                {
                    logger.Info($"Start to run move data job. groupRequestId {request.GroupRequestId}");
                    await RMPhysicalExplorer.MoveAsync(request.PhysicalMoveOption, string.Empty, groupRequestId: request.GroupRequestId);
                    hasSuccess |= RMPhysicalExplorer.HasSuccessNode;
                    hasFailed |= RMPhysicalExplorer.HasFailedNode;
                }
            }
            var jobStatus = GetJobStatus(hasSuccess, hasFailed);
            ReportManager.SetJobFinished(jobStatus);

        }
        private JobStatus GetJobStatus(bool hasSuccess, bool hasFailed)
        {
            if (hasSuccess && hasFailed) return JobStatus.FinishWithException;

            if (hasFailed) return JobStatus.Failed;

            return JobStatus.Finished;
        }
    }
}
