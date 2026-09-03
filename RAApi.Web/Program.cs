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
using AngleSharp.Text;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Api.Services.Services;
using AvePoint.RA.Api.Web.Authorize;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Api.Web.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.RateLimitsPolicyManager;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Service.Services.Common;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Extentions.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Storage.Util;
using System;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

#if DEBUG
    RALogger.ConfigFile = "Config/ApiLog4net.dev.config";
#else
    RALogger.ConfigFile = "Config/ApiLog4net.config";
#endif

RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

#region Application init
try
{
#if DEBUG
    while (System.IO.File.Exists("C:\\InitApiGlobal.sleep"))
    {
        System.Threading.Thread.Sleep(2000);
    }
#endif
    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((object sender, UnhandledExceptionEventArgs args) =>
    {
        logger.Error("App crashed, {0}", args.ExceptionObject);
        RALogger.WaitForAllLogsFlush();
    });

    RMServiceManagerUtil.Init();
    RMGlobalConfiguration.Init();

    LoggerInitializer.InitializeLogger(LogType.ServiceLog);
    RALogger.SetCustomizedLogPostfix("V: " + RMGlobalConfiguration.EnvSetting.ProductVersion);
    StorageApiConfiguration.Setup();
    CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;
    //init castle + MVC
    GlobalConfig.Init();
    //init singalR
    InitSignalR();

    AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
    CommonPoolUserUtil.Init(false);

    RAWebLocalCacheReleaserInitializer.Init("ApiWebLocalCacheConfig");
}
catch (Exception ex)
{
    logger?.Error($"init global error:{ex}");
}
#endregion

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureLogging((hostingContext, loggingBuilder) =>
{
    if (!hostingContext.HostingEnvironment.IsDevelopment())
    {
        loggingBuilder.ClearProviders();
    }
});
builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.AddServerHeader = false;
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<RateLimitsPolicyManager, RateLimitsPolicyManager>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.SetIsOriginAllowed((originUrl) =>
                          {
                              if (RMGlobalConfiguration.AppConfig[RMAppSettingKey.CORS_ALLOWED_ORIGIN].Split(";").Contains(originUrl, StringComparison.OrdinalIgnoreCase))
                              {
                                  return true;
                              }
                              var aosTenantId = TenantHelper.GetTenantBySiteUrlAsync(originUrl).GetAwaiter().GetResult();
                              return !string.IsNullOrEmpty(aosTenantId);
                          })
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials());
});

// Add services to the container.
builder.Services.AddControllersWithViews(options => 
    {
        options.Filters.Add<TimingActionFilterAttribute>();
        options.Filters.Add<JwtValidationHandler>();
        //options.Filters.Add<APIRateLimitFilter>();
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new IsoDateTimeConverter());
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Opus Web API";
    config.Description = "OpenAPI document for Opus Web API";
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseExceptionHandler(appError =>
{
    appError.Run(async (context) =>
    {
        var response = context.Response;
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            var error = contextFeature.Error;
            string path = context.Request.Path;
            logger.Error($"Request: {path}. Error: {error}");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync("{}");
        }
    });
});
app.UseSecurityHeaderMiddleware(builder =>
{
    builder
    .RemoveCustomizedHeader("X-Powered-By")
    .RemoveCustomizedHeader("Server")
    .AddStrictTransportSecurityHeader()
    .AddPermissionsPolicyHeader()
    .AddFrameOptionsSameOriginHeader()
    .AddXSSProtectionHeader()
    .AddContentSecurityPolicyHeader()
    .AddXContentTypeOptionsNoSniffHeader()
    .AddPragmaNoCacheHeader()
    .AddCacheControlHeader()
    .AddReferrerPolicyHeader()
    .AddExpires();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void InitSignalR()
{
    try
    {
        var thread = new Thread(() =>
        {
            logger.Info("Starting SignalR server setup in a separate thread.");

            var signalrService = (ISignalRService)PlatformWindsorManager.GetService(
                "AvePoint.RA.Service.Services.SignalR.SignalRService",
                typeof(ISignalRService));
            signalrService.SignalRSetup();

            logger.Info("Successfully set up SignalR server connection.");
        });
        thread.Start();
        logger.Info("Started thread to initialize SignalR setup.");
    }
    catch (Exception ex)
    {
        logger.Error("Failed to set up SignalR server.", ex);
    }
}
