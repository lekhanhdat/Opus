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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Cloud.Sdk.Data.Aos.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant.Notification.Excutor
{
    public class UpgradeExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(UpgradeExecutor));

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();

        private static IRMMailboxDao MailboxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();

        private const string DefaultPrivateChannelSiteContainerId = "41cfe969-e07b-45cb-a7d0-b022f967e929";

        public static void Upgrade()
        {
            var needUpgrade = TenantService.NeedUpgradeRemoteNodeForAosId(TenantLocalValue.LogonGroupId);
            if (!needUpgrade)
            {
                return;
            }

            try
            {
                Logger.Info($"Start upgrade record containers for aos id.");
                ClearCache(TenantLocalValue.LogonGroupId);
                var containers = RMAosApiClient.GetTenantAllContainers(TenantLocalValue.LogonGroupId);
                containers.Remove(RemoteNodeType.Channel);
                containers.Add(RemoteNodeType.Channel,
                    new List<RemoteNode> {
                        new RemoteNode {
                            Id = DefaultPrivateChannelSiteContainerId,
                            Name = RMConstants.DefaultPrivateChannelSitesGroup
                        }
                    });

                UpgradeRemoteNodeContainer(containers);
                UpgradeMailboxContainer(containers);

                TenantService.UpdateContainersUpgradeStatusToSuccessful(TenantLocalValue.LogonGroupId);
                Logger.Info("Successfule upgrade record containers for aos id.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while upgarde for aos id. Error: {e}");
                throw e;
            }
        }

        private static void UpgradeRemoteNodeContainer(Dictionary<RemoteNodeType, List<RemoteNode>> containers)
        {
            Logger.Info($"Start upgrade remote node containers.");

            var remoteNodeContainers = containers.Where(item => item.Key == RemoteNodeType.OneDrive ||
                item.Key == RemoteNodeType.Office365Group ||
                item.Key == RemoteNodeType.Channel ||
                item.Key == RemoteNodeType.SiteCollection
            ).ToDictionary(item => ConvertRemoteNodeTypeToNodeLevelByRemoteNode(item.Key), item => item.Value);

            foreach (var containerValues in remoteNodeContainers.Values)
            {
                containerValues.ForEach(item =>
                {
                    if (string.Equals(item.Name, RMConstants.DEFAULT_O365_GROUP, StringComparison.InvariantCultureIgnoreCase))
                    {
                        item.Name = RMConstants.DEFAULT_O365_SITES_GROUP;
                    }
                });
            }

            var existContainers = RemoteNodeDao.GetAllContainers();
            foreach (var existContainer in existContainers)
            {
                if(remoteNodeContainers.TryGetValue((NodeLevel)existContainer.NodeLevel, out var containerList))
                {
                    var aosContainer = containerList.FirstOrDefault(item => item.Name == existContainer.Url);
                    if(aosContainer != null)
                    {
                        existContainer.AosId = aosContainer.Id;
                        Logger.Info($"Successful find container: [{existContainer.Url}] aos id.");
                    }
                }
            }

            RemoteNodeDao.UpdateContainers(existContainers);
            Logger.Info($"Successful upgrade remote node containers.");
        }

        private static void UpgradeMailboxContainer(Dictionary<RemoteNodeType, List<RemoteNode>> containers)
        {
            Logger.Info("Start upgrade mailbox containers.");
            var mailboxContainers = containers.Where(item => item.Key == RemoteNodeType.Mailbox ||
                item.Key == RemoteNodeType.Office365Group
            ).ToDictionary(item => ConvertRemoteNodeTypeToNodeLevelByMailbox(item.Key), item => item.Value);

            foreach(var containerValues in mailboxContainers.Values)
            {
                containerValues.ForEach(item =>
                {
                    if (string.Equals(item.Name, RMConstants.DEFAULT_O365_SITES_GROUP, StringComparison.InvariantCultureIgnoreCase))
                    {
                        item.Name = RMConstants.DEFAULT_O365_GROUPS_GROUP;
                    }
                });
            }

            var existContainers = MailboxDao.GetAllContainers();
            foreach (var existContainer in existContainers)
            {
                if (mailboxContainers.TryGetValue((NodeLevel)existContainer.NodeLevel, out var containerLists))
                {
                    var aosContainer = containerLists.FirstOrDefault(item => item.Name == existContainer.Name);
                    if(aosContainer != null)
                    {
                        existContainer.AosId = aosContainer.Id;
                        Logger.Info($"Successful find container: [{existContainer.Name}] aos id.");
                    }
                }
            }

            MailboxDao.UpdateContainers(existContainers);
            Logger.Info($"Successful upgrade mailbox containers.");
        }

        private static NodeLevel ConvertRemoteNodeTypeToNodeLevelByRemoteNode(RemoteNodeType nodeType)
        {
            if (nodeType == RemoteNodeType.OneDrive)
            {
                return NodeLevel.SkyDriveProGroup;
            }
            else if (nodeType == RemoteNodeType.Office365GroupSites || nodeType == RemoteNodeType.Office365Group)
            {
                return NodeLevel.O365GroupSitesGroup;
            }
            else if (nodeType == RemoteNodeType.Channel)
            {
                return NodeLevel.PrivateChannelGroup;
            }

            return NodeLevel.WebApplication;
        }

        private static NodeLevel ConvertRemoteNodeTypeToNodeLevelByMailbox(RemoteNodeType nodeType)
        {
            if (nodeType == RemoteNodeType.Office365GroupMailboxes ||
               nodeType == RemoteNodeType.Office365Group)
            {
                return NodeLevel.ExchangeOnlineO365GroupGroup;
            }
            else
            {
                return NodeLevel.ExchangeOnlineMailboxGroup;
            }
        }

        private static void ClearCache(string tenantGroupId)
        {
            Logger.Info("Begain to clear cache.");

            var mbKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.Mailbox);
            var pcKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.PrivateChannel);
            var rnKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.RemoteNode);

            if (RedisCacheService.CacheProvider.KeyExists(mbKey))
            {
                RedisCacheService.CacheProvider.KeyDel(mbKey);
            }
            if (RedisCacheService.CacheProvider.KeyExists(pcKey))
            {
                RedisCacheService.CacheProvider.KeyDel(pcKey);
            }
            if (RedisCacheService.CacheProvider.KeyExists(rnKey))
            {
                RedisCacheService.CacheProvider.KeyDel(rnKey);
            }

            Logger.Info("Finish to clear cache.");
        }
    }
}
