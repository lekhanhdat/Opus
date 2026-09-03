/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;

namespace RAGoogle.Restore
{
    public class JobContext
    {
        public DateTime JobStartTime { private set; get; } = DateTime.MinValue;
        public string JobContextSetting { private set; get; }
        public string JobContextContent { private set; get; }
        public IRMReportManager ReportManager { get; set; }

        public long MainJobStartTime { get; private set; }
        private static MonitorExceptionType MonitorException { get; set; } = MonitorExceptionType.None;
        public string SubJobId { private set; get; } = string.Empty;
        public string MainJobId { private set; get; } = string.Empty;

        //當前Site是否有Error
        public bool NodeLevelError { get; set; }
        public bool HasSuccessNode { get; set; }
        public bool HasErrorNode { get; set; }
        public bool JobHasStopped { get; set; }
        public static bool IsCSDTenant { get; private set; }
        private ITenantInfoDao mTenantInfoDao { get; set; }
        protected ITenantInfoDao TenantInfoDao
        {
            get
            {
                if (mTenantInfoDao == null)
                {
                    mTenantInfoDao = AvePoint.RA.Common.PlatformWindsorManager.GetService(typeof(ITenantInfoDao)) as ITenantInfoDao;
                }
                return mTenantInfoDao;
            }
        }

        private readonly static Dictionary<string, JobContext> _jobManager = new Dictionary<string, JobContext>();
        public static JobContext GetInstance(string jobId, JobType jobType)
        {
            if (!_jobManager.TryGetValue(jobId, out var jobMgr))
            {
                lock (_jobManager)
                {
                    if (!_jobManager.TryGetValue(jobId, out jobMgr))
                    {
                        jobMgr = new JobContext(jobId, jobType);
                        _jobManager[jobId] = jobMgr;
                    }
                }
            }
            return jobMgr;
        }

        private List<JobType> needReportJob = new List<JobType>()
        {
            //EXO
            JobType.EXOCreateAndDestroyedFileReport,
            JobType.EXOItemsFilesDueDisposalReport,
            JobType.EXOTermUsageReport,
            JobType.EXOOrphanedTermUsageReport,
            JobType.EXORetiredTermUsageReport,
            //SP
            JobType.CreateAndDestroyedFileReport,
            JobType.ItemsFilesDueDisposal,
            JobType.BCSTermUsageReport,
            JobType.RetiredTermReport,
            JobType.OrphanedTermReport,
            JobType.SPOActionAuditReport,
            JobType.OneDriveActionAuditReport,
            //Physical
            JobType.PhysicalCreateAndDestroyedFileReport,
            JobType.PhysicalItemsFilesDueDisposalReport,
            JobType.PhysicalOrphanedTermUsageReport,
            JobType.PhysicalRetiredTermUsageReport,
            JobType.PhysicalTermUsageReport,
            JobType.AvailableSpaceReport,
            //FS
            JobType.FSBCSTermUsageReport,
            JobType.FSOrphanedTermReport,
            JobType.FSRetiredTermReport,
            JobType.FSItemsFilesDueDisposal,
            JobType.FSCreateAndDestroyedFileReport,
            //onedrive             
            JobType.OneDriveItemsFilesDueDisposalReport,
            JobType.OneDriveTermUsageReport,
            JobType.OneDriveOrphanedTermUsageReport,
            JobType.OneDriveRetiredTermUsageReport,
            JobType.OneDriveCreateAndDestroyedFileReport,
            //SPOnPrem
            JobType.SPOnPremCreateAndDestroyedFileReport,
            JobType.SPOnPremItemsFilesDueDisposal,
            JobType.SPOnPremBCSTermUsageReport,
            JobType.SPOnPremRetiredTermReport,
            JobType.SPOnPremOrphanedTermReport,
            //Box
            JobType.BoxItemsFilesDueDisposalReport,
            JobType.BoxBCSTermUsageReport,
            JobType.BoxOrphanedTermUsageReport,
            JobType.BoxRetiredTermUsageReport,
            JobType.BoxCreateAndDestroyedFileReport,
            //Google
            JobType.GoogleCreateAndDestroyedFileReport,
            JobType.GoogleItemsFilesDueDisposalReport,
            JobType.GoogleBCSTermUsageReport,
            JobType.GoogleOrphanedTermUsageReport,
            JobType.GoogleRetiredTermUsageReport,
            //Teams
            JobType.TeamsCreateAndDestroyedFileReport,
            JobType.TeamsItemsFilesDueDisposalReport,
            JobType.TeamsBCSTermUsageReport,
            JobType.TeamsOrphanedTermUsageReport,
            JobType.TeamsRetiredTermUsageReport,
        };

        private readonly List<JobType> onlyStorageInSubJobTable = new List<JobType>()
        {
            JobType.SimulateRestore
        };

        private JobContext(string jobId, JobType jobType)
        {
            JobStartTime = DateTime.UtcNow;
            SubJobId = jobId;
            var needReport = needReportJob.Contains(jobType);
            ReportMangerFactory.Instance.Init(jobId, jobType, needReport);
            ReportManager = ReportMangerFactory.Instance.ReportManager;
            if (AvePoint.RA.Common.JobService.JobServiceUtility.IsSubJob(jobId))
            {
                IRMSubJobDao SubJobDao = new RMSubJobDao();
                IJobMonitorDao JobMonitorDao = new JobMonitorDao();
                //从子job的Context中获取当前需要处理的节点.
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(jobId, true);
                if (!onlyStorageInSubJobTable.Contains(jobType))
                {
                    MainJobId = subJobWithContext.ParentId;
                    MainJobStartTime = JobMonitorDao.GetJob(subJobWithContext.ParentId).StartTime;
                }

                JobContextSetting = subJobWithContext.JobContext?.Settings;
                JobContextContent = subJobWithContext.JobContext?.Content;
                IsCSDTenant = TenantInfoDao.IsEnableCSD(TenantLocalValue.LogonGroupId);
            }

        }
        public async Task MonitorExcetionAsync(MonitorExceptionType exceptionType)
        {
            if (!string.IsNullOrEmpty(MainJobId) && !MonitorException.HasFlag(exceptionType))
            {
                await ReportManager.MonitorExceptionAsync(MainJobId, exceptionType);
            }
        }

        public void Finish(string comment = "")
        {

            var jobStatus = HasSuccessNode && HasErrorNode ?
                JobStatus.FinishWithException :
                (
                    HasErrorNode ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            if (JobHasStopped)
            {
                jobStatus = JobStatus.Stopped;
            }
            ReportManager.SetJobFinished(jobStatus, comment);
        }
    }
}
