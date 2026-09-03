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

namespace AvePoint.GCommon.GraphAPI
{
    using System;
    using System.Threading.Tasks;

    public interface IRetryable
    {
        object Retry<T1, T2>(Func<T1, T2, object> func, T1 a, T2 b);

        TResult Retry<TIn, TResult>(
            Func<Task<TResult>, Task, TResult> excuteSDKRequest,
            Func<TIn, Task<TResult>> doTask1 = null,
            Func<Task<TResult>> doTask2 = null,
            Func<TIn, Task> doTask3 = null,
            Func<Task> doTask4 = null,
            TIn requestBody = default(TIn));
    }



}