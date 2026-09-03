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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using RAFileSystem.Disposal.Archive;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.FileSystem.Restore
{
    public class FSRestoreMain : IScheduleJobWorker
    {
        public RestoreInfo restoreInfo { get; set; }
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public void Bind(string msg)
        {
            restoreInfo = SerializerHelper.DeserializeByJsonSerializer<RestoreInfo>(msg);
            JobContext.Current.mProgressManager.Create().IncreaseBase(3);
        }

        public void Run()
        {
            try
            {
                using (var pc1 = new AgentPerformanceScope("FSRestore.TotalRestore", addToStatistics: true))
                {
                    FSRestoreWorker analyzer = new FSRestoreWorker(restoreInfo);
                    analyzer.RunRestoreJob();
                }
            }
            catch (Exception ex)
            {
                logger.Error("FSRestoreMain Run error", ex);
            }
            finally
            {
                try
                {
                    logger.Info("start delete temp restore cache");
                    if (Directory.Exists(FSJobCache.RestoreInstance.FSRestoreCachePath))
                    {
                        Directory.Delete(FSJobCache.RestoreInstance.FSRestoreCachePath, true);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("delete temp restore cache error", ex);
                }
                try
                {
                    JobContext.Current.Cleanup();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while restore cleaning up. Error:" + e.ToString());
                    FSJobCache.RestoreInstance.FailedCount++;
                }
                if (FSJobCache.RestoreInstance.FailedCount > 0)
                {
                    if (FSJobCache.RestoreInstance.SuccessCount > 0)
                    {
                        JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.FinishWithException, JobContext.Current.JobId);
                    }
                    else
                    {
                        JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Failed, JobContext.Current.JobId);
                    }
                }
                else
                {
                    JobContext.Current.JobSummaryService.NotifyManager((int)JobStatus.Finished, JobContext.Current.JobId);
                }
                logger.Info("Enforce restore job finished.");
            }
        }
    }
}
