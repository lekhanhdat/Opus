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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.RA.FileSystem.Core
{
    public class ProgressManager : IProgressManager
    {
        private long branch = 1;
        private readonly object locker = new object();
        private FSJobStatusDto jobStatusInfo = new FSJobStatusDto();
        private List<IProgressService> allService = new List<IProgressService>();
        private AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private DateTime LastUpdateTime = DateTime.Now;
        private int LastProgress = 1;

        public ProgressManager(string jobId)
        {
            jobStatusInfo.JobId = jobId;
            jobStatusInfo.Status = JobStatus.Finished;
        }

        public IProgressService Create()
        {
            var prog = new ProgressService();
            lock (locker)
            {
                allService.Add(prog);
            }
            return prog;
        }

        //you can use the default value
        public void IncreaseBranchTo(long value)
        {
            if (value > 0)
            {
                Interlocked.Exchange(ref branch, value);
            }
        }
        public int NotifyManagerAsync()
        {
            try
            {
                int progress = 0;
                if (branch != 0)
                {
                    long total = allService.Sum(p => p.Total);
                    long finished = allService.Sum(p => p.Finished);
                    progress = total == 0 ? 1 : (int)(finished * 100 / total);
                    log.Debug("The progress: progress:{0}, total:{1}, finished:{2}, branch:{3}", progress, total, finished, allService.Count);
                }
                if (LastUpdateTime.AddMinutes(1) < DateTime.Now)
                {
                    LastUpdateTime = DateTime.Now;
                    if (progress <= LastProgress)
                    {
                        //make sure it's not  decreased....
                        progress = LastProgress;
                    }
                    else
                    {
                        LastProgress = progress;
                    }
                    if (progress >= 100)
                    {
                        //make sure it's not over 100%
                        progress = 99;
                    }
                    jobStatusInfo.Progress = progress;
                    using (new AgentPerformanceScope("Progress Manager--Send msg to Control"))
                    {
                        JobContext.Current.ApiClient.UpdateJobProgress(new HBJobStatusInfo() { JobId = jobStatusInfo.JobId, Progress = jobStatusInfo.Progress });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Warn("send progress errror {0}", ex.ToString());
                LastUpdateTime = DateTime.Now;
            }

            return 0;
        }

        public async void NotifyManager()
        {
            int a = await Task.Run(new Func<int>(NotifyManagerAsync));
        }

        /// <summary>
        /// When the job is finished normally, you can call this one to set the progress to 100
        /// if it's failed. no need to call this method
        /// </summary>
        public void FinalNotifyManager()
        {
            try
            {
                jobStatusInfo.Progress = 100;
                JobContext.Current.ApiClient.UpdateJobProgress(new HBJobStatusInfo() { JobId = jobStatusInfo.JobId, Progress = jobStatusInfo.Progress });
            }
            catch (Exception ex)
            {
                //TODO hyw
                log.Debug(ex.ToString());
            }

        }
    }
}
