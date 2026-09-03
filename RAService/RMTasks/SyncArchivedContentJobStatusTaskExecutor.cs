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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class SyncArchivedContentJobStatusTaskExecutor: ITaskExecutor
    {
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private ICommonService CommonService;

        public IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();


        private IRALogger mLogger = RALogger.GetInstance(typeof(SyncArchivedContentJobStatusTaskExecutor));

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                CommonService = (ICommonService)PlatformWindsorManager.GetService(typeof(ICommonService));
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    TenantUtil.RunUnderTenant(tInfo.TenantId, tInfo.RegisterEmail, ExcuteTask);
                }

            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while update disposal job status,ERROR:{0}", ex.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
        private void ExcuteTask()
        {
            try
            {
                var downloadJobs = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait, (int)DownloadContentJobStatus.InProgress });
                var archiveJobs = downloadJobs.Where(job => job.DownloadType == DownloadContentType.ArchivedContent).ToList();
                mLogger.Info($"Get {archiveJobs?.Count} download archived content jobs.");
                if (archiveJobs != null && archiveJobs.Count > 0)
                {
                    var jobs = RMJobService.GetSOJobsByIds(archiveJobs.Select(j => j.JobId).ToList());
                    mLogger.Info($"Get {jobs?.Count} download archived content jobs from dao.");
                    var jobStatusMapping = jobs.ToDictionary(k => k.Id, v => v.State);
                    foreach (var job in archiveJobs)
                    {
                        if (jobStatusMapping.ContainsKey(job.JobId))
                        {
                            job.JobStatus = jobStatusMapping[job.JobId];
                        }
                    }
                    DownloadDataInfoDao.BatchUpdate(archiveJobs);
                    mLogger.Info("Update download archived content job status finished.");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Error occurred while excute download arcived content job task,ERROR:{0}", ex.ToString());
            }
        }
    }
}
