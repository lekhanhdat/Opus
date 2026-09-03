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
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.StorageApi;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Common;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.Web.Common.Middlewares;
using AvePoint.RA.Web.Config;
using AvePoint.RA.Web.Extentions.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoapCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using LogType = AvePoint.RA.CommonUtil.LogType;

namespace AvePoint.RA.Web
{
    public class Program
    {
        private static IRALogger logger = null;
        private static readonly List<string> _DangerousStringList = new List<string>() { "&", "*", " " };

        public static void Main(string[] args)
        {
            ApplicationStart();

            
            var builder = WebApplication.CreateBuilder(args);
            if (!RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
            {
                builder.Logging.ClearProviders();
            }

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
                options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
            });
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSoapCore();
            builder.Services.AddSingleton<IRecordsService, RecordsService>();

            //Disable API controller actions try to infer parameters from DI
            //https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/7.0/api-controller-action-parameters-di
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.DisableImplicitFromServicesParameters = true;
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                //options.Filters.Add<JwtValidationHandler>();
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            var app = builder.Build();
            // Enforce HTTPS as early as possible to prevent insecure transport
            app.UseHttpsRedirection();
            app.ConfigureExceptionHandler();
            app.UseLogAPIRequestMiddleware();
            app.UseSecurityHeaderMiddleware(builder =>
            {
                builder
                .AddNoCacheWhiteList("/aui/", "/dist/", "/3rd/", "/images/", "content/fonts/fa/webfonts")
                .RemoveCustomizedHeader("X-Powered-By")
                .RemoveCustomizedHeader("Server")
                .AddStrictTransportSecurityHeader()
                .AddMicrophonePermissionsPolicyHeader()
                .AddFrameOptionsSameOriginHeader()
                .AddXSSProtectionHeader()
                .AddXContentTypeOptionsNoSniffHeader()
                .AddPragmaNoCacheHeader()
                .AddCacheControlHeader()
                .AddReferrerPolicyHeader()
                .AddExpires();
            });
            app.Use(AcceptRequest);
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSoapEndpoint<IRecordsService>(
                "/RecordsService.asmx",
                new SoapEncoderOptions());

            SetCspHeader(app);
            app.UseSecurityHeaderWithNonceMiddleware();
            app.RegisterRoutes();
            app.RegisterApiRoutes();

            Console.WriteLine("RAWeb started.");
            app.Run();
        }
       
