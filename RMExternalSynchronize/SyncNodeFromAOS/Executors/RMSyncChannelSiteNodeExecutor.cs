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
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.SharePoint.Common;
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
    public class RMSyncChannelSiteNodeExecutor : RMSyncSiteNodeExecutor
    {
        public RMSyncChannelSiteNodeExecutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfoes, RMSyncNodeAzureChangeLogger changeLogger) 
            : base(tenantClient, tenantConnectionInfoes, changeLogger)
        {
        }

        public override SourceFlag ContentSource => SourceFlag.SharePoint;

        public override RemoteNodeType AosNodeType => RemoteNodeType.Channel;

        protected override NodeLevel RecordContainerNodeLevel => NodeLevel.PrivateChannelGroup;

        protected override IEnumerable<RMSiteNodeAdaption> ConvertAosNodesToAdaption(RMContainerInfoAdaption containerInfo, RemoteNodesResult queryResult)
        {
            List<RMSiteNodeAdaption> result = new List<RMSiteNodeAdaption>();
            foreach (var item in queryResult.Channels)
            {
                if (string.IsNullOrEmpty(item.SiteUrl) || string.IsNullOrEmpty(item.SiteId))
                {
                    _logger.Warn($"Channel site url or site id is null or empty for Channel id: {item.Id}, site id: {item.SiteId}");
                    continue;
                }
                if (!ExistObjectId.Add(item.SiteId))
                {
                    _logger.Warn($"Channel site with site id: {item.SiteId} already exists, skip it to avoid duplicate. Channel id: {item.Id}");
                    continue;
                }
                _logger.Info($"Start to convert Channel id: {item.Id}, site id: {item.SiteId}");
                var res = new RMSiteNodeAdaption
                {
                    Id = item.Id,
                    ObjectId = item.SiteId,
                    TenantId = item.TenantId,
                    ContainerId = containerInfo.Id,
                    ContainerName = containerInfo.Name,
                    NodeLevel = item.ChannelType == ChannelType.Shared ? NodeLevel.SharedChannel : NodeLevel.PrivateChannel,
                    ConnectionType = (BposConnectionType)item.ConnectionType,
                    AdminUrl = item.AdminUrl,
                    Url = item.SiteUrl,
                    Name = item.Name,
                    AppType = ConvertIdentityTypeToAppType(item.AppProfileType),
                    TeamId = item.ParentId,
                    SiteCollectionType = AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.PrivateChannel,
                    UserName = (item.ConnectionType == ConnectionType.AppToken || item.ConnectionType == ConnectionType.Modern) ? item.AppProfileUsername : item.ServiceAccountUsername
                };
                SetRealSiteIdAndName(res, RemoteNodeType.Channel).GetAwaiter().GetResult();
                result.Add(res);
            }
            return result;
        }

        protected override async Task<IEnumerable<RMContainerInfoAdaption>> GetContainersAsync()
        {
            return await Task.FromResult(new List<RMContainerInfoAdaption> {
                new RMContainerInfoAdaption
                {
                    Id = "41cfe969-e07b-45cb-a7d0-b022f967e929",
                    Name = "Default Private Channel Sites Container",
                    NodeLevel = RecordContainerNodeLevel,
                    AosId = "41cfe969-e07b-45cb-a7d0-b022f967e929",
                }
            });
        }
    }
}
