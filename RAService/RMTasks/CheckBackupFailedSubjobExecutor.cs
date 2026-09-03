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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class CheckBackupFailedSubjobExecutor : ITaskExecutor
    {
        private RALogger mLogger = RALogger.GetInstance(typeof(CheckBackupFailedSubjobExecutor));
        public ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        public Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    TenantUtil.RunUnderTenant(tInfo.TenantId, tInfo.RegisterEmail, CheckAndUpdateFailedBackupSubjobInfo);
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"something went wrong when update size to AOS ,error:{e.ToString()}");
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
        public void CheckAndUpdateFailedBackupSubjobInfo()
        {
            try
            {
                IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
                IEXOArchiverIndexSubInfoDao EXOArhciverSubInfo = PlatformWindsorManager.GetService<IEXOArchiverIndexSubInfoDao>();
                IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
                var failedJobids = ArchiverIndexSubInfoDao.GetAllBackupOrMergeIndexFailedSubJobIds();
                var exoFailedJobids = EXOArhciverSubInfo.GetAllBackupOrMergeIndexFailedEXOSubJobIds();
                failedJobids.AddRange(exoFailedJobids);
                mLogger.Info($"get failed job ids from sub info finish,count:{failedJobids?.Count}");
                var notCheckedSubjobs = SubJobDao.GetNotCheckedFailedSubJobs();
                mLogger.Info($"get not checked sub jobs finish,count:{notCheckedSubjobs?.Count}");
                foreach (var subjob in notCheckedSubjobs)
                {
                    mLogger.Info($"CheckAndUpdateFailedBackupSubjobInfo check sub job id:{subjob.Id}");
                    if (failedJobids.Contains(subjob.Id))
                    {
                        mLogger.Info($"CheckAndUpdateFailedBackupSubjobInfo check sub job id:{subjob.Id},it fit");
                        subjob.HasCheckedBackupFailed = (int)HasCheckedBackupStatus.CheckedFit;
                    }
                    else
                    {
                        mLogger.Info($"CheckAndUpdateFailedBackupSubjobInfo check sub job id:{subjob.Id},it not fit");
                        subjob.HasCheckedBackupFailed = (int)HasCheckedBackupStatus.CheckedNotFit;
                    }
                }
                BatchUpdate(notCheckedSubjobs, HasCheckedBackupStatus.CheckedFit, SubJobDao);
                BatchUpdate(notCheckedSubjobs, HasCheckedBackupStatus.CheckedNotFit, SubJobDao);
                mLogger.Info($"CheckAndUpdateFailedBackupSubjobInfo update sub jobs finish");
            }
            catch (Exception e)
            {
                mLogger.Error($"something went wrong when check backup failed sub job ,error:{e}");
            }
        }

        public void BatchUpdate(IEnumerable<RMSubJob> notCheckedSubjobs, HasCheckedBackupStatus status, IRMSubJobDao subJobDao)
        {
            var failedJobs = notCheckedSubjobs.Where(job => job.HasCheckedBackupFailed == (int)status);
            foreach (var batch in failedJobs.Batch(500))
            {
                subJobDao.BatchUpdateBackupFailedStatusByIds(batch.Select(job => job.Id), status);
            }
        }
    }
}