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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using Util;

namespace RAGoogle.Extension;

public static class DataQueueExtension
{
    private static readonly IRALogger logger = RALogger.GetInstance(typeof(DataQueueExtension));
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
