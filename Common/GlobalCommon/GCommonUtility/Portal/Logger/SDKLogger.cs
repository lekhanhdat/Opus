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
using System.Text;
using System.Threading.Tasks;
using Cloud.Sdk.Core.Logging;

namespace AvePoint.GCommon.Utility.Portal.Logger
{
    public class SDKLogger : ISdkLogger
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SDKLogger));
        public void Debug(string formatter, params object[] parameters)
        {
            logger.Debug("AOS SDK Debug:" + formatter, parameters);
        }

        public void Error(Exception ex, string formatter, params object[] parameters)
        {
            string message = string.Empty;
            try
            {
                message = string.Format(formatter, parameters);
            }
            catch (Exception e)
            {
                message = formatter + string.Join(";", parameters);
            }
            logger.Error("AOS SDK Error:" + message + "\n" + ex?.ToString());
        }

        public void Error(string formatter, params object[] parameters)
        {
            logger.Error("AOS SDK Error:" + formatter, parameters);
        }

        public void Info(string formatter, params object[] parameters)
        {
            logger.Info("AOS SDK Info:" + formatter, parameters);
        }

        public void Warn(string formatter, params object[] parameters)
        {
            logger.Warn("AOS SDK Warn:" + formatter, parameters);
        }
    }
}
