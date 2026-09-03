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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    public class AveMultiReceiver:IDisposable
    {
        public delegate AveMultiReceiveTask FileHeadReceivedEvent(string fileHead);

        private AveMultiTaskTree taskTree;
        public AveTaskHierarchyScheduler scheduler;
        private AveMultiTaskThreadPool pool;
        private Byte[] innerBuffer;
        private bool closeReceiver;

        public AveMultiReceiver(int threadCount, bool closeReceiver = true)
        {
            this.taskTree = new AveMultiTaskTree(threadCount * 2);
            this.pool = new AveMultiTaskThreadPool(threadCount);
            this.scheduler = new AveTaskHierarchyScheduler(taskTree, pool);
            this.innerBuffer = new Byte[65536];
            this.closeReceiver = closeReceiver;
        }

        public FileHeadReceivedEvent OnFileHeadReceived { get; set; }

        public Action<int> OnTaskPoolFull { get; set; }

        public Action OnDataReceiveEnd { get; set; }

        public Action<string> OnDataReceiveException { get; set; }

        public void Wait()
        {
            this.scheduler.Wait();
        }

        public void Dispose()
        {
            if (scheduler != null)
            {
                scheduler.Dispose();
            }
            if (pool != null)
            {
                pool.Dispose();
            }
        }
    }
}
