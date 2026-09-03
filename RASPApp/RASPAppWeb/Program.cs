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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Web.Extentions.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RASPAppWeb.Middleware;
using System;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

#if DEBUG
    RALogger.ConfigFile = "APPLog4net.dev.config";
#else
    RALogger.ConfigFile = "APPLog4net.config";
#endif
RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

#region Application init
try
{
    RMServiceManagerUtil.Init();
    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((object sender, UnhandledExceptionEventArgs args) =>
    {
        logger.Error("App crashed, {0}", args.ExceptionObject);
        RALogger.WaitForAllLogsFlush();
    });
    RMGlobalConfiguration.Init();
    AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
}
catch (Exception ex)
{
    logger?.Error($"init global error:{ex}");
}
#endregion

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureLogging((hostingContext, loggingBuilder) =>
{
    if (!RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
    {
        loggingBuilder.ClearProviders();
    }
});
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.AddServerHeader = false;
    //options.AllowSynchronousIO = true;
    if (!RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
    {
        var certFile = "tls.crt";
        var keyFile = "tls.key";
        options.ConfigureHttpsDefaults(listenOptions =>
        {
            listenOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certFile, keyFile);
        });
    }
});
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

#region Init Routers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Default}/{action=Index}/{id?}");

app.MapWhen(
    context => context.Request.Path.ToString().EndsWith("Default.aspx"),
    appBuilder => appBuilder.UseRedirectWithoutUrlExtentionMiddleware()
);
app.MapWhen(
    context => context.Request.Path.ToString().EndsWith("RelatedRecords.ashx"),
    appBuilder => appBuilder.UseRelatedRecordsHandlerMiddleware()
);
#endregion

app.Run();
