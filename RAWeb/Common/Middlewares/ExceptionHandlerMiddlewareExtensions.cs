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
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Middlewares
{
    internal static class ExceptionHandlerMiddlewareExtensions
    {
        private static IRALogger logger = RALogger.GetInstance(typeof(ExceptionHandlerMiddlewareExtensions));

        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseStatusCodePages(HandleStatusCodePagesException);

            app.UseExceptionHandler(appError =>
            {
                appError.Run(HandleServerException);
            });
        }


        private static async Task HandleStatusCodePagesException(StatusCodeContext statusContext)
        {
            var response = statusContext.HttpContext.Response;
            var statusCode = response.StatusCode;
            switch (statusCode)
            {
                case 404:
                    response.Redirect("/ErrorPage/PageNotFound", true);
                    break;
                default:
                    response.Redirect("/ErrorPage/NotAvailableService", true);
                    break;
            }

            await Task.CompletedTask;
        }

        private static async Task HandleServerException(HttpContext context)
        {
            var response = context.Response;
            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null)
            {
                var error = contextFeature.Error;
                logger.Error($"Application_Error: {error}");
                var cryptoEx = error as CryptographicException;
                if (cryptoEx != null)
                {
                    //Clear Session
                    //FederatedAuthentication.WSFederationAuthenticationModule.SignOut();
                    //TODO
                }

                string path = context.Request.Path;
                if(path.StartsWith("/api", System.StringComparison.OrdinalIgnoreCase))
                {
                    response.Clear();
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync("{}");
                }
                else
                {
                    response.Redirect("/ErrorPage/NotAvailableService", true);
                }
            }
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await Task.CompletedTask;
        }
    }
}
