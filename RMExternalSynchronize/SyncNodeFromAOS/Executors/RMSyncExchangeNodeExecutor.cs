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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using Cloud.Sdk.AosModern;
using Cloud.Sdk.Data.AosModern;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS.Executors
{
    public class RMSyncExchangeNodeExecutor : RMSyncNodeExecutor
    {
        public RMSyncExchangeNodeExecutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfoes, RMSyncNodeAzureChangeLogger changeLogger) 
            : base(tenantClient, tenantConnectionInfoes, changeLogger)
        {
        }

        public override SourceFlag ContentSource => SourceFlag.Exchange;

        public override RemoteNodeType AosNodeType => RemoteNodeType.Mailbox;

        protected override NodeLevel RecordContainerNodeLevel => NodeLevel.ExchangeOnlineMailboxGroup;

        protected override async Task<IEnumerable<RMContainerInfoAdaption>> GetRecordContainers()
        {
            return await s_syncNodeDao.GetExchangeContainersAsync();
        }

        protected override Task AddContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            return s_syncNodeDao.AddExchangeContainersAsync(containerInfoes);
        }

        protected override Task DeleteContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            return s_syncNodeDao.DeleteExchangeContainersAsync(containerInfoes);
        }

        protected override Task UpdateContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            return s_syncNodeDao.UpdateExchangeContainerAsync(containerInfoes);
        }

        protected override async Task SyncNodeAsync(RMContainerInfoAdaption containerInfo)
        {
            foreach (var tenantConnectionInfo in _tenantConnectionInfoes)
            {
                try
                {
                    _logger.Info($"Start sync [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] mailbox nodes.");

                    var queryResult = await _tenantClient.RemoteNodeService.QueryRemoteNodesAsync(new RemoteNodesQueryParameter
                    {
                        TenantId = tenantConnectionInfo.Id,
                        NodeTypes = new() { AosNodeType },
                        ContainerId = containerInfo.AosId
                    });

                    var aosNodes = queryResult.Mailboxes.ConvertAll(item => new RMExchangeNodeAdaption
                    {
                        Id = item.Id,
                        ObjectId = item.ObjectId,
                        TenantId = item.TenantId,
                        ContainerId = containerInfo.Id,
                        ContainerName = containerInfo.Name,
                        NodeLevel = NodeLevel.ExchangeOnlineMailbox,
                        AppType = ConvertIdentityTypeToAppType(item.AppProfileType),
                        ConnectionType = (BposConnectionType)item.ConnectionType,
                        EmailAddress = item.Name,
                        UserName = (item.ConnectionType == ConnectionType.AppToken || item.ConnectionType == ConnectionType.Modern) ? item.AppProfileUsername : item.ServiceAccountUsername
                    }).ToHashSet();

                    _logger.Debug($"Mailbox node count: [{aosNodes.Count}].");

                    var recordNodes = (await s_syncNodeDao.GetExchangeNodesAsync(containerInfo.Id, tenantConnectionInfo.Id)).ToHashSet();

                    var needAddNodes = aosNodes.Except(recordNodes).ToHashSet();
                    _logger.Debug($"Need add mailbox node count: [{needAddNodes.Count}].");
                    await s_syncNodeDao.AddExchangeNodesAsync(needAddNodes);
                    RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Add, containerInfo, needAddNodes);
                    await _changeLogger.Record(needAddNodes, ContentSource, RMSyncNodeChangeType.Add);

                    var needDeleteNodes = recordNodes.Except(aosNodes).ToHashSet();
                    _logger.Debug($"Need delete mailbox node count: [{needDeleteNodes.Count}].");
                    await s_syncNodeDao.DeleteExchangeNodesAsync(needDeleteNodes);
                    RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Delete, containerInfo, needDeleteNodes);
                    needDeleteNodes.ForEach(item => item.ContainerName = containerInfo.Name);
                    await _changeLogger.Record(needDeleteNodes, ContentSource, RMSyncNodeChangeType.Delete);

                    var intersectNodes = recordNodes.Intersect(aosNodes).ToHashSet();

                    var needUpdateNodes = await intersectNodes.ToAsyncEnumerable().WhereAwait(async intersectNode =>
                    {
                        var hasChange = false;

                        var existNode = aosNodes.First(aosContainer => intersectNode.ObjectId.Equals(aosContainer.ObjectId, StringComparison.OrdinalIgnoreCase));

                        if (intersectNode.EmailAddress != existNode.EmailAddress)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed email address.");
                            await _changeLogger.RecordChangeName(intersectNode, ContentSource, intersectNode.EmailAddress, existNode.EmailAddress);
                            hasChange = true;
                        }

                        if (intersectNode.AppType != existNode.AppType)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed app type.");
                            hasChange = true;
                        }

                        if (intersectNode.ConnectionType != existNode.ConnectionType)
                        {
                            _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed connection type.");
                            hasChange = true;
                        }

                        if (string.IsNullOrWhiteSpace(intersectNode.UserName) || !intersectNode.UserName.Equals(existNode.UserName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!string.IsNullOrWhiteSpace(existNode.UserName))
                            {
                                _logger.Info($"The object [O365 tenant: {intersectNode.TenantId} - Object id: {intersectNode.ObjectId}] has been changed user name.");
                                hasChange = true;
                            }
                        }

                        intersectNode.EmailAddress = existNode.EmailAddress;
                        intersectNode.AppType = existNode.AppType;
                        intersectNode.ConnectionType = existNode.ConnectionType;
                        intersectNode.UserName = existNode.UserName;

                        return hasChange;
                    }).ToListAsync();
                    _logger.Debug($"Need update node count: [{needUpdateNodes.Count}].");
                    await s_syncNodeDao.UpdateExchangeNodesAsync(needUpdateNodes);
                    RMSyncNodeJobManager.AddSucceedJobDetail(ContentSource, RMSyncNodeAction.Update, containerInfo, needUpdateNodes);

                    _logger.Info($"Successful sync [O365 Tenant: {tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - container aos id: {containerInfo.AosId} - container name: {containerInfo.Name}] mailbox nodes.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while sync [{tenantConnectionInfo.Id} - {tenantConnectionInfo.Name} - {containerInfo.AosId} - {containerInfo.Name}] mailbox nodes. Error: {e}");
                    RMSyncNodeJobManager.AddFailedJobDetail(ContentSource, RMSyncNodeAction.None, [containerInfo], e);
                }
            }
        }

        protected override async Task UpgradeNodeAsync(RMContainerInfoAdaption containerInfo)
        {

        }
    }
}
