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
using AvePoint.RA.Api.Web.Authorize;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Web.Extentions.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Filters
{
    public abstract class RMApiPerformanceLoggerAttribute : ActionFilterAttribute
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMApiPerformanceLoggerAttribute));

        protected abstract string PerformanceLog(HttpRequest request, double totalMilliseconds);

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            OnActionExecuting(context);
            
            if(context.Result == null)
            {
                var stopWatch = new Stopwatch();

                stopWatch.Start();

                var nextContext = await next();

                stopWatch.Stop();

                var performanceLog = PerformanceLog(context.HttpContext.Request, stopWatch.Elapsed.TotalMilliseconds);

                s_logger.Debug(performanceLog);

                OnActionExecuted(nextContext);
            }
        }
    }

    public class RMConnectorApiPerformanceLoggerAttribute : RMApiPerformanceLoggerAttribute
    {
        protected override string PerformanceLog(HttpRequest request, double totalMilliseconds)
        {
            return $"[Performance-Connector] Tenant Id: [{TenantLocalValue.LogonGroupId}] Method: [{request.Path}] Used Time [{totalMilliseconds} ms].";
        }
    }

    public class RMAgentApiPerformanceLoggerAttribute : RMApiPerformanceLoggerAttribute
    {
        protected override string PerformanceLog(HttpRequest request, double totalMilliseconds)
        {
            return $"[Performance-Agent] Tenant Id: [{TenantLocalValue.LogonGroupId}] Agent Id: [{request.GetRequestHeadersParam(RequestHeadersParam.HYBRID_AGENT_ID)}] Job Id: [{request.GetRequestHeadersParam(RequestHeadersParam.AGENT_JOB_ID)}] Method: [{request.Path}] Used Time [{totalMilliseconds} ms]."; ;
        }
    }
}
