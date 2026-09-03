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
using System.IO;
using System.Text;
using System.Threading;

namespace AvePoint.RA.Common.RAProcess.Locker
{
    public class RMProcessMutexLocker : IRMProcessLocker
    {
        private readonly string _mutexName;

        public RMProcessMutexLocker(string mutexName)
        {
            _mutexName = mutexName;
        }

        public void Lock(Action action)
        {
            using (var mutex = new Mutex(false, _mutexName))
            {
                try
                {
                    mutex.WaitOne();
                    action();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public T Lock<T>(Func<T> func)
        {
            using (var mutex = new Mutex(true, _mutexName))
            {
                var res = default(T);
                try
                {
                    mutex.WaitOne();
                    res = func();
                }
                finally
                {
                    mutex.ReleaseMutex();
                }

                return res;
            }
        }
    }
}
