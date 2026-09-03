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
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace AvePoint.RA.Web.Extentions.Util
{
    public static class HttpContextExtensions
    {
        private static readonly HttpContextAccessor HttpContextAccessor = new HttpContextAccessor();
        private static IRALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static HttpContext CurrentHttpContext(bool throwIfNull = true)
        {
            var httpContext = HttpContextAccessor.HttpContext;
            if (throwIfNull && httpContext == null)
            {
                throw new Exception("must AddHttpContextAccessor to Services");
            }
            return httpContext;
        }

        public static string GetClientIP(this HttpContext context)
        {
            string result = string.Empty;
            try
            {
                if (context.Request.Headers.ContainsHeader("X-Forwarded-For"))
                {
                    result = context.Request.Headers.GetHeaderValue("X-Forwarded-For");
                    return result.Split(':')[0];
                }
                if (context.Request.Headers.ContainsHeader("X-Real-IP"))
                {
                    result = context.Request.Headers.GetHeaderValue("X-Real-IP");
                    return result;
                }
                return context.Connection.RemoteIpAddress?.ToString().Split(':')[0];
            }
            catch (Exception ex)
            {
                Logger.Error($"Get client ip address error,{ex}");
                return result;
            }
        }

    }
}
