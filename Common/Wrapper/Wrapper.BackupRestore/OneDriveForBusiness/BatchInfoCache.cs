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


namespace AvePoint.Wrapper.BackupRestore
{
    internal class BatchInfoCache
    {
        Dictionary<string, CacheItem> mInternalCache = new Dictionary<string, CacheItem>();
        public bool TryGet(string key, out CacheItem value)
        {
            return mInternalCache.TryGetValue(key, out value);
            //value = null;
            //CacheItem temp = null;
            //if (mInternalCache.TryGetValue(key, out temp))
            //{
            //    value = temp;
            //    return true;
            //}
            //return false;
        }

        public void Add(string key, CacheItem value)
        {
            this.mInternalCache[key] = value;
        }

        public void Clear()
        {
            this.mInternalCache.Clear();
            this.mInternalCache = null;
        }
    }

    internal class CacheItem
    {
        public object Value;
        public ProcessResult Result;
    }
}
