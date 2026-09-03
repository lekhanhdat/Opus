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

namespace AvePoint.Media.Storage.TSM
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

    delegate object RetryLogic();

    class TSMRetry
    {
        StorageLogger logger;
        int maxRetryCount;
        int retryInternal;
        bool isRetry;
        int retryCount;

        public TSMRetry(bool isRetry,int maxRetryCount,int retryInternal)
        {
            this.maxRetryCount = maxRetryCount;
            this.isRetry = isRetry ;
            this.retryInternal = retryInternal;
            logger = StorageLogger.GetInstance(this.GetType());
        }

        public object Retry(bool isMoreRetry, TSMIOException ioe, RetryLogic retryLogic)
        {
            if (ioe != null && !ioe.IsRetryable())
            {
                throw ioe;
            }
            if (isMoreRetry)
            {
                try
                {
                    logger.Info("isMoreRetry is true we will retry immediately");
                    return retryLogic();
                }
                catch (IOException e)
                {
                    logger.Warn("Warn : ", e);
                }
            }
            if (isRetry)
            {
                while (retryCount < maxRetryCount)
                {
                    ++retryCount;
                    logger.Debug("We will retry after " + retryInternal + " s. Retry count: " + retryCount);
                    try
                    {
                        Thread.Sleep(retryInternal * 1000);
                    }
                    catch(ThreadInterruptedException e)
                    {
                        logger.Error("Error : ", e);
                        throw new ThreadInterruptedException(e.Message);
                    }
                    try
                    {
                        logger.Info("Begin retry");
                        Object ret = retryLogic();
                        logger.Info("Retry successfully.");
                        retryCount = 0;
                        return ret;
                    }
                    catch (TSMIOException e)
                    {
                        logger.Warn("Retry error : ", e);
                    }
                    catch (IOException ex)
                    {
                        logger.Error(ex.Message,ex);
                        break;
                    }
                }
                logger.Error("Retry fail.");
            }
            throw ioe;
        }

    }
}
