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
using Cloud.Sdk.Data.Cop.Insights;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS.Executors
{
    public class RMSyncSharePointSiteNodeExecutor : RMSyncSiteNodeExecutor
    {
        public RMSyncSharePointSiteNodeExecutor(AosModernApiTenantClient tenantClient, List<TenantConnectionInfo> tenantConnectionInfoes, RMSyncNodeAzureChangeLogger changeLogger) 
            : base(tenantClient, tenantConnectionInfoes, changeLogger)
        {
        }

        public override SourceFlag ContentSource => SourceFlag.SharePoint;

        public override RemoteNodeType AosNodeType => RemoteNodeType.SiteCollection;

        protected override NodeLevel RecordContainerNodeLevel => NodeLevel.WebApplication;

        protected override IEnumerable<RMSiteNodeAdaption> ConvertAosNodesToAdaption(RMContainerInfoAdaption containerInfo, RemoteNodesResult queryResult)
        {
            var result = new List<RMSiteNodeAdaption>();
            foreach (var item in queryResult.SPSites)
            {
                if (string.IsNullOrEmpty(item.Url) || string.IsNullOrEmpty(item.ObjectId))
                {
                    _logger.Warn($"SiteCollection url or object id is null or empty for SiteCollection id: {item.Id}, object id: {item.ObjectId}");
                    continue;
                }
                if (!ExistObjectId.Add(item.ObjectId))
                {
                    _logger.Warn($"SiteCollection with object id: {item.ObjectId} already exists, skip it to avoid duplicate. Site id: {item.Id}");
                    continue;
                }
                _logger.Info($"Start to convert SiteCollection id: {item.Id}, object id: {item.ObjectId}");
                var res = new RMSiteNodeAdaption
                {
                    Id = item.Id,
                    ObjectId = item.ObjectId,
                    TenantId = item.TenantId,
                    ContainerId = containerInfo.Id,
                    ContainerName = containerInfo.Name,
                    AdminUrl = item.AdminUrl,
                    NodeLevel = NodeLevel.SiteCollection,
                    ConnectionType = (BposConnectionType)item.ConnectionType,
                    Url = item.Url,
                    Name = item.Name,
                    DomainName = item.DomainName,
                    TemplateName = item.TemplateName,
                    TemplateTitle = item.TemplateTitle,
                    IsPublicWebSite = item.IsPublicWebSite,
                    AppType = ConvertIdentityTypeToAppType(item.AppProfileType),
                    UserName = (item.ConnectionType == ConnectionType.AppToken || item.ConnectionType == ConnectionType.Modern) ? item.AppProfileUsername : item.ServiceAccountUsername
                };
                SetRealSiteIdAndName(res, RemoteNodeType.SiteCollection).GetAwaiter().GetResult();
                result.Add(res);
            }
            return result;
        }
    }
}
