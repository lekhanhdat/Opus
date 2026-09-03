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
using AvePoint.Records.Core.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Api
{
    public class AzureFileShareApiUtil
    {
        public static string UrlCombin(string fullUrl, string relativeUrl)
        {
            if (fullUrl.EndsWith("/"))
            {
                fullUrl = fullUrl.Substring(0, fullUrl.Length - 1);
            }

            if (relativeUrl.StartsWith("/"))
            {
                relativeUrl = relativeUrl.Substring(1);
            }

            if (relativeUrl.EndsWith("/"))
            {
                relativeUrl = relativeUrl.Substring(0, relativeUrl.Length - 1);
            }

            return string.IsNullOrEmpty(fullUrl) ? relativeUrl : fullUrl + "/" + relativeUrl;
        }

        public static string UrlCorrect(string url)
        {
            if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static Guid GenerateId(string fullPath)
        {
            fullPath = UrlCorrect(fullPath);
            return ("Azure File Share Source" + fullPath).ToMd5();
        }

        public static Guid GenerateParentId(string fullPath, string name)
        {
            fullPath = UrlCorrect(fullPath);
            fullPath = fullPath.Replace(name, "");
            fullPath = UrlCorrect(fullPath);
            return ("Azure File Share Source" + fullPath).ToMd5();
        }
    }
}
