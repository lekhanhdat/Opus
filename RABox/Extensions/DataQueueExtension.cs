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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace RABox.Extensions
{
    public static class DataQueueExtension
    {
        private static readonly ILogger logger = LoggerFactory.Get(MethodBase.GetCurrentMethod().DeclaringType);

        public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this DataQueue<TSource> source, int size)
        {
            TSource[] bucket = null;
            var count = 0;
            TSource item = default;
            while (!Equals(item = source.ReadAsync().Result, default(TSource)))
            {
                if (bucket == null)
                    bucket = new TSource[size];

                bucket[count++] = item;
                if (count != size)
                    continue;

                yield return bucket;

                bucket = null;
                count = 0;
            }

            if (bucket != null && count > 0)
                yield return bucket.Take(count).ToArray();
        }

        public static IEnumerable<TSource> ToIEnumerable<TSource>(this DataQueue<TSource> source,
            Action<TSource> action = null, CancellationToken cancellationToken = default)
        {
            do
            {
                var data = source.ReadAsync();
                if (Equals(data.Result, default(TSource)))
                    yield break;

                action?.Invoke(data.Result);
                yield return data.Result;
            }
            while (!cancellationToken.IsCancellationRequested);
            logger.Warn("Finish to convert Queue to IEnumerable");
        }
    }
}
