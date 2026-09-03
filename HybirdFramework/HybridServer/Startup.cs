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
using AvePoint.RA.Contract.Common;
using AvePoint.RA.RedisCache;
using CommonModel.MethodInfo;
using HybridProxy;
using HybridServer.Configuration;
using HybridServer.EF;
using HybridServer.Hubs;
using HybridServer.Log;
using HybridServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HybridServer
{
    public class Startup
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(Startup));
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var clientTimeoutInterval = Configuration.GetValue<int>(ConfigKey.SIGNALR_CLIENT_TIMEOUT_INTERVAL);
            var handshakeTimeout = Configuration.GetValue<int>(ConfigKey.SIGNALR_HAND_SHAKE_TIMEOUT);
            var keepAliveInterval = Configuration.GetValue<int>(ConfigKey.SIGNALR_KEEP_ALIVE_INTERVAL);
            var maximumReceiveMessageSize = Configuration.GetValue<int>(ConfigKey.SIGNALR_MAX_RECEIVE_MESSAGE_SIZE);
            var enableDetailedErrors = Configuration.GetValue<bool>(ConfigKey.SIGNALR_ENABLE_DETAILED_ERRORS);
            var isDevEnv = !GlobalConfiguration.IsProduction;
            var isGCPEnv = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(Configuration.GetValue<string>(ConfigKey.ENVIRONMENT_NAME)?.ToLower());

            services.AddSignalR(opt=> {
                opt.ClientTimeoutInterval = TimeSpan.FromSeconds(clientTimeoutInterval);
                opt.HandshakeTimeout = TimeSpan.FromSeconds(handshakeTimeout);
                opt.KeepAliveInterval = TimeSpan.FromSeconds(keepAliveInterval);
                opt.MaximumReceiveMessageSize = maximumReceiveMessageSize * 1024;
                opt.EnableDetailedErrors = enableDetailedErrors;
            })//.AddStackExchangeRedis(Configuration.GetValue<string>("RedisConnection"),option=> {
            //    option.Configuration.ChannelPrefix = "SignalR";
            //});
             .AddStackExchangeRedis(o =>
             {
                 o.ConnectionFactory = async writer =>
                 {
                     
                      var connection = await RedisConnectionFactory.ConnectAsync(GlobalConfiguration.RedisConn, isGCPEnv, isDevEnv);

                     connection.ConnectionFailed += (_, e) =>
                     { 
                         logger.Error("Connection to Redis failed.");
                     };

                     if (!connection.IsConnected)
                     {
                         logger.Error("Did not connect to Redis.");
                         throw new Exception("Did not connect to Redis.");
                     }
                     return connection;
                 };
             });

            //auth configuration
            ConfigureAuth(services);
            var dbConnStr = Configuration.GetValue(ConfigKey.SIGNALR_DB_CONNECTION_STRING);
            logger.Debug($"SignalR DB Connection String:");
            try
            {
                SqlConnection conn = new SqlConnection(dbConnStr);
                conn.Open();
                conn.Close();
            }
            catch (Exception ex)
            {
                logger.Debug($"SignalR DB Connection String incorrect: {ex}");
            }

            services.AddDbContext<SignalRRDBContext>(options => options.UseSqlServer(dbConnStr));
            services.AddTransient<SignalRRepository, SignalRRepository>();

            services.AddMemoryCache();

            services.AddScoped<ConnectionManagementService, ConnectionManagementService>();
             services.AddSingleton<RedisCacheService, RedisCacheService>(sp => { return new RedisCacheService(GlobalConfiguration.RedisConn, isGCPEnv, isDevEnv); });
            services.AddSingleton<CacheManagementService, CacheManagementService>();
            services.AddSingleton<IInMemoryHeartbeatQueue, InMemoryHeartbeatQueue>();
            services.AddHostedService<HeartbeatProcessorService>();
            //services.AddScoped<HybridServerHub, HybridServerHub>();

        }

        private void ConfigureAuth(IServiceCollection services)
        {
            var authBuilder = services.AddAuthentication("Bearer");

            authBuilder
                .AddJwtBearer("Bearer", options =>
                {
                    options.ForwardDefaultSelector = GetSchema;
                })
                .AddScheme<JwtBearerOptions, JwtBearerHandler>(ProxyConstants.Token_Source_Public, options =>
                {
                    //"https://identity-public.sharepointguild.com";
                    options.Authority = Configuration.GetValue(ConfigKey.PUBLIC_IDENTITY_SERVICE_URL);
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        AuthenticationType = "IdentityServer",
                        ValidateAudience = true,
                        SignatureValidator = (string token, TokenValidationParameters parameters) =>
                        {
                            var jwt = new JsonWebToken(token);
                            return jwt;
                        },
                        //"https://graph-public.sharepointguild.com/records"
                        ValidAudience = Configuration.GetValue(ConfigKey.PUBLIC_AUDIENCE_URL),
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = false
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = GetHeaderValue(context.Request.Headers, "Authorization");
                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs/HybridServerHub")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },

                        OnForbidden = ctx =>
                        {
                            ctx.HttpContext.Items["ErrorMessage"] = "On token validate forbidden";
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = ctx =>
                        {
                            ctx.HttpContext.Items["ErrorMessage"] = ctx.Exception.Message;
                            var logger = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>().CreateLogger("IdentityServer");
                            logger.LogError(ctx.Exception, ctx.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddScheme<JwtBearerOptions, JwtBearerHandler>(ProxyConstants.Token_Source_Internal, options => 
                {
                    //"https://identity.sharepointguild.com";
                    options.Authority = Configuration.GetValue(ConfigKey.IDENTITY_SERVICE_URL);
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        AuthenticationType = "IdentityServer",
                        ValidateAudience = true,
                        SignatureValidator = (string token, TokenValidationParameters parameters) =>
                        {
                            var jwt = new JsonWebToken(token);
                            return jwt;
                        },
                        //"https://graph.avepoint.internal/records"
                        ValidAudience = Configuration.GetValue(ConfigKey.AUDIENCE_URL),
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = false
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            //var accessToken;
                            var accessToken = GetHeaderValue(context.Request.Headers, "Authorization");
                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/hubs/HybridServerHub")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },

                        OnForbidden = ctx =>
                        {
                            ctx.HttpContext.Items["ErrorMessage"] = "On token validate forbidden";
                            return Task.CompletedTask;
                        },

                        OnAuthenticationFailed = ctx =>
                        {
                            ctx.HttpContext.Items["ErrorMessage"] = ctx.Exception.Message;
                            var logger = ctx.HttpContext.RequestServices.GetService<ILoggerFactory>().CreateLogger("IdentityServer");
                            logger.LogError(ctx.Exception, ctx.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };

                });


            services.AddAuthorization(options =>
            {
                options.AddPolicy(APIScope.Manager, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", APIScope.Manager);
                });

                options.AddPolicy(APIScope.Agent, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", APIScope.Agent);
                });

                options.AddPolicy(APIScope.Common, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", APIScope.Common);
                });
            });
        }

        private static string GetSchema(HttpContext context)
        {
            var token = GetHeaderValue(context.Request.Headers, "Authorization");
            if (!string.IsNullOrEmpty(token) && token.ToString().StartsWith("Bearer"))
            {
                return GetHeaderValue(context.Request.Headers, "Token-Source");
            }

            return null;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();


            #region ensure create db

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<SignalRRDBContext>();
                context.Database.EnsureCreated();
            }

            #endregion

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                endpoints.MapGet("/healthz", async context =>
                {
                    await context.Response.WriteAsync("ok");
                });
                endpoints.MapHub<HybridServerHub>("/HybridServerHub");
            });
           

            SetupTimerJob(app);
        }

        private void SetupTimerJob(IApplicationBuilder app)
        {
            //long running timer job
            Task timerjob = new Task(() =>
            {
                using (var timerScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    var repository = timerScope.ServiceProvider.GetRequiredService<SignalRRepository>();
                    var cacheService = timerScope.ServiceProvider.GetRequiredService<CacheManagementService>();
                    var _connectionService = timerScope.ServiceProvider.GetRequiredService<ConnectionManagementService>();
                    var hubcontext = timerScope.ServiceProvider.GetRequiredService<IHubContext<HybridServerHub>>();
                    do
                    {
                        try
                        {
                            cacheService.ClearManager().Wait();
                            logger.Info("not active manager cleared");
                            var needtoNotify = repository.EnsureAgentConnectionStatus().Result;
                            if (needtoNotify)
                            {
                                this.logger.Info("Send notification to all managers");
                                //notification every manager we have new agents here! welcome!
                                var connectionIds = _connectionService.GetManagerConnectionId();
                                hubcontext.Clients.Clients(connectionIds).SendAsync(HubMethodNames.AgentConnectionNotification);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Warn("error occured during time job:" + e.ToString());
                        }
                        finally
                        {
                            Thread.Sleep(20000);
                        }
                    }
                    while (true);
                }

            }, TaskCreationOptions.LongRunning);
            timerjob.Start();
        }

        private static string GetHeaderValue(IHeaderDictionary headers, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (headers.TryGetValue(key, out var headerVal))
            {
                return headerVal;
            }

            var lowerCaseKey = key.ToLower();
            if (headers.TryGetValue(lowerCaseKey, out headerVal))
            {
                return headerVal;
            }

            return null;
        }
    }
}
