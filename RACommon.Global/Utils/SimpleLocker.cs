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
using AvePoint.RA.Contract.Services;
using System.Collections.Generic;

namespace AvePoint.RA.Common.Utils
{
    public class SimpleLocker
    {
        private AvePoint.RA.Contract.Services.IRALogger logger = null;
        private readonly object _globalLocker = new object();
        private readonly Dictionary<string, Locker> _lockerPool = new Dictionary<string, Locker>();

        public SimpleLocker(IRALogger logger)
        {
            this.logger = logger;
        }

        public Locker GetLocker(string key)
        {
            lock (_globalLocker)
            {
                if (_lockerPool.ContainsKey(key))
                {
                    _lockerPool[key].Count++;
                }
                else
                {
                    _lockerPool[key] = new Locker(key);
                }

                return _lockerPool[key];
            }
        }

        public void FreeLocker(string key)
        {
            lock (_globalLocker)
            {
                if (_lockerPool.ContainsKey(key))
                {
                    _lockerPool[key].Count--;

                    if (_lockerPool[key].Count == 0)
                    {
                        _lockerPool.Remove(key);
                    }
                }
                else
                {
                    logger.Warn("Error locker state: " + key);
                }
            }
        }

        #region Nested type: Locker

        public class Locker
        {
            public Locker(string key)
            {
                Key = key;
                Count = 1;
            }

            public string Key { get; set; }

            public int Count { get; set; }
        }

        #endregion
    }
}
