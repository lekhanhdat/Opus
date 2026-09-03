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
using AvePoint.RA.Common.Util;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Common
{
    public class Util
    {
        private static Configuration mWebConfiguration;
        private static Configuration WebConfiguration
        {
            get
            {
                if (mWebConfiguration == null)
                {
                    mWebConfiguration = WebUtil.GetWebConfiguration();
                }
                return mWebConfiguration;
            }
        }
        public static string GetAppSettingValue(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                var setting = WebConfiguration.AppSettings.Settings[key];
                if (setting != null)
                {
                    value = setting.Value;
                }
            }
            return value;
        }


        public static string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            { 
                if (e.InnerException != null)
                {
                    comment = e.InnerException.Message;
                }
            }
            return comment;
        }
    }
}
