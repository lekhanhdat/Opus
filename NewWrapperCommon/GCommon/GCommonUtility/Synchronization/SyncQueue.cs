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



namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    #endregion
    /// <summary>
    /// 同步先入先出队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SyncQueue<T>
    {
        private List<T> mInteralList;
        private int mSize;
        private int mWaitSeconds;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size">队列大小，超过这个大小时候，队列会阻塞，-1为不限制大小</param>
        /// <param name="waitSeconds">阻塞等待时间，-1为无限等待</param>
        public SyncQueue(int size = -1, int waitSeconds = -1)
        {
            mSize = size;
            mWaitSeconds = waitSeconds * 1000;
            mInteralList = new List<T>(size);
        }

        /// <summary>
        /// 向队列尾加入一个元素，如果队列超过指定大小，则同步等待
        /// </summary>
        /// <param name="t"></param>
        public void Put(T t)
        {
            lock (mInteralList)
            {
                if (mSize > 0 && mInteralList.Count > mSize)
                {
                    if (mWaitSeconds > 0)
                    {
                        if (!Monitor.Wait(mInteralList, mWaitSeconds))
                        {
                            throw new Exception("Put item wait time out");
                        }
                    }
                    else
                    {
                        if (!Monitor.Wait(mInteralList))
                        {
                            throw new Exception("Put item unknown exception");
                        }
                    }
                }
                mInteralList.Add(t);
                Monitor.PulseAll(mInteralList);
            }
        }

        /// <summary>
        /// 从队列头取出第一个元素，如果队列为空，则同步等待
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T t = default(T);
            lock (mInteralList)
            {
                if (mInteralList.Count > 0)
                {
                    t = mInteralList[0];
                    mInteralList.RemoveAt(0);
                }
                else
                {
                    if (mWaitSeconds > 0)
                    {
                        if (!Monitor.Wait(mInteralList, mWaitSeconds))
                        {
                            throw new Exception("Get item wait time out");
                        }
                    }
                    else
                    {
                        if (!Monitor.Wait(mInteralList))
                        {
                            throw new Exception("Get item unknown exception");
                        }
                    }
                    t = mInteralList[0];
                    mInteralList.RemoveAt(0);
                }
                Monitor.PulseAll(mInteralList);
            }
            return t;
        }
    }
}
