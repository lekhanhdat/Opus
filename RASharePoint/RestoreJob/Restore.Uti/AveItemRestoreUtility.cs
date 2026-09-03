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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using System.Reflection;
namespace AvePoint.Item.Restore
{
    public static class AveItemRestoreUtility
    {
        private static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public static string FormatMessage(Exception e, string format, params object[] parameters)
        {
            string message = e.Message;
            if (e is System.Reflection.TargetInvocationException && e.InnerException != null)
            {
                message = e.InnerException.Message;
            }
            return string.Format("{0}.{1}", string.Format(format, parameters), message);
        }

        public static bool IsSiteExisted(AveObjectModelFactory factory, string siteUrl, ref bool destHostheader)
        {
            bool result = true; //if site don't existed
            try
            {
                using (IAveSite tempSite = factory.CreateSite(siteUrl))
                {
                    destHostheader = tempSite.HostHeaderIsSiteName;
                    return string.Equals(tempSite.Url.TrimEnd('/'), siteUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.WARN, "An error occurred while checking if the destination web that has been existed.SiteUrl:{0}. Error Message: {1}.", siteUrl,e.ToString());
                result = false;
            }
            return result;
        }

        public static string GetWebAppUrl(AveObjectModelFactory factory, string siteName)
        {
            try
            {
                using (IAveSite tempSite = factory.CreateSite(siteName))
                {
                    return tempSite.WebApplication.AlternateUrls.GetResponseUrl(AveUrlZone.Default).Uri.ToString();
                }
            }
            catch(Exception e)
            {
                log.Log(AveLogLevel.DEBUG, "An error occurred while getting web application url with API.SiteUrl:{0}. Error Message: {1}.", siteName, e.ToString());
                int i = 7;
                while (i < siteName.Length)
                {
                    if (siteName.Substring(i , 1) == "/")
                        break;
                    i++;
                }
                string stUrl = siteName.Substring(0, i) + "/";
                return stUrl;
            }
        }
        /// <summary>
        /// Formate like itemName:1.0
        /// </summary>
        /// <param name="itemName"></param>
        /// <param name="uiVersion"></param>
        /// <returns></returns>
        public static string GetItemVersionString(string itemName, int uiVersion)
        {
            return string.Format("{0}:{1}.{2}", itemName, uiVersion / 512, uiVersion % 512);
        }
    }

    public static class NullableBooleanExtension
    {
        public static void SetIfValueNotExist(ref bool? self, bool value)
        {
            if (!self.HasValue)
            {
                self = value;
            }
        }
    }
}
