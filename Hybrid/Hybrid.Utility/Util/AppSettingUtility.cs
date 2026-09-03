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
using System;
using System.Configuration;
using System.Reflection;

namespace AvePoint.Hybrid.Utility.Util
{
    public class AppSettingUtility
    {
        private static IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static string GetSettingByKey(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            //logger.Info(string.Format("Get the setting value from Environment, key : '{0}', value : '{1}'", key, value));

            if (string.IsNullOrEmpty(value))
            {
                value = ConfigurationManager.AppSettings.Get(key);
                logger.Warn(string.Format("Can't find the setting value from Environment, key : '{0}', try to get it from app setting, value : '{1}'", key, value));
            }

            if (!string.IsNullOrEmpty(value))
            {
                value = value.Trim().TrimEnd('/');
            }
            return value;
        }
    }


}
