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
namespace AvePoint.RA.Web.Extentions.Middleware;

using AvePoint.RA.CommonUtil;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

public class LogAPIRequestMiddleware
{
    private static RALogger logger = RALogger.GetInstance(typeof(LogAPIRequestMiddleware));
    private RequestDelegate _next;

    public LogAPIRequestMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            try
            {
                var request = context.Request;
                if (!IsIgnoreRequest(context))
                {
                    logger.Info($"Log web request : {request.Method} {request.Path}, Cost:{stopwatch.ElapsedMilliseconds}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"LogWebRequest error: {ex}");
            }
        }
    }

    private bool IsIgnoreRequest(HttpContext context)
    {
        var path = context.Request.Path.ToString();
        if(path.StartsWith("/Content/", StringComparison.OrdinalIgnoreCase) || 
            path.StartsWith("/Scripts/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/Images/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/aui/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/dist/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        return false;
    }
}
