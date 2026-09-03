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
    public class RMSyncTeamGroupSiteNodeExecutor : RMSyncSiteNodeExecutor
    {
        public RMSyncTeamGroupSiteNodeExecutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfoes, RMSyncNodeAzureChangeLogger changeLogger) 
            : base(tenantClient, tenantConnectionInfoes, changeLogger)
        {
        }

        public override SourceFlag ContentSource => SourceFlag.SharePoint;

        public override RemoteNodeType AosNodeType => RemoteNodeType.Office365Group;

        protected override NodeLevel RecordContainerNodeLevel => NodeLevel.O365GroupSitesGroup;

        protected override IEnumerable<RMSiteNodeAdaption> ConvertAosNodesToAdaption(RMContainerInfoAdaption containerInfo, RemoteNodesResult queryResult)
        {
            var result = new List<RMSiteNodeAdaption>();
            foreach (var item in queryResult.O365Groups)
            {
                if(string.IsNullOrEmpty(item.SiteUrl) || string.IsNullOrEmpty(item.SiteId))
                {
                    _logger.Warn($"O365Group site url or site object id is null or empty for site id: {item.Id}, site id: {item.SiteId}");
                    continue;
                }
                if(!ExistObjectId.Add(item.SiteId))
                {
                    _logger.Warn($"O365Group site with site id: {item.SiteId} already exists, skip it to avoid duplicate. Site id: {item.Id}");
                    continue;
                }
                _logger.Info($"Start to convert O365Group site site id: {item.Id}, site id: {item.SiteId}");
                var res = new RMSiteNodeAdaption
                {
                    Id = item.Id,
                    ObjectId = item.SiteId,
                    TenantId = item.TenantId,
                    ContainerId = containerInfo.Id,
                    ContainerName = containerInfo.Name,
                    AdminUrl = item.AdminUrl,
                    NodeLevel = NodeLevel.O365GroupSites,
                    ConnectionType = (BposConnectionType)item.ConnectionType,
                    Url = item.SiteUrl,
                    Name = item.Name,
                    DisplayName = item.DisplayName,
                    SiteCollectionType = item.GroupType switch
                    {
                        O365GroupType.TeamsGroup => AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Teams,
                        _ => AvePoint.GCommon.Contract.SharePointBrowser.SiteCollectionType.Group
                    },
                    TeamId = item.ObjectId,
                    TemplateName = item.TemplateName,
                    TemplateTitle = item.TemplateTitle,
                    AppType = ConvertIdentityTypeToAppType(item.AppProfileType),
                    UserName = (item.ConnectionType == ConnectionType.AppToken || item.ConnectionType == ConnectionType.Modern) ? item.AppProfileUsername : item.ServiceAccountUsername
                };
                SetRealSiteIdAndName(res, RemoteNodeType.Office365Group).GetAwaiter().GetResult();
                result.Add(res);
            }
            return result;
        }

        protected override void ReNameContainers(IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            containerInfoes.ForEach(item => item.Name = RMSyncNodeConverter.ContainerNameConvertToDB(item.Name));
        }
    }
}
