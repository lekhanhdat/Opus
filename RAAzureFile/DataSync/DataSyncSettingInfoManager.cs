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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AzureFileShare;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAAzureFile.DataSync
{
    public class DataSyncSettingInfoManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DataSyncSettingInfoManager));

        private static readonly IRMAzureFileShareConnectionService AzureFileShareConnectionService =
            PlatformWindsorManager.GetService<IRMAzureFileShareConnectionService>();

        private static readonly IRMAzureFileSettingsService AzureFileShareSettingService =
            PlatformWindsorManager.GetService<IRMAzureFileSettingsService>();

        private static readonly Dictionary<string, AzureFileSettingDto> SettingInfoCache = new Dictionary<string, AzureFileSettingDto>();

        public static async Task InitializationAsync(AzureFileShareApiContext apiContext, AzureFileShareApiDirectoryClient directoryClient)
        {
            directoryClient.LoadProperties();
            while(!directoryClient.IsRoot)
            {

                (var hasSetting_a, var directorySettingInfo) = await AzureFileShareSettingService.TryGetSettingInfoAsync(directoryClient.Id);
                if (hasSetting_a)
                {
                    SettingInfoCache[directoryClient.FullPath] = directorySettingInfo;
                }

                directoryClient = directoryClient.Parent;
                directoryClient.LoadProperties();
            }

            (var hasSetting_b, var settingInfo) = await AzureFileShareSettingService.TryGetSettingInfoAsync(apiContext.ConnectionInfo.ConnectionId);
            if (hasSetting_b)
            {
                SettingInfoCache[apiContext.ConnectionFullUrl] = settingInfo;
                return;
            }

            var connectionInfo = await AzureFileShareConnectionService.GetAsync(apiContext.ConnectionInfo.ConnectionId);
            (hasSetting_b, settingInfo) = await AzureFileShareSettingService.TryGetSettingInfoAsync(connectionInfo.ConnectionGroupId);

            if (hasSetting_b)
            {
                SettingInfoCache[apiContext.ConnectionFullUrl] = settingInfo;
                return;
            }



            throw new Exception($"Can't loaded setting for connection: [{apiContext.ConnectionInfo.ConnectionId}].");
        }

        public static async Task<AzureFileSettingDto> LoadSettingInfoAsync(AzureFileShareApiDirectoryClient directoryClient)
        {
            using (new PerformanceScope("AzureFileShare:DataSync:LoadSettingInfo", "", true))
            {
                if (!directoryClient.IsLoadedProperties)
                {
                    directoryClient.LoadProperties();
                }

                if (directoryClient.IsRoot)
                {
                    var rootSettingKey = SettingInfoCache.Keys.Where(item => directoryClient.FullPath.StartsWith(item)).OrderByDescending(item => item.Length).First();
                    return SettingInfoCache[rootSettingKey];
                }
                (var hasSetting, var settingInfo) = await AzureFileShareSettingService.TryGetSettingInfoAsync(directoryClient.Id);
                if (hasSetting)
                {
                    SettingInfoCache[directoryClient.FullPath] = settingInfo;
                    return settingInfo;
                }

                var settingKey = SettingInfoCache.Keys.Where(item => directoryClient.FullPath.StartsWith(item)).OrderByDescending(item => item.Length).First();
                return SettingInfoCache[settingKey];
            }
        }

        public static async Task ResetSettingInfoAsync(AzureFileShareApiDirectoryClient directoryClient)
        {
            try
            {
                var settingKey = SettingInfoCache.Keys.Where(item => directoryClient.FullPath.StartsWith(item)).OrderByDescending(item => item.Length).First();
                var settingInfo = SettingInfoCache[settingKey];
                if(settingInfo.ScopeId == directoryClient.Id)
                {
                    Logger.Info($"Current azure file node [{directoryClient.Id}] has settings , Reset it.");
                    await AzureFileShareSettingService.ResetSyncSettingAsync(directoryClient.Id);
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while reset azure file node [{directoryClient.Id}] sync setting info. Error: {e}");
            }
        }
    }
}
