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
namespace Microsoft365.Common.HttpUtil
{
    using System;

    public struct ToomanyRequestRetryOption
    {
        public ToomanyRequestRetryOption(TimeSpan maxRetryAfter,TimeSpan maxRetryTime,TimeSpan defaultRetryAfter,int maxRetries)
        {
            MaxAfterTime=maxRetryAfter;
            MaxRetryTime=maxRetryTime;
            DefaultRetryAfter=defaultRetryAfter;
            MaxRetries = maxRetries;
        }
        public ToomanyRequestRetryOption()
        { }
        /// <summary>
        /// max time wait before next retry
        /// </summary>
        public TimeSpan MaxAfterTime { get; set; } = TimeSpan.FromMinutes(15);
        /// <summary>
        /// max time for retry one request.
        /// </summary>
        public TimeSpan MaxRetryTime { get; set; } = TimeSpan.FromMinutes(60);
        /// <summary>
        /// max retry times.
        /// </summary>
        public int MaxRetries { get; set; } = int.MaxValue;
        public TimeSpan DefaultRetryAfter { get; set; } = TimeSpan.FromMinutes(2);

        public override int GetHashCode()
        {
            return HashCode.Combine(MaxAfterTime,MaxRetryTime, DefaultRetryAfter, MaxRetries);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            ToomanyRequestRetryOption? option = obj as ToomanyRequestRetryOption?;
            return option.HasValue
                && this.MaxAfterTime == option.Value.MaxAfterTime
                && this.MaxRetryTime == option.Value.MaxRetryTime
                && this.DefaultRetryAfter == option.Value.DefaultRetryAfter
                && this.MaxRetries == option.Value.MaxRetries;
        }
    }
}
