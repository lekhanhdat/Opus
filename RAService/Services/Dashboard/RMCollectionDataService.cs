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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.CommonUtil;
using System.Reflection;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Service.Services.Dashboard.AuditHandler;
using AvePoint.RA.Common;

namespace AvePoint.RA.Service.Services.Dashboard
{
    [Audit]
    public class RMCollectionDataService: RMServiceBase, IRMCollectionDataService
    {
        protected RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();


        public string RunScheduleJob(JobRunBy jobRunBy, JobType jobType)
        {
            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
                //DB.Explorer.Dao.CosmosImp.ExplorerDao eDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao();
                //int count = eDao.QueryCount(string.Format("SELECT VALUE COUNT(1) FROM c where c.scopeId = \"{0}\"", "028c1511-61e8-4b61-aacf-df51b2366cc1"));
                //Logger.Info("query count {0}", count);
            }
            catch (Exception ex)
            {
                Logger.Error("error occurred while RunUniqueIDSettingsScheduleJob,ERROR:{0}", ex.ToString());
            }
            return id;
        }
        [Audit(Module = AuditModule.ReportCenter, Category = AuditCategory.DashboardCollectionDataJob, Action = AuditAction.DashboardCollectionDataJob, AfterHandler = typeof(CollectionDataAfterAuditHandler))]
        public string RealRunJob(JobRunBy jobRunBy, JobType jobType, string jobRunByUser = "")
        {
            string jobId = string.Empty;
            jobId = RMJobService.CreateJob(jobType, string.IsNullOrEmpty(jobRunByUser) ? "RM_TS_RunSchedule" : jobRunByUser);

            List<string> runningJobs = RMJobService.GetCollectionDataSettingJobs();

            bool isSkip = runningJobs.Any(j => j != jobId);
            if (!isSkip)
            {
                StartJob(jobType, jobId, jobRunBy);
            }
            else
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_UID_JobSkip");
                Logger.Info("unidsetting job has job running,so shedule job is skip");
            }
            return jobId;
        }

        public void StartJob(JobType jobType, string jobId, JobRunBy runBy)
        {
            mJobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = jobId,
                JobType = jobType,
                RunBy = runBy,
                CommandLine = string.Format("{0} {1}", jobType, jobId),
            });
        }

    }
}
