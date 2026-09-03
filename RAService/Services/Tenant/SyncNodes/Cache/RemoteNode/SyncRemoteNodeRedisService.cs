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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.SyncNode.Compatible;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache
{
    public class SyncRemoteNodeRedisService : AbstractSyncNodeRedisService, ISyncRemoteNodeRedisService
    {
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private static AveLogger logger = AveLogger.GetInstance(typeof(SyncRemoteNodeRedisService));

        #region ISyncRemoteNodeRedisService Impl
        public void InsertRemoteNode(RemoteSiteCollection remoteNode, Action sqlAction)
        {
            if (remoteNode == null)
            {
                logger.Info("No remotenode to insert into redis.");
                return;
            }
            List<SyncRemoteNodePara> caches = new List<SyncRemoteNodePara> { SyncDataConverter.ConvertDBNodeModelToCacheModel(remoteNode) };
            AddNodesToCache(TenantLocalValue.LogonGroupId, SyncDataConverter.ConvertCacheListToDict(caches), sqlAction);
            logger.Info("Insert remotenodes to redis successfully.");
        }

        public void UpdateRemoteNode(RemoteSiteCollection remoteNode, Action sqlAction)
        {
            if (remoteNode == null)
            {
                logger.Info("No remotenode to update to redis.");
                return;
            }
            List<SyncRemoteNodePara> caches = new List<SyncRemoteNodePara> { SyncDataConverter.ConvertDBNodeModelToCacheModel(remoteNode) };
            UpdateNodesToCache(TenantLocalValue.LogonGroupId, SyncDataConverter.ConvertCacheListToDict(caches), sqlAction);
            logger.Info("Update remotenode to redis successfully.");
        }

        public void UpdateRemoteNodes(List<RemoteSiteCollection> remoteNodes, Action sqlAction)
        {
            if (remoteNodes == null || remoteNodes.Count == 0)
            {
                logger.Info("No remotenodes to update to redis.");
                return;
            }
            List<SyncRemoteNodePara> caches = remoteNodes.ConvertAll(SyncDataConverter.ConvertDBNodeModelToCacheModel);
            UpdateNodesToCache(TenantLocalValue.LogonGroupId, SyncDataConverter.ConvertCacheListToDict(caches), sqlAction);
            logger.Info("Update remotenodes to redis successfully.");
        }

        public void DeleteRemoteNodes(List<string> urls, Action sqlAction, bool ignoreCase = true)
        {
            if (urls == null || urls.Count == 0)
            {
                logger.Info("No urls to be deleted.");
                return;
            }
            DeleteNodesFromCache(TenantLocalValue.LogonGroupId, urls, sqlAction, ignoreCase);
            logger.Info("Delete redis successfully.");
        }
        #endregion

        #region AbstractSyncNodeRedisService Override
        protected override string GenerateCacheKey(string tenantGroupId)
        {
            return RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.RemoteNode);
        }

        protected override string GenerateFieldKeyForGroup(RMCompatibleRemoteNode aosSyncNode)
        {
            return RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(aosSyncNode);
        }

        protected override string GenerateFieldKeyForGroup(RemoteNodePara daoGroup)
        {
            //return RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKey(daoGroup);
            return RedisFieldKeyUtil.GenerateRemoteNodeGroupFieldKeyByAosId(daoGroup);
        }

        protected override List<RemoteNodePara> GetAllGroupsInDB()
        {
            var groups = new List<RemoteNodePara>();
            try
            {
                groups = RemoteNodeService.GetRemoteWebApplicationNodes();
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get all groups. Exception is {0}.", ex.ToString());
                throw;
            }
            return groups;
        }

        protected override IEnumerable<List<SyncRemoteNodePara>> GetAllNodesInDBByPage()
        {
            var pageSize = 1000;
            for (var pageIndex = 0; ; pageIndex++)
            {
                var res = RemoteNodeService.GetAllSiteCollectionNodesByPage(pageIndex, pageSize);
                if (!res.Any())
                {
                    yield break;
                }
                else
                {
                    yield return res;
                }
            }
        }

        //protected override List<SyncRemoteNodePara> GetAllNodesInDB()
        //{
        //    var nodes = new List<SyncRemoteNodePara>();
        //    try
        //    {
        //        nodes = RemoteNodeService.GetAllSiteCollectionNodes();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Failed to get all nodes. Exception is {0}.", ex.ToString());
        //        throw;
        //    }
        //    return nodes;
        //}
        #endregion
    }
}
