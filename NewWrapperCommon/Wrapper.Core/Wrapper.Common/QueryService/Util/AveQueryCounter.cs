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
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AvePoint.Wrapper.Common
{
    internal class AveQueryCounter
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveQueryCounter));
        private static Dictionary<Guid, StackTrace> queryDic = new Dictionary<Guid, StackTrace>();
        private static object thisLock = new object();
        private StackTrace stackTrace;

        public void AddConnectionRecord(Guid guid)
        {
            lock (thisLock)
            {
                if (WrapperConfiguration.EnableStackInfo)
                {
                    stackTrace = new StackTrace(true);
                }
                PostStackInfo();
                queryDic.Add(guid, stackTrace);
            }
        }

        public void RemoveConnectionRecord(Guid guid)
        {
            lock (thisLock)
            {
                queryDic.Remove(guid);
            }
        }

        private void PostStackInfo()
        {
            if (queryDic.Count >= WrapperConfiguration.MaxConnectionCount)
            {
                if (WrapperConfiguration.EnableStackInfo)
                {
                    int num = 1;
                    foreach (StackTrace st in queryDic.Values)
                    {
                        for (int i = 0; i < st.FrameCount; i++)
                        {
                            StackFrame sf = st.GetFrame(i);
                            mLog.Info("Stack Info:{0}, {1}", num, sf.GetMethod());
                        }
                        num++;
                    }
                }
                else
                {
                    mLog.Warn("Current total connection count {0} has exceeded the Max count:{1}", queryDic.Count, WrapperConfiguration.MaxConnectionCount);
                }
            }
        }
    }
}
