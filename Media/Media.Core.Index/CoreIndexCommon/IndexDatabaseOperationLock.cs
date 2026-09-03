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
    using System.Threading;
    #endregion

    /// <summary>
    /// this class is used for synchronizing operations. 
    /// <example> 
    /// <code>
    ///           using (OperationLock sqliteLock = new OperationLock(connection string))
    ///           {
    ///             // operations running here will be sequentially
    ///           }
    /// </code>
    /// </example>
    /// </summary>
    internal sealed class IndexDatabaseOperationLock : IDisposable
    {
        static Dictionary<String, Object> opeartionLocks = new Dictionary<String, Object>();

        String connectionString = String.Empty;

        public IndexDatabaseOperationLock(string connectionString)
        {
            this.connectionString = connectionString.ToUpper();
            lock (opeartionLocks)
            {
                if (!opeartionLocks.ContainsKey(this.connectionString))
                {
                    opeartionLocks.Add(this.connectionString, new object());
                }
            }
            var connectionLock = opeartionLocks[this.connectionString];
            Monitor.Enter(connectionLock);
        }

        public void Dispose()
        {
            var connectionLock = opeartionLocks[this.connectionString];
            Monitor.PulseAll(connectionLock);
            Monitor.Exit(connectionLock);
        }
    }
}
