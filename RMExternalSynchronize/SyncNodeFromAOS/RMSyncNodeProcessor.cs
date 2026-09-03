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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.GraphApi.Tenant;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.O365Tenant;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Core.Synchronize.DbManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.SynchronizeDao;
using AvePoint.RA.DB.Dao.SynchronizeDao.Imp;
using AvePoint.RA.DB.Dao.SynchronizeRemoteNodeDao.Imp;
using AvePoint.RA.Service.JobMonitor;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.TeamsSetting;
using AvePoint.RA.Service.Services.Tenant.Upgrade;
using Cloud.Sdk.Data.AosModern;
using Newtonsoft.Json;
using RATeams;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using RMSynchronize.SyncNodeFromAOS.CheckLicense.CheckContentSourceLicense;
using RMSynchronize.SyncNodeFromAOS.CheckLicense.ContentSource;
using RMSynchronize.SyncNodeFromAOS.CheckLicense.ContentSourceInterface;
using RMSynchronize.SyncNodeFromAOS.Executors;
using System.Collections.Generic;
using System.Linq;

namespace RMSynchronize.SyncNodeFromAOS
{
    public class RMSyncNodeProcessor
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncNodeProcessor));

        private static readonly ITenantInfoDao s_tenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();

        private static readonly IJobMonitorService s_jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private static readonly IJobQueueService s_jobQueueService = PlatformWindsorManager.GetService<IJobQueueService>();

        private static readonly IRMAOSNotificationDao s_notificationDao = PlatformWindsorManager.GetService<IRMAOSNotificationDao>();

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly IRMSecurityContainerService s_securityContainerService = PlatformWindsorManager.GetService<IRMSecurityContainerService>();

        private static readonly ILicenseHelperService s_licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();

        private static readonly IRMTeamsSettingsService s_teamsSettingsService = PlatformWindsorManager.GetService<IRMTeamsSettingsService>();

        private static readonly IRMSyncNodeDao s_syncNodeDao = PlatformWindsorManager.GetService<IRMSyncNodeDao>();

        private static readonly IRMCache s_cache = PlatformWindsorManager.GetService<IRMCache>();

        private static readonly IKeyValueSqliteDao s_SqliteKeyValueDao = new KeyValueSqliteDao();
        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private const string S_TENANT_USER_SEAT_CACHE_KEY = "TENANT_USER_SEAT";

        private const string S_NEED_SYNCED_CONTENT_SOURCE = "Need_Synced_Content_Source";

        private const string S_CUSTOM_USER_SEATS = "CUSTOM_USER_SEATS";

        private const string S_TENANT_SUB_JOB_CONTROL_CACHE_KEY = "TENANT_SUB_JOB_CONTROL";

        public static async Task RunAsync(JobQueueMessage message)
        {
            try
            {
                var versionTicks = DateTime.UtcNow.Ticks;

                //var isInitNodesJob = string.Equals("true", message.Extension, StringComparison.OrdinalIgnoreCase);
                await RMSynchronizeDbManager.DownloadDatabaseAsync();

                var isInitNodesJob = false;

                RMSyncNodeJobManager.Init(message.JobId);

                ClearNotifications();

                var changeLogger = new RMSyncNodeAzureChangeLogger(!isInitNodesJob, message.JobId);

                var executors = await GetExecutors(changeLogger);

                await UpgradeNode(executors);

                foreach (var executor in executors)
                {
                    await executor.RunAsync();
                }

                //Update Sync Time To Sql Server
                await s_keyValueDao.UpsertAsync(RMSynchronizeDbManager.LastSyncTimeKey, versionTicks.ToString());

                //Update Sync Time to Sqlite
                await s_SqliteKeyValueDao.UpsertAsync(RMSynchronizeDbManager.LastSyncTimeKey, versionTicks.ToString());

                await AddTenantInfoToCache();

                await RMSynchronizeDbManager.SyncDatabaseToStorageAsync();

                RMSyncNodeJobManager.SetJobFinished();
                s_tenantInfoDao.UpdateSyncNodeState(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.Aos.Notification.RMInitNodeState.Synced);

                s_logger.Debug($"Succeed add change logs count: [{changeLogger.ChangeLogsCount}].");
                if (!isInitNodesJob && changeLogger.ChangeLogsCount > 0)
                {
                    var hasGControlGoogleLicense = await TenantService.HasInitGControlPlatForm();
                    if (!s_licenseHelperService.HasOpusILLicense && !s_licenseHelperService.HasOpusGoogleLicense && !hasGControlGoogleLicense)
                    {
                        s_logger.Error("No lifecycle license,can not run sync permission schedule job.");
                        changeLogger.DeleteLocalChangeLogFile();
                        return;
                    }
                    changeLogger.UploadChangeLogReport();
                    if (!SkipSyncSecurityContainerJob())
                    {
                        TryRunSyncSecurityContainerJob(message.JobId);
                    }
                }

                var hasUpgradeSettingTeams = s_keyValueDao.GetValueByKey(KeyNameCollection.HasUpgradeTeamsSettings);
                if (hasUpgradeSettingTeams != null && !bool.Parse(hasUpgradeSettingTeams.Value))
                {
                    s_teamsSettingsService.RunTeamsNodeSettingUpgradeJob();
                }

                var hasUpgradeTeamsData = s_keyValueDao.GetValueByKey(KeyNameCollection.HasUpgradeTeamsData);
                if (hasUpgradeTeamsData != null && !bool.Parse(hasUpgradeTeamsData.Value))
                {
                    s_teamsSettingsService.RunTeamsDataUpgradeJob();
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process sync node job. Error: {e}");
                RMSyncNodeJobManager.SetJobFailed(e.Message);
                s_tenantInfoDao.UpdateSyncNodeState(TenantLocalValue.LogonGroupId, AvePoint.RA.Contract.Aos.Notification.RMInitNodeState.SyncFailed);
            }
        }

        private static void TryRunSyncSecurityContainerJob(string jobId)
        {
            int count = s_jobQueueService.GetMessagesCount(TenantLocalValue.LogonGroupId, JobType.SyncSecurityContainer);
            if (count > 0)
            {
                s_logger.Info($"Tenant: {TenantLocalValue.LogonGroupId} has permission sync job in JobQueue.");
                return;
            }

            count = s_jobMonitorService.GetRunningJobsCount(JobType.SyncSecurityContainer);
            if (count > 0)
            {
                s_logger.Info($"Tenant: {TenantLocalValue.LogonGroupId} has running permission sync job.");
                return;
            }

            s_securityContainerService.RunScheduleJob(JobRunBy.Schedule, jobId);
        }
        private static async Task<List<TenantConnectionInfo>> CreateNewInfosByTenantId()
        {
            var tenantIdList = await s_syncNodeDao.GetTenantIdListFromDB();
            var tenantConnectionInfoList = new List<TenantConnectionInfo>();
            foreach (var tenant in tenantIdList)
            {
                var newInfo = new TenantConnectionInfo()
                {
                    Id = tenant,
                    Name = "Need_Delete_Info"
                };
                tenantConnectionInfoList.Add(newInfo);

            }
            return tenantConnectionInfoList;
        }
        private static async Task<List<RMSyncNodeExecutor>> GetExecutors(RMSyncNodeAzureChangeLogger changeLogger)
        {
            List<RMSyncNodeExecutor> executors = [];
            HashSet<Type> executorTypes = new();
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var o365Tenant = await client.TenantManagementService.GetByTypeAsync(Cloud.Sdk.Data.AosModern.PlatformType.Office365);
            var newInfos = await CreateNewInfosByTenantId();
            var o365Tenants = o365Tenant.Concat(newInfos).DistinctBy(x => x.Id).ToList();
            var googleTenants = await client.TenantManagementService.GetByTypeAsync(Cloud.Sdk.Data.AosModern.PlatformType.Google);
            var hasUpgradeTeams = s_keyValueDao.HasUpgradeTeams();
            var needSyncedContentSources = GetNeedSyncedContentSources();
            needSyncedContentSources.ForEach(needSyncedContentSource =>
            {
                if (needSyncedContentSource == SourceFlag.Teams && hasUpgradeTeams)
                {
                    s_logger.Info("Teams content source need process.");
                }
                List<RMSyncNodeExecutor> rmSyncNodeExecutors = needSyncedContentSource switch
                {
                    SourceFlag.SharePoint =>
                    hasUpgradeTeams ?
                    [
                        new RMSyncSharePointSiteNodeExecutor(client, o365Tenants, changeLogger),
                    ] :
                    [
                        new RMSyncSharePointSiteNodeExecutor(client, o365Tenants, changeLogger),
                        new RMSyncTeamGroupSiteNodeExecutor(client, o365Tenants, changeLogger),
                        new RMSyncChannelSiteNodeExecutor(client, o365Tenants, changeLogger),
                    ],
                    SourceFlag.Teams =>
                    hasUpgradeTeams ?
                    [
                        new RMSyncTeamsNodeExecutor(client, o365Tenants, changeLogger),
                        new RMSyncTeamsChannelSiteNodeExecutor(client, o365Tenants, changeLogger),
                    ] :
                    [],
                    SourceFlag.OneDrive =>
                    [
                        new RMSyncOneDriveSiteNodeExecutor(client, o365Tenants, changeLogger)
                    ],
                    SourceFlag.Exchange =>
                    [
                        new RMSyncExchangeNodeExecutor(client, o365Tenants, changeLogger)
                    ],
                    SourceFlag.Google =>
                    [
                        new RMSyncGoogleMyDriveContainerNodeExecutor(client, googleTenants, changeLogger),
                        new RMSyncGoogleSharedDriveContainerNodeExcutor(client, googleTenants, changeLogger),
                    ]
                };

                foreach (var executor in rmSyncNodeExecutors)
                {
                    if (executorTypes.Add(executor.GetType()))
                    {
                        executors.Add(executor);
                    }
                }
            });

            return executors;
        }

        private static async Task AddTenantInfoToCache()
        {
            try
            {
                var o365TenantIds = RMAosApiClient.GetO365TenantIds(TenantLocalValue.LogonGroupId);

                var o365TenantSubscribed = await o365TenantIds.ConvertAllAsync(async o365TenantId =>
                {
                    try
                    {
                        var tenantManager = new RMGraphTenantManager(o365TenantId);
                        var skus = await tenantManager.GetSharePointSubscribedSkusAsync();
                        var userSeats = skus.Sum(item => item.PrepaidUnits.Enabled);
                        s_logger.Info($"Tenant [{o365TenantId}] user seats [{userSeats}].");
                        return new RMO365TenantSubscribed
                        {
                            Id = o365TenantId,
                            UserSeats = userSeats,
                        };
                    }
                    catch (Exception e)
                    {
                        s_logger.Error($"An error occurred while calculate tenant [{o365TenantId}] subscribed skus. Error: {e}");
                    }

                    return new RMO365TenantSubscribed
                    {
                        Id = o365TenantId,
                        UserSeats = 0,
                    };
                });

                var setting = s_keyValueDao.GetValueByKey(S_CUSTOM_USER_SEATS);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    if (int.TryParse(setting.Value, out var customUserSeats))
                    {
                        s_logger.Info($"Used custom user seats [{customUserSeats}].");
                        o365TenantSubscribed.ForEach(item => item.UserSeats = customUserSeats);
                    }
                }

                await s_cache.SetListAsync(S_TENANT_USER_SEAT_CACHE_KEY, o365TenantSubscribed);
                await s_cache.KeyExpiredAsync(S_TENANT_USER_SEAT_CACHE_KEY, 60 * 60 * 24);
                await s_cache.RemoveAsync(S_TENANT_SUB_JOB_CONTROL_CACHE_KEY);
                s_logger.Info("Successful add o365 tenant subscribed to redis.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while add tenant info to cache. Error: {e}");
            }
        }

        private static List<SourceFlag> GetNeedSyncedContentSources()
        {
            ContentSource defaultContentSources = new DefaultContentSources();
            try
            {
                var setting = s_keyValueDao.GetValueByKey(S_NEED_SYNCED_CONTENT_SOURCE);
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    var contentSources = JsonConvert.DeserializeObject<List<SourceFlag>>(setting.Value);
                    if (contentSources?.Count > 0)
                    {
                        return contentSources;
                    }
                }

                defaultContentSources = new CheckGoogleLicense(defaultContentSources);
                defaultContentSources = new CheckM365License(defaultContentSources);
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while get need synced content source. Error: {e}");
            }

            return defaultContentSources.GetSourceFlags();
        }

        private static bool SkipSyncSecurityContainerJob()
        {
            bool skipSyncSecurityContainerJob = false;
            try
            {
                var setting = s_keyValueDao.GetValueByKey("SkipSyncSecurityContainerJob");
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    skipSyncSecurityContainerJob = Convert.ToBoolean(setting.Value);
                    s_logger.Info($"SkipSyncSecurityContainerJob:{skipSyncSecurityContainerJob}.");
                }
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while SkipSyncSecurityContainerJob. Error: {e}");
            }
            return skipSyncSecurityContainerJob;
        }

        private static void ClearNotifications()
        {
            try
            {
                s_notificationDao.DeleteAll(TenantLocalValue.LogonGroupId);
                s_logger.Info($"Successful clear notifications.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while clear notifications. Error: {e}");
            }
        }

        private static async Task UpgradeNode(List<RMSyncNodeExecutor> executors)
        {
            if (RMTenantUpgradeHelper.IsNeedUpgrade(TenantLocalValue.LogonGroupId, RMUpgradeFeature.SyncNode))
            {
                s_logger.Info("Begin to upgrade remote nodes.");
                try
                {
                    RMTenantUpgradeHelper.SetToUpgrading(TenantLocalValue.LogonGroupId);
                    if (!await s_syncNodeDao.HasAnySites())
                    {
                        s_logger.Info($"Current tenant {TenantLocalValue.LogonGroupId} no need to upgraded.");
                        RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.SyncNode, RMUpgradeStatus.Success);
                        return;
                    }

                    foreach (var executor in executors)
                    {
                        await executor.UpgradeAsync();
                    }

                    RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.SyncNode, RMUpgradeStatus.Success);
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while upgrade node. Error: {e}");
                    RMTenantUpgradeHelper.SetToFinish(TenantLocalValue.LogonGroupId, RMUpgradeFeature.SyncNode, RMUpgradeStatus.Failed);
                }
            }
        }
    }
}
