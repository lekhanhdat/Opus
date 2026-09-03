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
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.Utils
{
    public static class StartJobWatcher
    {
        private static AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ConcurrentDictionary<string, StartedJobHandler> StartedJobs = new ConcurrentDictionary<string, StartedJobHandler>();

        public static void Add(string jobId)
        {
            StartedJobs.TryAdd(jobId, new StartedJobHandler(600, StartedJobs, jobId));
            logger.Info("Add job to watcher, jobid:{0}", jobId);
        }

        public static bool Exists(string jobId)
        {
            if (StartedJobs.ContainsKey(jobId))
            {
                return true;
            }
            return false;
        }
    }



    public class StartedJobHandler
    {
        private static AvePoint.GCommon.AveLogger logger = AvePoint.GCommon.AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly CancellationTokenSource CancelToken;
        private readonly ConcurrentDictionary<string, StartedJobHandler> StartedJobs;
        private readonly string JobId;
        public StartedJobHandler(int timeout, ConcurrentDictionary<string, StartedJobHandler> startedJobs, string jobId)
        {
            this.JobId = jobId;
            this.StartedJobs = startedJobs;
            if (timeout != 0)
            {
                this.CancelToken = new CancellationTokenSource(timeout * 1000);
                this.CancelToken.Token.Register(() =>
                {
                    bool removed = StartedJobs.TryRemove(this.JobId, out StartedJobHandler handler);
                    logger.Info($"Reach pending time, job id:{this.JobId}, has removed from dic:{removed}");
                });
            }
        }
    }
}
