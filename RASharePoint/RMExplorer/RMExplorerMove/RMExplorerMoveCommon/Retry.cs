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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RetryUtil
    {
        public delegate T RetryDelegate<T>();
        private RALogger logger = RALogger.GetInstance(typeof(RetryUtil));
        private int MaxRetryCount { get; set; } = 3;

        public T Excute<T>(RetryDelegate<T> del)
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    return del.Invoke();
                }
                catch (Exception re)
                {
                    if (counter > this.MaxRetryCount)
                    {
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", counter, re);
                        throw;
                    }
                    logger.Info("Retry after at once. Retry count: " + counter);
                    continue;
                }
            }
        }

        public T Excute<T>(Func<T> func)
        {
            int counter = 0;
            while (true)
            {
                try
                {
                    counter++;
                    return func();
                }
                catch (Exception ex)
                {
                    if (counter > this.MaxRetryCount)
                    {
                        logger.Error("too many retry failed. Retry count:{0}, msg:{1}", counter, ex);
                        throw;
                        
                    }
                    logger.Info("Retry after at once. Retry count: " + counter);
                    continue;
                }
            }
        }
    }
}
