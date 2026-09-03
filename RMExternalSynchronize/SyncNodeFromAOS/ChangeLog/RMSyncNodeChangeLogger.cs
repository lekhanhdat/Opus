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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.SyncNode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;

namespace RMSynchronize.SyncNodeFromAOS.ChangeLog
{
    public class RMSyncNodeChangeLogger
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncNodeChangeLogger));

        private static readonly IRMCache s_cache = PlatformWindsorManager.GetService<IRMCache>();

        private readonly bool _enable;

        public int ChangeLogsCount { get; private set; }

        public RMSyncNodeChangeLogger(bool enable)
        {
            _enable = enable;
        }

        public async Task Record(IEnumerable<RMContainerInfoAdaption> containerInfoes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            foreach (var containerInfo in containerInfoes)
            {
                await Record(containerInfo, contentSource, changeType);
            }
        }

        public async Task Record(RMContainerInfoAdaption containerInfo, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            try
            {
                var changeInfo = new RMSyncNodeChangeInfo
                {
                    Id = containerInfo.Id,
                    Url = containerInfo.Name,
                    ChangeType = changeType,
                    IsContainer = true,
                };

                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[contentSource], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{contentSource}] container [{containerInfo.Id} - {containerInfo.Name}] [{changeType}] change log. Error: {e}");
            }
        }

        public async Task RecordChangeName(RMContainerInfoAdaption containerInfo, SourceFlag contentSource, string beforeUrl, string changedUrl)
        {
            try
            {
                if (!_enable)
                {
                    return;
                }

                var changeInfo = new RMSyncNodeChangeInfo
                {
                    Id = containerInfo.Id,
                    BeforeUrl = beforeUrl,
                    Url = changedUrl,
                    ChangeType = RMSyncNodeChangeType.ChangeName,
                    IsContainer = true,
                };

                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[contentSource], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{contentSource}] container [{containerInfo.Id} - {containerInfo.Name}] [{RMSyncNodeChangeType.ChangeName}] change log. Error: {e}");
            }
        }

        public async Task Record(IEnumerable<RMSiteNodeAdaption> nodes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            foreach (var node in nodes)
            {
                await Record(node, contentSource, changeType);
            }
        }
        
        public async Task Record(IEnumerable<RMGoogleNodeAdaption> nodes, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            foreach (var node in nodes)
            {
                await Record(node, contentSource, changeType);
            }
        }

        public async Task Record(RMSiteNodeAdaption node, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            try
            {
                var changeInfo = new RMSyncNodeChangeInfo
                {
                    Id = node.Id,
                    Url = node.Url,
                    AosId = node.ObjectId,
                    ChangeType = changeType,
                    NodeLevel = node.NodeLevel,
                    ContainerId = node.ContainerId,
                    IsContainer = false,
                    O365TenantId = node.TenantId,
                    ContainerName = node.ContainerName
                };
                if (contentSource == SourceFlag.Teams)
                {
                    try
                    {
                        changeInfo.RealId = new Guid(node.TeamId);
                    }
                    catch (Exception e)
                    {
                        s_logger.Warn($"An error occurred while teams record [{contentSource}] container [{node.Id} - {node.Url}] [{changeType}] change log. Error: {e}");
                    }
                }
                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[contentSource], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{contentSource}] container [{node.Id} - {node.Url}] [{changeType}] change log. Error: {e}");
            }
        }
        
        public async Task Record(RMGoogleNodeAdaption node, SourceFlag contentSource, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            try
            {
                var changeInfo = new RMSyncNodeChangeInfo
                {
                    Id = node.Id,
                    Url = node.Name,
                    AosId = node.ObjectId,
                    ChangeType = changeType,
                    NodeLevel = node.NodeLevel,
                    ContainerId = node.ContainerId,
                    IsContainer = false,
                    O365TenantId = node.TenantId,
                    ContainerName = node.ContainerName,
                };

                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[contentSource], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{contentSource}] container [{node.Id}] [{changeType}] change log. Error: {e}");
            }
        }

        public async Task RecordChangeName(RMSiteNodeAdaption node, SourceFlag contentSource, string beforeUrl, string changedUrl)
        {

            try
            {
                if (!_enable)
                {
                    return;
                }

                var changeInfo = new RMSyncNodeChangeInfo
                {
                    Id = node.Id,
                    BeforeUrl = beforeUrl,
                    Url = changedUrl,
                    AosId = node.ObjectId,
                    ChangeType = RMSyncNodeChangeType.ChangeName,
                    NodeLevel = node.NodeLevel,
                    ContainerId = node.ContainerId,
                    IsContainer = false,
                    O365TenantId = node.TenantId,
                    ContainerName = node.ContainerName,
                };

                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[contentSource], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{contentSource}] container [{node.Id} - {node.Url}] [{RMSyncNodeChangeType.ChangeName}] change log. Error: {e}");
            }
        }
        
        public async Task RecordChangeName(RMGoogleNodeAdaption node, SourceFlag contentSource, string beforeName, string changedName)
        {
            try
            {
                if (!_enable)
                {
                    return;
                }

                var changeInfo = new RMSyncNodeChangeInfo()
                {
                    Id = node.Id,
                    BeforeUrl = beforeName,
                    Url = changedName,
                    AosId = node.ObjectId,
                    ChangeType = RMSyncNodeChangeType.ChangeName,
                    NodeLevel = node.NodeLevel,
                    ContainerId = node.ContainerId,
                    IsContainer = false,
                    O365TenantId = node.TenantId,
                    ContainerName = node.ContainerName,
                };

                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[contentSource], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{contentSource}] container [{node.Id} ] [{RMSyncNodeChangeType.ChangeName}] change log. Error: {e}");
            }
        }

        public async Task Record(IEnumerable<RMExchangeNodeAdaption> nodes, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            foreach (var node in nodes)
            {
                await Record(node, changeType);
            }
        }

        public async Task Record(RMExchangeNodeAdaption node, RMSyncNodeChangeType changeType)
        {
            if (!_enable)
            {
                return;
            }

            try
            {
                var changeInfo = new RMSyncNodeChangeInfo
                {
                    Id = node.Id,
                    Url = node.EmailAddress,
                    AosId = node.ObjectId,
                    ChangeType = changeType,
                    NodeLevel = node.NodeLevel,
                    ContainerId = node.ContainerId,
                    IsContainer = false,
                    O365TenantId = node.TenantId,
                    ContainerName = node.ContainerName,
                };

                await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[SourceFlag.Exchange], changeInfo);

                ChangeLogsCount++;
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while record [{SourceFlag.Exchange}] container [{node.Id} - {node.EmailAddress}] [{changeType}] change log. Error: {e}");
            }
        }

        public async Task RecordChangeName(RMExchangeNodeAdaption node, string beforeUrl, string changedUrl)
        {
            if (!_enable)
            {
                return;
            }

            var changeInfo = new RMSyncNodeChangeInfo
            {
                Id = node.Id,
                BeforeUrl = beforeUrl,
                Url = changedUrl,
                AosId = node.ObjectId,
                ChangeType = RMSyncNodeChangeType.ChangeName,
                NodeLevel = node.NodeLevel,
                ContainerId = node.ContainerId,
                IsContainer = false,
                O365TenantId = node.TenantId,
                ContainerName = node.ContainerName,
            };

            await s_cache.ListAddAsync(RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_CACHE_KEY + RMSyncNodeChangeTypeConstants.SYNC_NODE_CHANGE_LOG_SOURCE_CACHE_KEY[SourceFlag.Exchange], changeInfo);
            ChangeLogsCount++;
        }
    }
}
