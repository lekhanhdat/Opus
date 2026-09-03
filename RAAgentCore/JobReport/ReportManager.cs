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
using AvePoint.Hybrid.Utility.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AvePoint.RA.FileSystem.Core
{

    public class ReportManager<T> : IReportManager<T>
    {
        private AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly object locker = new object();
        private static bool jobFinished = false;
        private List<IReportService<T>> allServices = new List<IReportService<T>>();
        private MemoryListCacheService<T> cachedDetails;
        private Action<IEnumerable<T>> action;
        private AutoResetEvent exitEvent1;

        public ReportManager(Action<IEnumerable<T>> action)
        {
            exitEvent1 = new AutoResetEvent(false);
            cachedDetails = new MemoryListCacheService<T>();
            this.action = action;
        }
        public IReportService<T> Create()
        {
            IReportService<T> reportService = new ReportService<T>();
            lock (locker)
            {
                allServices.Add(reportService);
            }
            return reportService;
        }

        public void NotifyManager()
        {
            try
            {
                int batchCount = 1000;
                allServices.ForEach(service => cachedDetails.AddBatch(service.TakeBatch()));
                if (cachedDetails.Count >= batchCount || jobFinished)
                {
                    var tempDetails = cachedDetails.Take(batchCount).ToList();
                    action(tempDetails);
                }
            }
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
            }
            finally
            {
                if (jobFinished && cachedDetails.Count == 0)
                {
                    exitEvent1.Set();
                }
            }

        }
        public void FinalNotifyManager()
        {
            try
            {
                jobFinished = true;
                allServices.ForEach(service => cachedDetails.AddBatch(service.TakeBatch()));
                log.Debug("Final report entries count: {0}", cachedDetails.Count);
                if (cachedDetails.Count > 0)
                {
                    int batchCount = 1000;
                    while (cachedDetails.Count > 0)
                    {
                        var tempDetails = cachedDetails.Take(batchCount).ToList();
                        if (tempDetails.Count == 0) break;
                        action(tempDetails);
                        log.Debug("Final batch report entries count: {0}", tempDetails.Count);
                    }
                }
                WaitCompleted();
            }
            catch (Exception ex)
            {
                log.Debug(ex.ToString());
            }
        }
        private void WaitCompleted()
        {
            while (true)
            {
                if (exitEvent1.WaitOne(3 * 1000))
                {
                    break;
                }
                log.Info("wait job completed...");
            }
        }
    }
}
