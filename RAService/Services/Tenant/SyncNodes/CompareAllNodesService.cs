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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;
using AOS_Sdk = Cloud.Sdk.Data.Aos.Tenant;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Cache.Services;
using System.Linq;
using Cloud.Sdk.Data.Aos.Tenant;
using AvePoint.RA.Common.SyncNode.Compatible;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes
{
    public class CompareAllNodesService
    {
        private static RALogger logger = RALogger.GetInstance(typeof(CompareAllNodesService));

        private IRMRemoteNodeService RemoteNodeService = PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        private IRMMailboxService EmailService = PlatformWindsorManager.GetService<IRMMailboxService>();
        private IRMDeleteRemoteSiteAspect DeleteRemoteSiteAspect = PlatformWindsorManager.GetService<IRMDeleteRemoteSiteAspect>();
        private ISyncRemoteNodeRedisService RemoteNodeRedisService = PlatformWindsorManager.GetService<ISyncRemoteNodeRedisService>();
        private ISyncMailboxRedisService MailboxRedisService = PlatformWindsorManager.GetService<ISyncMailboxRedisService>();
        private ISyncChannelRedisService ChannelRedisService = PlatformWindsorManager.GetService<ISyncChannelRedisService>();
        
        public void Compare(List<RMCompatibleRemoteNode> aosNodes, string tenantGroupId, string o365TenantGroupId)
        {
            var allNodesKeys = new HashSet<string>();
            foreach (var node in aosNodes)
            {
                if (node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
                {
                    allNodesKeys.Add(node.Name?.ToLowerInvariant());
                    allNodesKeys.Add(node.Url?.ToLowerInvariant());
                }
                else if(node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Mailbox ||
                    node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.SiteCollection ||
                    node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.OneDrive ||
                    node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.ProjectOnline ||
                    node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Channel ||
                    node.NodeType == Cloud.Sdk.Data.AosModern.RemoteNodeType.Office365Group)
                {
                    allNodesKeys.Add(node.Url?.ToLowerInvariant());
                }
            }
            allNodesKeys.Remove(null);
            allNodesKeys.Remove(string.Empty);

            CompareSiteCollection(allNodesKeys, tenantGroupId, o365TenantGroupId);
            CompareMailbox(allNodesKeys, tenantGroupId, o365TenantGroupId);
            ComparePrivateChannel(allNodesKeys, tenantGroupId, o365TenantGroupId);
        }


        private void CompareSiteCollection(HashSet<string> allNodesKeys, string tenantGroupId, string o365TenantGroupId)
        {
            try
            {
                var deleteItems = new List<string>();
                var allRemoteNodesDic = RedisCacheService.CacheProvider.HGetAll<SyncRemoteNodePara>(
                    RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.RemoteNode));
                foreach (var pair in allRemoteNodesDic)
                {
                    if (pair.Value.NodeLevel == GCommon.Contract.Tree.Object.NodeLevel.WebApplication
                       || pair.Value.NodeLevel == GCommon.Contract.Tree.Object.NodeLevel.SkyDriveProGroup
                       || pair.Value.NodeLevel == GCommon.Contract.Tree.Object.NodeLevel.O365GroupSitesGroup)
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(pair.Value?.TenantId) && string.Equals(pair.Value.TenantId, o365TenantGroupId))//只过滤当前tenant的站点
                    {
                        if (!allNodesKeys.Contains(pair.Key?.ToLowerInvariant()) 
                            && pair.Value.ScanSource != RemoteNodeScanSource.None)
                        { //此站点不在AOS中存在
                            deleteItems.Add(pair.Key);
                        }
                    }
                }
                DeleteRemoteNodeObjects(deleteItems);
            }
            catch (Exception e)
            {
                logger.Error("Compare remotenodes failed" + e.ToString());
            }
        }

        private void CompareMailbox(HashSet<string> allNodesKeys, string tenantGroupId, string o365TenantGroupId)
        {
            try
            {
                var deleteItems = new List<string>();
                var allMailboxsDic = RedisCacheService.CacheProvider.HGetAll<SyncRemoteNodePara>(RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.Mailbox));
                foreach (var pair in allMailboxsDic)
                {
                    if (pair.Value.NodeLevel == GCommon.Contract.Tree.Object.NodeLevel.ExchangeOnlineO365GroupGroup
                    || pair.Value.NodeLevel == GCommon.Contract.Tree.Object.NodeLevel.ExchangeOnlineMailboxGroup)
                    {
                        continue;
                    }
                    if (!string.IsNullOrEmpty(pair.Value?.TenantId) && string.Equals(pair.Value.TenantId, o365TenantGroupId))//只过滤当前tenant的站点
                    {
                        if (!allNodesKeys.Contains(pair.Key?.ToLowerInvariant())
                            && pair.Value.ScanSource != RemoteNodeScanSource.None)
                        {
                            deleteItems.Add(pair.Key);
                        }
                    }
                }
                DeleteMailboxObjects(deleteItems);
            }
            catch (Exception e)
            {
                logger.Error("Compare mailbox failed." + e.ToString());
            }
        }

        private void ComparePrivateChannel(HashSet<string> allNodesKeys, string tenantGroupId, string o365TenantGroupId)
        {
            try
            {
                var deleteItems = new List<string>();
                var allPricateChannelDic = RedisCacheService.CacheProvider.HGetAll<SyncRemoteNodePara>(RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.PrivateChannel));
                foreach (var pair in allPricateChannelDic)
                {
                    //if (pair.Value.NodeLevel == GCommon.Contract.Tree.Object.NodeLevel.PrivateChannelGroup)
                    //{
                    //    continue;
                    //}
                    if (!string.IsNullOrEmpty(pair.Value?.TenantId) && string.Equals(pair.Value.TenantId, o365TenantGroupId))//只过滤当前tenant的站点
                    {
                        if (!allNodesKeys.Contains(pair.Key?.ToLowerInvariant())
                            && pair.Value.ScanSource != RemoteNodeScanSource.None)
                        {
                            deleteItems.Add(pair.Key);
                        }
                    }
                }
                DeletePrivateChannelObjects(deleteItems);
            }
            catch (Exception e)
            {
                logger.Error("Compare private channel failed." + e.ToString());
            }
        }

        private void DeleteMailboxObjects(List<string> nodes)
        {
            logger.Info("CompareAllNodesService:delete mailbox object count: {0}.", nodes.Count);
            var deleteItems = new List<string>();
            nodes.ForEach(mail =>
            {
                deleteItems.Add(mail);
                if (deleteItems.Count == 200)
                {
                    LogDeleteItems(deleteItems, NodeType.EOMailBox);
                    MailboxRedisService.DeleteMailboxes(deleteItems, () =>
                    {
                        this.EmailService.DeleteMailboxByNames(deleteItems);
                    });
                    deleteItems.Clear();
                }
            });
            if (deleteItems.Count > 0)
            {
                LogDeleteItems(deleteItems, NodeType.EOMailBox);
                MailboxRedisService.DeleteMailboxes(deleteItems, () =>
                {
                    this.EmailService.DeleteMailboxByNames(deleteItems);
                });
            }
            logger.Info("CompareAllNodesService:delete mailbox objects successful.");
        }

        private void DeleteRemoteNodeObjects(List<string> nodes)
        {
            logger.Info("CompareAllNodesService:delete remotenode object count: {0}.", nodes.Count);
            var deleteItems = new List<string>();
            nodes.ForEach(site =>
            {
                deleteItems.Add(site);
                if (deleteItems.Count == 200)
                {
                    LogDeleteItems(deleteItems, NodeType.SharePointSites);
                    RemoteNodeRedisService.DeleteRemoteNodes(deleteItems, () =>
                    {
                        this.RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                        this.DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                    });
                    deleteItems.Clear();
                }
            });
            if (deleteItems.Count > 0)
            {
                LogDeleteItems(deleteItems, NodeType.SharePointSites);
                RemoteNodeRedisService.DeleteRemoteNodes(deleteItems, () =>
                {
                    this.RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    this.DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                });
            }
            logger.Info("CompareAllNodesService:delete remotenode objects successful.");
        }

        private void DeletePrivateChannelObjects(List<string> nodes)
        {
            logger.Info("CompareAllNodesService:delete private channel object count: {0}.", nodes.Count);
            var deleteItems = new List<string>();
            nodes.ForEach(site =>
            {
                deleteItems.Add(site);
                if (deleteItems.Count == 200)
                {
                    LogDeleteItems(deleteItems, NodeType.PrivateChannelSites);
                    ChannelRedisService.DeletePrivateChannels(deleteItems, () =>
                    {
                        this.RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                        this.DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                    });
                    deleteItems.Clear();
                }
            });
            if (deleteItems.Count > 0)
            {
                LogDeleteItems(deleteItems, NodeType.PrivateChannelSites);
                ChannelRedisService.DeletePrivateChannels(deleteItems, () =>
                {
                    this.RemoteNodeService.DeleteRemoteSiteCollectionsByUrl(deleteItems);
                    this.DeleteRemoteSiteAspect.DeleteRelatedDataByUrl(deleteItems);
                });
            }
            logger.Info("CompareAllNodesService:delete private channel objects successful.");
        }

        private void LogDeleteItems(List<string> items, NodeType nodeType)
        {
            logger.Info($"Current nodeType is {nodeType}");
            logger.Info(string.Join(",", items));
        }
    }
}
