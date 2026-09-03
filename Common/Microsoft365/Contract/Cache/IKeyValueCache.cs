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
namespace Microsoft365.Common.Cache
{
    using System;
    public interface IKeyValueCache<TKey, TValue>
    {
        /// <summary>
        /// max count of key value pair,if cache reach the max key count, it will remove the keys that have the latest expired time.
        /// </summary>
        int Capacity { get; set; }
        /// <summary>
        /// Get a value which is not expired.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TValue Get(TKey key);
        /// <summary>
        /// add key value with default life time setup in cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddOrUpdate(TKey key, TValue value);
        /// <summary>
        /// add key value with specific expired time
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <param name="expiredOn"></param>
        void AddOrUpdate(TKey key, TValue entry, DateTimeOffset expiredOn);
        /// <summary>
        /// clear all key vaules in cache
        /// </summary>
        void Clear();
    }
}