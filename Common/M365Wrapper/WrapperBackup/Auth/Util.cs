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

namespace ExchangeUtility.Graph
{
    using System;
    using System.Net;

    public static class ExchangePathExtension
    {
        public static string ToParentInternalPath(this string internalPath)
        {
            return internalPath.Substring(0, internalPath.LastIndexOf(ExchangeConstants.PathParser));
        }

        public static string ToDisplayPath(this string internalPath)
        {
            return internalPath.Replace(ExchangeConstants.PathParser, ExchangeConstants.PathCombineChar);
        }

        public static string ToTitle(this string internalPath)
        {
            var index = internalPath.LastIndexOf(ExchangeConstants.PathParser);
            if (index > 0)
            {
                return internalPath.Substring(index + 1);
            }
            return internalPath;
        }
    }

    internal static class HttpExtension
    {
        public static System.Net.HttpStatusCode? StatusCode(this Exception ex)
        {
            var webException = (ex as WebException);
            var httpResponse = webException?.Response as HttpWebResponse;
            return httpResponse?.StatusCode;
        }
    }
}