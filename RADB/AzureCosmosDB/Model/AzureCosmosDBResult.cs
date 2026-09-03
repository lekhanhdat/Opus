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
using AvePoint.RA.DB.AzureCosmosDB.Exceptions;
using AvePoint.RA.DB.AzureCosmosDB.WriteMode;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.DB.AzureCosmosDB.Model
{
    public record RMAzureCosmosDBQueryPagniationResult<TResult>(string ContinuationToken, IEnumerable<TResult> Values);

    public record RMAzureCosmosDBRangeActionFailedResult(Record Item, bool IsOptimisticLockConflict, Exception Exception);

    public class RMAzureCosmosDBRetryerResult
    {
        public int RetriedTimes { get; internal set; }

        public int MaxRetryTimes { get; internal set; }

        public bool IsSucceed { get; internal set; }

        public bool IsOptimisticLockConflict { get; internal set; }

        public bool CanContinueRetry { get; internal set; }

        public RMAzureCosmosDBRetryerException Exception { get; internal set; }
    }

    public class RMAzureCosmosDBImmediatelyConcurrentActionResult
    {
        public Record Item { get; internal set; }

        public bool IsSucceed { get; internal set; }

        public bool IsOptimisticLockConflict { get; internal set; }

        public bool CanContinueRetry { get; internal set; }

        public RMAzureCosmosDBRetryerException Exception { get; internal set; }
    }

    public class RMAzureCosmosDBDelayConcurrentActionResult
    {
        public Record Item { get; internal set; }

        public RMAzureCosmosDBActionType ActionType { get; internal set; }

        public bool IsSucceed { get; internal set; }

        public bool IsOptimisticLockConflict { get; internal set; }

        public bool CanContinueRetry { get; internal set; }

        public RMAzureCosmosDBRetryerException Exception { get; internal set; }
    }
}
