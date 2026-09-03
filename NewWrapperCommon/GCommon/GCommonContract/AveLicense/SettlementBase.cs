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
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Contract.AveLicense
{
    public abstract class SettlementBase
    {
        public bool IsSharepointTime { get; set; }
        /// <summary>
        /// License系统的当前时间
        /// </summary>
        public DateTime CachedCurrentTime
        {
            get
            {
                if ((DateTime.Now - Stamp) > MaxCachedHostTime)
                {
                    CachedSystemTime = GetSystemTime();
                }
                return CachedSystemTime;
            }
        }

        /// <summary>
        /// 重新获取License 系统时间
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetSystemTime();

        /// <summary>
        /// 缓存时间的最大保留时间，超过该时间，缓存时间会被更新
        /// </summary>
        protected TimeSpan MaxCachedHostTime = new TimeSpan(0, 5, 0);

        /// <summary>
        /// 缓存时间的local host时间戳
        /// </summary>
        protected DateTime Stamp { get; set; }

        /// <summary>
        /// 缓存的License System时间
        /// </summary>
        private DateTime cachedSystemTime;
        protected DateTime CachedSystemTime
        {
            get
            {
                return cachedSystemTime;
            }
            set
            {
                this.Stamp = DateTime.Now;
                this.cachedSystemTime = value;
            }
        }

    }
}
