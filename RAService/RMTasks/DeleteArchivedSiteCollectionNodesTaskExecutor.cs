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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class DeleteArchivedSiteCollectionNodesTaskExecutor : ITaskExecutor
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DeleteArchivedSiteCollectionNodesTaskExecutor));

        private static readonly string RedisKey = "LastScanArchivedSCTime";

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private static IRMDeleteRemoteSiteAspect DeleteRemoteSiteAspect => PlatformWindsorManager.GetService<IRMDeleteRemoteSiteAspect>();

        private static ISyncRemoteNodeRedisService RemoteNodeRedisService => PlatformWindsorManager.GetService<ISyncRemoteNodeRedisService>();

        private static ISyncChannelRedisService ChannelRedisService => PlatformWindsorManager.GetService<ISyncChannelRedisService>();

        private static readonly TimeSpan DefaultForwardTime = TimeSpan.FromDays(1);

        private long DefaultLastExecuteTime;


        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                DefaultLastExecuteTime = (DateTime.UtcNow - DefaultForwardTime).Ticks;
                var tenants = TenantService.GetAllAvailableTenantInfo();
                Logger.Info($"Total of {tenants} tenant need to be execute delete archived site collection.");
                tenants.ForEach(ExecuteDeleteArchivedSiteCollection);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while execute delete archived site collection nodes taks. Error: {e}");
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        private void ExecuteDeleteArchivedSiteCollection(TenantInfoDto tenant)
        {
            Logger.Info($"Tenant: [{tenant.TenantId}] begin execute delete archived site collection.");
            TenantUtil.RunUnderTenant(tenant.TenantId, tenant.RegisterEmail, () =>
            {
                try
                {
                    var lastExecuteTime = GetTenantLastExecuteTime(tenant.TenantId);
                    var currentExecuteTime = DateTime.UtcNow.Ticks;
                    var scUrls = GetArchivedSiteCollections(lastExecuteTime, currentExecuteTime);
                    var remoteSCs = RemoteNodeService.GetRemoteSiteCollectionBySiteUrls(scUrls);
                    Logger.Info($"Get remote site collection by site urls count: [{remoteSCs.Count}].");
                    var remoteNodes = remoteSCs.FindAll(item => item.NodeType != RemoveNodeType.PrivateChannel).Select(item => item.url).ToList();
                    var privateChannelNodes = remoteSCs.FindAll(item => item.NodeType == RemoveNodeType.PrivateChannel).Select(item => item.url).ToList();
                    DeleteRemoteNodeObjects(remoteNodes);
                    DeletePrivateChannelObjects(privateChannelNodes);
                    SetTenantLastExecuteTime(tenant.TenantId, currentExecuteTime);
                }
                catch(Exception e)
                {
                    Logger.Error($"An error occurred while delete archived site collection by tenant: [{tenant.TenantId}]. Error: {e}");
                }
            });
            Logger.Info($"Tenant: [{tenant.TenantId}] end execute delete archived site collection.");
        }
        
        private List<string> GetArchivedSiteCollections(long lastExecuteTime, long currentExecuteTime)
        {
            var exploreDao = new ExplorerDao();
            Logger.Info($"Begin get archived site collection between [{lastExecuteTime}] to [{currentExecuteTime}] by tenant: [{TenantLocalValue.LogonGroupId}].");
            var res = exploreDao.QueryAll(item =>
                        item.DestroyedTime >= lastExecuteTime &&
                        item.DestroyedTime <= currentExecuteTime &&
                        item.RecordStatus == (int)RMRecordStatus.Destroyed &&
                        item.NodeType == (int)RMNodeLevel.SiteCollection).Select(item => item.DirPath).ToList();
            Logger.Info($"End get archived site collection: [{res.Count}]");
            return res;
        }

        private void DeleteRemoteNodeObjects(List<string> nodes)
        {
            Logger.Info($"Delete remotenode object count: [{ nodes.Count}].");
            DatabaseUtility.BatchOperation(nodes, (batchItems) => {
                var deleteItems = batchItems.ToList();
                RemoteNodeRedisService.DeleteRemoteNodes(deleteItems, () =>
                {
                    RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                });
            });
            Logger.Info("Delete remotenode objects successful.");
        }

        private void DeletePrivateChannelObjects(List<string> nodes)
        {
            Logger.Info($"Delete private channel object count: [{nodes.Count}].");
            DatabaseUtility.BatchOperation(nodes, (batchItems) => {
                var deleteItems = batchItems.ToList();
                ChannelRedisService.DeletePrivateChannels(deleteItems, () =>
                {
                    RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                });
            });
            Logger.Info("Delete private channel objects successful.");
        }

        private long GetTenantLastExecuteTime(string tenantId)
        {
            try
            {
                var ticks = RedisCacheService.CacheProvider.HGet(RedisKey, tenantId);
                if(!string.IsNullOrEmpty(ticks))
                {
                    Logger.Info($"Get tenant: [{tenantId}] last execute time: {ticks}.");
                    return Convert.ToInt64(ticks);
                }
                Logger.Warn($"Redis does not exist tenant: [{tenantId}] record.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get tenant last execute time. Error: {e}");
            }
            return DefaultLastExecuteTime;
        }

        private void SetTenantLastExecuteTime(string tenantId, long ticks)
        {
            try
            {
                RedisCacheService.CacheProvider.HSet(RedisKey, tenantId, ticks.ToString());
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while set tenant: [{tenantId}] last execute time: [{ticks}]. Error: {e}");
            }
        }
    }
}
