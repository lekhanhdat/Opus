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




namespace AvePoint.Media.Core.Index
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using AvePoint.RA.CommonUtil;
    #endregion

    internal class ConnectionLock
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);

        List<ConnectionLockInfo> connectionLocks = new List<ConnectionLockInfo>();

        public bool Add(Guid id, ConnectionLockType lockType)
        {
            lock (connectionLocks)
            {
                connectionLocks.RemoveAll((lockInfo) =>
                {
                    if (lockInfo.OwnerThread.IsAlive == false)
                    {
                        logger.Warn($"ConnectionLock AddRemoveLock Of DeadThread:{lockInfo.ToString()}");
                        return true;
                    }
                    return false;
                });
                if (lockType == ConnectionLockType.Download || lockType == ConnectionLockType.Upload)
                {
                    if (connectionLocks.Count > 0)
                    {
                        logger.Info($"ConnectionLock Add CancelLock:{lockType}");
                        ShowCurrentLocks();
                        return false;
                    }
                }
                else if (lockType == ConnectionLockType.ReadWrite)
                {
                    while (connectionLocks.Count == 1
                        && (connectionLocks[0].Type == ConnectionLockType.Download || connectionLocks[0].Type == ConnectionLockType.Upload))
                    {
                        logger.Info($"ConnectionLock Add WaitingLock:{lockType.ToString()}");
                        ShowCurrentLocks();
                        Monitor.Wait(connectionLocks);
                        logger.Info($"ConnectionLock Add WaitingLockAwake:{lockType.ToString()}");
                        ShowCurrentLocks();
                    }
                }
                connectionLocks.Add(new ConnectionLockInfo { Type = lockType, ID = id, OwnerThread = Thread.CurrentThread });
                return true;
            }
        }

        public void Remove(Guid connectionID)
        {
            lock (connectionLocks)
            {
                connectionLocks.RemoveAll((lockInfo) => { return lockInfo.ID == connectionID; });
                Monitor.PulseAll(connectionLocks);
            }
        }

        private void ShowCurrentLocks()
        {
            StringBuilder sb = new StringBuilder();
            foreach (ConnectionLockInfo lockInfo in connectionLocks)
            {
                sb.Append("[");
                sb.Append(lockInfo.ToString());
                sb.Append("]");
            }
            logger.Info($"ConnectionLock ShowCurrentLocks AllLocks:{sb.ToString()}");
        }
        internal class ConnectionLockInfo
        {
            public ConnectionLockType Type { get; set; }
            public Guid ID { get; set; }
            public Thread OwnerThread { get; set; }

            public override String ToString()
            {
                return Type.ToString() + " " + ID + " " + OwnerThread.Name;
            }
        }
    }
}
