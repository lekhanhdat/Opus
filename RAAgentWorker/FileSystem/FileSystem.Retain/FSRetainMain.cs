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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.Storage.Util;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using log4net;
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.Contract.RMWeb.JobMonitor;

namespace RAFileSystem.FileSystem.FileSystem.Retain
{
    public class FSRetainMain : IScheduleJobWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private List<ArchiverPruningJob> archiverPruningJobs { get; set; }
        private string rehydrationTemp;
        private bool destinationStoreInArchiverTier;
        public void Bind(string msg)
        {
            archiverPruningJobs = SerializerHelper.DeserializeByJsonSerializer<List<ArchiverPruningJob>>(msg);
            JobContext.Current.mProgressManager.Create().IncreaseBase(3);
        }

        public void Run()
        {
            try
            {
                using (var pc1 = new AgentPerformanceScope("FSRetain.TotalRetainTime", addToStatistics: true))
                {
                    logger.Info("Start enforce retain job.");
                    FSRetainWorker fSRetainWorker = new FSRetainWorker(archiverPruningJobs);
                    fSRetainWorker.RunRetainJob();
                    logger.Info("finish retain job.");
                }
            }
            finally
            {
                try
                {
                    logger.Info("start delete temp retain cache");
                    if (System.IO.Directory.Exists(BackgroundSettings.GetInstance().ArchiveTemp))
                    {
                        System.IO.Directory.Delete(BackgroundSettings.GetInstance().ArchiveTemp, true);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("delete temp retain cache error", ex);
                }
                try
                {
                    JobContext.Current.Cleanup();
                }
                catch (Exception e)
                {
                    logger.Error("An error occurred while retain cleaning up. Error:" + e.ToString());
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
                logger.Info("Enforce retain job finished.");
            }
        }
        
    }
}
