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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Telemetry.Generator;
using Cloud.Sdk.Telemetry;
using Cloud.Sdk.Telemetry.Data;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Telemetry
{
    public class TelemetryContext
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(TelemetryContext));

        private static readonly Dictionary<TelemetryModule, TelemetryGenerator> TelemetryModuleGenerators = new Dictionary<TelemetryModule, TelemetryGenerator>();

        private static readonly ICloudTelemetryClient TelemetryClient;

        static TelemetryContext()
        {
            TelemetryClient = InitCloudTelemetrySdk();
            InitTelemetryGenerators();
        }

        public static async Task FlushAsync()
        {
            try
            {
                if(TelemetryClient != null)
                {
                    await TelemetryClient.Flush();
                    Logger.Info($"Telemetry records were flushed to cloud telemetry");
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while flushing telemetry record to cloud telemetry. Error: {e}");
            }
        }

        public static void SendToQueue(TelemetryModule module, TelemetryEventType eventType, IList<object> args = null)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            var username = TenantLocalValue.LogonUserEmail;
            SendToQueue(tenantId, username, module, eventType, args);
        }

        private static void SendToQueue(string tenantId, string username, TelemetryModule module, TelemetryEventType eventType, IList<object> args)
        {
            try
            {
                TenantUtil.RunUnderTenant(tenantId, username, () =>
                {
                    if(!TelemetryModuleGenerators.TryGetValue(module, out var telemetryGenerator))
                    {
                        Logger.Warn($"Skipped, The [{module}] can't find telemetry generator.");
                        return;
                    }

                    var record = telemetryGenerator.GenerateTelemetryRecord(eventType, args);

                    if (record == null)
                    {
                        Logger.Warn($"Skipped, The [{module}] excute [{eventType}] by tenant: [{tenantId}] create telemetry record failed.");
                        return;
                    }

                    if (TelemetryClient == null)
                    {
                        Logger.Warn($"Skipped, The [{module}] excute [{eventType}] by tenant: [{tenantId}] get cloud telemetry client failed.");
                        return;
                    }

                    Logger.Info($"Sending telemetry record to cloud telemetry. [{module} - {eventType}] by tenant: [{tenantId}]");
                    TelemetryClient.Add(record);
                });
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while send [{module} - {eventType}] by tenant: [{tenantId}] telemetry record to cloud telemetry. Error: {e}");
            }
        }

        private static ICloudTelemetryClient InitCloudTelemetrySdk()
        {
            try
            {
                Logger.Info("Init cloud telemetry sdk.");
                var connStr = RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.TELEMETRY_CONNECTION_STRING];
                if(string.IsNullOrEmpty(connStr))
                {
                    Logger.Warn("Not configured cloud telemetry connection string.");
                    return null;
                }
                var service = new ServiceCollection();
                service.AddCloudTelemetry(Product.CloudRecords, RMGlobalConfiguration.EncryptConfig[RMCommonSettingKey.TELEMETRY_CONNECTION_STRING]);
                service.UseCustomizedLoggerInstance(new TelemetryLogger());
                return service.BuildServiceProvider().GetService<ICloudTelemetryClient>();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while init cloud telemetry sdk. Error: {e}");
                return null;
            }
        }

        private static void InitTelemetryGenerators()
        {
            try
            {
                var telemetryGeneratorType = typeof(TelemetryGenerator);
                var assembly = Assembly.GetAssembly(telemetryGeneratorType);
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract) continue;
                    if (type.BaseType?.Name == telemetryGeneratorType.Name)
                    {
                        var instance = Activator.CreateInstance(type) as TelemetryGenerator;
                        TelemetryModuleGenerators.Add(instance.Module, instance);
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while init telemetry generators. Error: {e}");
            }
        }
    }
}
