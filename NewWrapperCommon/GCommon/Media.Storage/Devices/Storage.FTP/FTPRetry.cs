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

namespace AvePoint.Media.Storage.FTP
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.GCommon;
    using System.Threading;
    using AvePoint.Media.Storage.Util;
    #endregion

    delegate T RetryLogic<T>();
    delegate void RetryLogic();

    internal class FTPRetry
    {
        StorageLogger logger;
        Int32 maxRetryCount;
        Boolean isRetry;
        Int32 retryInternal;

        public FTPRetry(Boolean isRetry, Int32 maxRetryCount, Int32 retryInternal)
        {
            this.isRetry = isRetry;
            this.maxRetryCount = maxRetryCount;
            this.retryInternal = retryInternal;
            logger = StorageLogger.GetInstance(this.GetType());
        }
        public T Retry<T>(RetryLogic<T> del)
        {
            Int32 retryCount = 0;
            while (true)
            {
                try
                {
                    retryCount++;
                    return del.Invoke();
                }
                catch (Exception ex)
                {
                    if (isRetry && IsRetryable(ex))
                    {
                        try
                        {
                            if (retryCount <= this.maxRetryCount)
                            {
                                logger.Debug("We will retry after " + retryInternal + " s. Retry count: " + retryCount);
                                Thread.Sleep(this.retryInternal * 1000);
                            }
                            else
                            {
                                logger.Error("too many retry failed. Retry count:{0}, msg:{1}", retryCount, ex.ToString());
                                throw;
                            }
                        }
                        catch (ThreadInterruptedException e)
                        {
                            logger.Warn("Retry error: ", e.ToString());
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
        public bool IsRetryable(Exception ex)
        {

            string msg = ex.Message;
            if (!string.IsNullOrEmpty(msg))
            {
                return (msg.Contains("Unable to connect to the remote server") || msg.Contains("The operation has timed-out"));
            }
            else
            {
                return false;
            }
        }
    }
}
