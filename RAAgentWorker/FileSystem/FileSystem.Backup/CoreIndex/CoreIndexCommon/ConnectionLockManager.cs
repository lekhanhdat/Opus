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
    using AvePoint.RA.CommonUtil;
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    #endregion

    public static class ConnectionLockManager
    {
        static Dictionary<String, ConnectionLock> allConnectionLocks = new Dictionary<String, ConnectionLock>();

        /// <summary>
        /// this method is used to acquire the correct connection lock
        /// </summary>
        /// <param name="connectionString">current database connection string</param>
        /// <param name="connectionID">current connection id</param>
        /// <param name="lockType">current connection lock type</param>
        /// <returns>false only happens on Sync lock type, means you don't need do the sync, others will do it. otherwise true</returns>
        public static bool GetConnectionLock(String connectionString, Guid connectionID, ConnectionLockType lockType)
        {
            ConnectionLock currentConnectionLock = GetCurrentConnectionLock(connectionString);
            return currentConnectionLock.Add(connectionID, lockType);
        }

        public static void RemoveConnectionLock(String connectionString, Guid connectionID)
        {
            ConnectionLock currentConnectionLock = GetCurrentConnectionLock(connectionString);
            currentConnectionLock.Remove(connectionID);
        }

        static ConnectionLock GetCurrentConnectionLock(String connectionString)
        {
            if (!connectionString.StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException();
            }
            connectionString = connectionString.Trim().ToLower();
            lock (allConnectionLocks)
            {
                if (!allConnectionLocks.ContainsKey(connectionString))
                {
                    allConnectionLocks.Add(connectionString, new ConnectionLock());
                }
            }
            return allConnectionLocks[connectionString];
        }
    }
}
