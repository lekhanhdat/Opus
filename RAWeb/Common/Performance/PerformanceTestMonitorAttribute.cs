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
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Performance
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PerformanceTestMonitorAttribute : ActionFilterAttribute
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(PerformanceTestMonitorAttribute));

        private const string DefaultHeaderName = "PerformanceLogEnable";

        public string FunctionName { get; set; }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var headers = context.HttpContext.Request.Headers;
            var headerVal = headers.GetFirstHeaderValue(DefaultHeaderName);
            if (bool.TryParse(headerVal, out var result) && result)
            {
                Logger.Debug($"[Performance Log]-[{FunctionName}] request.");
            }
            return base.OnActionExecutionAsync(context, next);
        }
    }
}
