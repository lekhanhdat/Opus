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
using AvePoint.Hybrid.ClientCore.Logging;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;


namespace AvePoint.Hybrid.Utility
{
    public class HybridSDKLogger : ISdkLogger
    {

        private IRALogger logger = RALogger.GetInstance(typeof(HybridSDKLogger));

        public void Debug(string formatter, params object[] parameters)
        {
            logger.Debug(formatter, parameters);
        }

        public void Error(Exception ex, string formatter, params object[] parameters)
        {
            logger.Error(formatter, parameters);
        }

        public void Error(string formatter, params object[] parameters)
        {
            logger.Error(formatter, parameters);
        }

        public void Info(string formatter, params object[] parameters)
        {
            logger.Info(formatter, parameters);
        }

        public void Warn(string formatter, params object[] parameters)
        {
            logger.Warn(formatter, parameters);
        }
    }
}
