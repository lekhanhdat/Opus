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
using System.Threading;
using Amib.Threading;
using AvePoint.GCommon.Utility;

namespace AvePoint.GCommon.Utility
{
    public class ThreadPoolHelper<T> where T : class
    {
        private int TaskCount;
        private ManualResetEvent _doneEvent { get; set; }
        private Action<T> Action { get; set; }
        private List<T> List { get; set; }
        public List<T> TempList { get; private set; }
        private HttpContext Context { get; set; }
        public  List<T> ResultList { get; private set; }
        private readonly object lockObj = new object();
        private System.Func<T, bool> IsAdd2ResultList { get; set; }
        private AveLogger logger = AveLogger.GetInstance(typeof(ThreadPoolHelper<T>));

        public static List<T> RunAsParallel(Action<T> action, ref List<T> list, HttpContext context, System.Func<T, bool> isAdd2ResultList = null) 
        {
            ThreadPoolHelper<T> pool = new ThreadPoolHelper<T>(action, ref list, context, isAdd2ResultList);
            pool.Start(0);
            list = pool.TempList;
            return pool.ResultList;
        }

        private ThreadPoolHelper(Action<T> action, ref List<T> list, HttpContext context, System.Func<T, bool> isAdd2ResultList = null)
	    {
            this.TaskCount = list.Count();
            this._doneEvent = new ManualResetEvent(false);
            this.Action = action;
            this.List = list;
            this.Context = context;
            this.ResultList = new List<T>();
            this.IsAdd2ResultList = isAdd2ResultList;
	    }

        private void Start(int retryTime)
        {
            if (this.List == null || this.List.Count() == 0)
            {
                return;
            }
            Dictionary<IWorkItemResult, T> parallelRecords = new Dictionary<IWorkItemResult, T>();
            SmartThreadPool smartThreadPool = new SmartThreadPool() { MaxThreads = 2,};
            foreach (var item in this.List)
	        {
                IWorkItemResult wir = smartThreadPool.QueueWorkItem<T>(DoWork, item);
                parallelRecords.Add(wir, item);
	        }
            smartThreadPool.WaitForIdle();
            this.TempList = new List<T>();
            this.TempList.AddRange(this.List);
            this.List = new List<T>();
            foreach (var result in parallelRecords.Keys)
            {
                Exception e = null;
                result.GetResult(out e);
                if (e != null)
                {
                    logger.Error("An error occurred when doing work as parallel. Retry time is {0}. The record count is {1}.", retryTime, parallelRecords.Keys.Count, e.ToString());
                    this.List.Add(parallelRecords[result]);
                }
            }
            if (this.List.Count > 0 && retryTime == 0)
            {
                Random rd = new Random();
                int waitSeconds = 60 + rd.Next(1, 12) * 5;
                Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));
                Start(retryTime + 1);
            }
            smartThreadPool.Shutdown(true, TimeSpan.FromSeconds(30));
        }

        private void DoWork(object arg)
        {
            HttpContextHelper.Current = this.Context;
            if (this.Action != null)
            {
                this.Action(arg as T);
            }
                
            if (this.IsAdd2ResultList != null && this.IsAdd2ResultList(arg as T))
            {
                lock(this.lockObj)
                {
                    this.ResultList.Add(arg as T);
                }
            }
        }

    }
}
