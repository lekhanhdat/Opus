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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveClientTaskRetryHelper : AveTaskRetryHelper
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveClientTaskRetryHelper));

        public AveClientTaskRetryHelper(int maxRetryAttemptCount, params Exception[] exceps)
            : base(maxRetryAttemptCount, exceps)
        {
        }

        public AveClientTaskRetryHelper(int maxRetryAttemptCount, params KeyValuePair<string, string>[] exceps)
            : base(maxRetryAttemptCount, exceps)
        {
        }

        public AveClientTaskRetryHelper(int maxRetryAttemptCount, params int[] serverErrorCodes)
            : base(maxRetryAttemptCount, serverErrorCodes)
        {
        }

        public override void ExecuteWithRetryMechanism(Action taskToRetry)
        {
            int attemptCount = -1;
            while (attemptCount++ < MAX_RETRY_ATTEMPT_COUNT)
            {
                try
                {
                    taskToRetry();
                    break;
                }
                catch(ServerException e)
                {
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}, ServerErrorCode: {2}", MAX_RETRY_ATTEMPT_COUNT, e.ToString(), e.ServerErrorCode);
                        throw;
                    }
                    if (NeedRetryServerException(e))
                    {
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}, ServerErrorCode: {2}", attemptCount + 1, e.Message, e.ServerErrorCode);
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}, ServerErrorCode: {1}", e.ToString(), e.ServerErrorCode);
                    throw;
                }
                catch (Exception e)
                {
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}", MAX_RETRY_ATTEMPT_COUNT, e.ToString());
                        throw;
                    }
                    if (NeedRetry(e))
                    {
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}", attemptCount + 1, e.Message);
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
        }

        public override bool ExecuteWithRetryMechanism(Func<bool, bool> taskToRetry)
        {
            int attemptCount = -1;
            bool needReloadFile = false;
            while (attemptCount++ < MAX_RETRY_ATTEMPT_COUNT)
            {
                try
                {
                    return taskToRetry(needReloadFile);
                }
                catch (ServerException e)
                {
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}, ServerErrorCode: {2}", MAX_RETRY_ATTEMPT_COUNT, e.ToString(), e.ServerErrorCode);
                        throw;
                    }
                    if (NeedRetryServerException(e))
                    {
                        needReloadFile = true;
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}, ServerErrorCode: {2}", attemptCount + 1, e.Message, e.ServerErrorCode);
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}, ServerErrorCode: {1}", e.ToString(), e.ServerErrorCode);
                    throw;
                }
                catch (Exception e)
                {
                    if (attemptCount == MAX_RETRY_ATTEMPT_COUNT)
                    {
                        logger.Error("Failed to retry from an exception ( {0} time(s) retried ), error message : {1}", MAX_RETRY_ATTEMPT_COUNT, e.ToString());
                        throw;
                    }
                    if (NeedRetry(e))
                    {
                        needReloadFile = true;
                        logger.Warn("Retry from an exception ( {0} time(s) retried ) , error message : {1}", attemptCount + 1, e.Message);
                        System.Threading.Thread.Sleep(5000);
                        continue;
                    }
                    logger.Warn("Retry mechanism encountered unexcepted exception, error message : {0}", e.ToString());
                    throw;
                }
            }
            return false;
        }

        private bool NeedRetryServerException(ServerException e)
        {
            return NeedRetryByServerErrorCode(e.ServerErrorCode) || NeedRetryByExceptionMessage(e);
        }
    }
}