        private static void ApplicationStart()
        {

#if DEBUG
            while (File.Exists("C:\\InitGlobal.sleep"))
            {
                Thread.Sleep(2000);
            }
#endif
            try
            {
                //init log
                InitLogConfigure();
                RMServiceManagerUtil.Init();
                RMGlobalConfiguration.Init();
                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    RMGlobalConfiguration.AppConfig.SetEnvironmentVariable(RMAppSettingKey.ENABLE_SECURITY_TRIMMING, "true");
                }
                ThreadPool.SetMinThreads(10, 10);
                FipsModeUtil.InitControlCryptoMode();
                CspCommunicationWrapper.CommunicationEncryptionKey = CspCommunicationWrapper.staticCommunicationEncryptionKey;


                AppDomain.CurrentDomain.UnhandledException += FlushLog;

                GlobalConfig.Init();
                StorageApiConfiguration.Setup();
                //init singalr server connection
                InitSignalR();

                AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
                PoolUserUtil.Init(false);
                //更新数据库结构
                //new RMDBInitializer().InitializeControlDatabase();

                RAWebLocalCacheReleaserInitializer.Init("WebLocalCacheConfig");
            }
            catch (Exception ex)
            {
                logger?.Error($"init global error:{ex.ToString()}");
            }

        }

        private static void FlushLog(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            RALogger.WaitForAllLogsFlush();
        }

        private static void InitLogConfigure()
        {
            try
            {
#if DEBUG
                RALogger.ConfigFile = "Config/ControlLog4net.dev.config";
#else
                RALogger.ConfigFile = "Config/ControlLog4net.config";
#endif
                logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
                //Init log configure
                LoggerInitializer.InitializeLogger(LogType.ServiceLog);
                RALogger.SetCustomizedLogPostfix("V: " + RMGlobalConfiguration.EnvSetting.ProductVersion);
            }
            catch (Exception ex)
            {
                throw new Exception("Init logging exception: ", ex);
            }
        }

        private static void InitSignalR()
        {
            try
            {
                Thread curr = new Thread(() =>
                {
                    logger.Info("Begin to set up signalr server connection.");

                    ISignalRService signalrService = (ISignalRService)PlatformWindsorManager.GetService("AvePoint.RA.Service.Services.SignalR.SignalRService", typeof(ISignalRService));
                    signalrService.SignalRSetup();

                    logger.Info("Successfully set up signalr server connection");

                });
                curr.Start();
                logger.Info("Start thread to init sigalr setup.");

            }
            catch (Exception e)
            {
                logger.Error("Fail to setup signalr server.", e);
            }
        }

        private static async Task AcceptRequest(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.ToString();
            var isDangerRequestPath = _DangerousStringList.Any(d => path.Contains(d));
            if (isDangerRequestPath)
            {
                context.Response.Redirect("/ErrorPage/PageNotFound", true);
                await Task.CompletedTask;
            }

            AcquireRequestState(context);

            await next(context);

            CleanTenantInfo();

        }

        private static void AcquireRequestState(HttpContext context)
        {
            var cookieCultureName = "";
            var languages = context.Request.GetTypedHeaders().AcceptLanguage;
            if (languages != null && languages.Count > 0)
            {
                cookieCultureName = languages.First().Value.Value;
            }
            if (cookieCultureName == "")
            {
                cookieCultureName = CultureUtil.GetDefaultCulture();
            }

            CultureUtil.SetCulture(cookieCultureName);
        }

        private static void CleanTenantInfo()
        {
            TenantLocalValue.LogonGroupId = null;
            TenantLocalValue.LogonUserId = null;
            TenantLocalValue.AccountType = Contract.RMWeb.RMAccountType.None;
            TenantLocalValue.DisplayName = null;
            TenantLocalValue.PartnerUser = null;
            TenantLocalValue.CallerType = null;
        }

        /// <summary>
        /// "default-src 'self';" 
        /// "script-src 'self' 'unsafe-inline' 'unsafe-eval' blob:;"
        /// "style-src 'self' 'unsafe-inline';
        /// font-src 'self' data:; " 
        /// "form-action 'self' *.sharepointguild.com https://apwebapptest.azurewebsites.net;
        /// frame-ancestors 'self';" 
        /// "base-uri 'self';" 
        /// "img-src 'self' data:
        /// </summary>
        /// <param name="app"></param>
        private static void SetCspHeader(IApplicationBuilder app)
        {
            try
            {
                var formActionsSource = GetCustomFormActionsSource();
                //https://docs.nwebsec.com/en/latest/nwebsec/getting-started.html
                string[] copilotConnectSources = [];
                string chatbotApiURL = RMGlobalConfiguration.AppConfig.GetChatBotAPIUrl();
                try
                {
                    if (!string.IsNullOrEmpty(chatbotApiURL))
                    {
                        var sourceList = new List<string>() { "wss://*.speech.microsoft.com" };
                        var chatbotURL = RMGlobalConfiguration.AppConfig[RMAppSettingKey.CHAT_BOT_URL];
                        if (!string.IsNullOrEmpty(chatbotURL))
                        {
                            sourceList.Add(chatbotURL);
                        }
                        
                        chatbotApiURL = chatbotApiURL.TrimEnd('/');
                        var serviceNameIdx = chatbotApiURL.IndexOf("/copilot");
                        if (serviceNameIdx != -1)
                        {
                            var chatbotHostURL = chatbotApiURL[..serviceNameIdx];
                            sourceList.Add(chatbotHostURL);
                            sourceList.Add(chatbotHostURL.Replace("https://", "wss://"));
                            copilotConnectSources = sourceList.ToArray(); 
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"Parse cpoilot host URL error: {e}");
                }
                app.UseCsp(cspHeaderConfig => cspHeaderConfig
                    .DefaultSources(s => s.None())
                    .UpgradeInsecureRequests()
                    .ScriptSources(s => s.Self().CustomSources("blob:", "*.aptrinsic.com", "*.avepointonlineservices.com"))
                    .StyleSources(s => s.Self().UnsafeInline().CustomSources("*.aptrinsic.com", "*.avepointonlineservices.com", "fonts.googleapis.com"))
                    .FontSources(s => s.Self().CustomSources("data:", "fonts.gstatic.com", "res.cdn.avepointonlineservices.com", "res-1.cdn.office.net"))
                    .ImageSources(s => s.Self().CustomSources("data:", "*.aptrinsic.com", "storage.googleapis.com", "https://*.avepointonlineservices.com", "blob:"))
                    .ConnectSources(s => s.Self().CustomSources(["*.aptrinsic.com", "*.avepointonlineservices.com", .. copilotConnectSources]))
                    .FrameSources(s => s.Self().CustomSources("www.youtube.com"))
                    .MediaSources(s => s.Self().CustomSources("blob:", "*.avepointonlineservices.com", "*.sharepointguild.com"))
                    .WorkerSources(s =>
                    {
                        s.Self().CustomSources("blob:", "data:");
                        if (!string.IsNullOrEmpty(chatbotApiURL))
                        {
                            s.CustomSources(chatbotApiURL);
                        }
                    })
                    .FormActions(s =>
                    {
                        if (formActionsSource != null)
                        {
                            s.Self().CustomSources(formActionsSource);
                        }
                        else
                        {
                            s.Self();
                        }
                    })
                    .BaseUris(s => s.Self())
                    .FrameAncestors(s => s.Self()));
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to SetCspHeader, Error:{ex.ToString()}");

            }
        }

        /// <summary>
        /// 读取配置文件中设置的CSPHeader的FormAction，多个FormAction用;分隔
        /// </summary>
        /// <returns></returns>
        private static string[] GetCustomFormActionsSource()
        {
            var formActionsStr = RMGlobalConfiguration.AppConfig[RMAppSettingKey.CSP_FORM_ACTION];
            return !string.IsNullOrEmpty(formActionsStr) ? formActionsStr.Split(';') : null;
        }

    }
}
