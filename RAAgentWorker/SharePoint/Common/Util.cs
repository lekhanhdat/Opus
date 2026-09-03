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
using AvePoint.RA.Common.Global.Util;
using System;
using System.Configuration;

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

    public enum OnPremiseSPNodeLevel
    {
        SiteCollection = 1,
        APP = 2,
        Web = 3,
        List = 1000,
        Folder = 1002,
        Item = 10000,
        ItemVersion = 10001,
        Attachment = 20000,
        Document = 50000,
        DocumentVersion = 50001,
        FitParentRule = 70000,
    }

    public enum ItemDependencyOption
    {
        NotRestore = 0,
        Overwrite = 1,
        Append = 2,
        SkipConfilctItem = 3,
    }

}
