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
using System.Threading;

namespace AvePoint.GCommon.Utility
{
    public class FileLock : IDisposable
    {
        private Mutex mLock;
        private string mLockName;
        private IAveLogger mLog = AveLogger.GetInstance(typeof(FileLock));
        private FileLock(string lockName)
        {
            bool owned;
            mLockName = lockName;
            mLock = new Mutex(true, lockName, out owned);
            mLog.Info($"Enter file locker.LockName:{mLockName},Owned:{owned}");
            if (!owned)
            {
                mLock.WaitOne();
            }
        }

        public static FileLock LockFile(string fileName)
        {
            return new FileLock(fileName);
        }

        public void Dispose()
        {
            mLock.ReleaseMutex();
            mLog.Info($"Exit file locker.LockName:{mLockName}");
        }
    }
}
