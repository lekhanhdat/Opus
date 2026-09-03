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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
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
    public class DeleteExpiredArchivedContentTaskExecutor : ITaskExecutor
    {
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IArchivedContentDownloadService ArchivedContentDownloadService => PlatformWindsorManager.GetService<IArchivedContentDownloadService>();
        public IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        public IJobMonitorDao JobMonitorDao => PlatformWindsorManager.GetService<IJobMonitorDao>();

        private IRALogger mLogger = RALogger.GetInstance(typeof(DeleteExpiredArchivedContentTaskExecutor));

        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
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
                //保留一周以内下载的，最多100个
                var expiredContents = DownloadDataInfoDao.GetDownloadDataInfoByRetentionTime(DateTime.UtcNow.AddDays(-7).Ticks);
                mLogger.Info($"Get {expiredContents?.Count} expired archived content.");
                if (expiredContents != null && expiredContents.Count > 0)
                {
                    List<RMDownloadDataInfo> deletedInfos = new List<RMDownloadDataInfo>();
                    foreach (var content in expiredContents)
                    {
                        try
                        {
                            if (content.JobStatus is (int)DownloadContentJobStatus.InProgress or (int)DownloadContentJobStatus.Stopping or (int)DownloadContentJobStatus.Wait)
                            {
                                var job = JobMonitorDao.GetJob(content.JobId);
                                if (job != null)
                                {
                                    if (job.Status is (int)JobStatus.InProgress or (int)JobStatus.Stopping or (int)JobStatus.Wait)
                                    {
                                        mLogger.Warn($"Download content are expired, but the job is in progress, we will skip the deletion. content name: {content.Name}, job id: {content.JobId}");
                                        continue;
                                    }
                                    else
                                    {
                                        mLogger.Warn($"Download content are expired, although content is running, but the job ends. we will perform the deletion. content name: {content.Name}, job id: {content.JobId}");
                                    }
                                }
                                else
                                {
                                    mLogger.Warn($"Download content are expired, but can not find the job, we will skip the deletion. content name: {content.Name}, job id: {content.JobId}");
                                    continue;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            mLogger.Info($"Get job status error, job id {content.JobId}, error:{e}");
                        }
                        try
                        {
                            //if (content.JobStatus == (int)DownloadContentJobStatus.Finished)
                            {
                                ArchivedContentDownloadService.DeleteExpiredData(content.JobId);
                            }
                            deletedInfos.Add(content);
                        }
                        catch (Exception e)
                        {
                            mLogger.Warn($"Error occurred while deleting expired archived content. JobId:{content?.JobId} Error:{e.ToString()}");
                        }
                    }
                    if (deletedInfos.Count > 0)
                    {
                        DownloadDataInfoDao.BatchDelete(deletedInfos);
                    }
                    mLogger.Info("Delete expired archived content finished.");
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Error occurred while excute delete expired content task,ERROR:{0}", ex.ToString());
            }
            try
            {
                //keep 30 days 以内下载的，最多100个
                var expiredContents = DownloadDataInfoDao.GetZipPasswordInfoByRetentionTime(DateTime.UtcNow.AddDays(-30).Ticks);
                mLogger.Info($"Get {expiredContents?.Count} expired archived zippassword content.");
                DownloadDataInfoDao.BatchDelete(expiredContents);
            }
            catch(Exception ex)
            {
                mLogger.Error("Error occurred while excute delete zippassword expired content task,ERROR:{0}", ex.ToString());
            }
        }
    }
}
