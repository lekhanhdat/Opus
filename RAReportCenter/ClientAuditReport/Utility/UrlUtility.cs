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

namespace RAReportCenter.ClientAuditReport.Scanner
{
    public static class UrlUtility
    {
        private static AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(UrlUtility));

        public static string GetSiteUrl(string fullUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fullUrl))
                {
                    return "";
                }
                Uri result;
                if (Uri.TryCreate(fullUrl, UriKind.Absolute, out result))
                {
                    var rootUrl = result.GetLeftPart(UriPartial.Authority);
                    var absolutePath = result.AbsolutePath;

                    if (absolutePath.StartsWith("/sites/", StringComparison.OrdinalIgnoreCase) || absolutePath.StartsWith("/teams/", StringComparison.OrdinalIgnoreCase)
                        || absolutePath.StartsWith("/personal/", StringComparison.OrdinalIgnoreCase) || absolutePath.StartsWith("/portals/", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = absolutePath.Split('/');
                        return rootUrl + "/" + path[1] + "/" + path[2];
                    }
                    else if (absolutePath.StartsWith("/search/", StringComparison.OrdinalIgnoreCase) || absolutePath.Equals("/search", StringComparison.OrdinalIgnoreCase))
                    {
                        return rootUrl + "/search";
                    }
                    return rootUrl;
                }
            }
            catch (Exception ex)
            {
                logger.Warn($@"fail get site url,ex:{ex}");
            }
            return "";
        }

        public static string MatchWebUrl(HashSet<string> urlSet, string objectUrl)
        {
            if (string.IsNullOrEmpty(objectUrl))
            {
                return null;
            }
            int lastSlashIndex = -1;
            while (objectUrl.Length > 7 && (lastSlashIndex = objectUrl.LastIndexOf("/")) != -1)
            {
                objectUrl = objectUrl.Substring(0, lastSlashIndex);
                if (urlSet.Contains(objectUrl))
                {
                    return objectUrl;
                }
            }
            return null;
        }
    }
}
