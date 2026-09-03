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
using AvePoint.GCommon;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace AvePoint.RA.Api.Web.Public.Filters
{
    public class TimingActionFilterAttribute : ActionFilterAttribute
    {
        private static readonly IAveLogger logger = AveLogger.GetInstance(typeof(TimingActionFilterAttribute));
        private const string Key = "ExcuteTime";

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpMethod = context.HttpContext.Request.Method;
            var path = context.HttpContext.Request.Path.Value;
            if (!@"/healthz".Equals(path))
            {
                logger.Info($"Start excute request: {httpMethod} {path}");
            }
            var stopWatch = new Stopwatch();
            context.ActionDescriptor.Properties[Key] = stopWatch;
            stopWatch.Start();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (!context.ActionDescriptor.Properties.ContainsKey(Key))
            {
                return;
            }

            if (context.ActionDescriptor.Properties[Key] is Stopwatch stopWatch)
            {
                stopWatch.Stop();
                var httpMethod = context.HttpContext.Request.Method;
                var path = context.HttpContext.Request.Path.Value;
                if (!@"/healthz".Equals(path))
                {
                    logger.Info($"Excute request: {httpMethod} {path}, cost: {stopWatch.Elapsed}");
                }
            }
        }
    }
}
