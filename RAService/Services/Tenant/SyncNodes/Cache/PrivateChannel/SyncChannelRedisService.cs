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
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
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
    public class SyncChannelRedisService : AbstractSyncNodeRedisService, ISyncChannelRedisService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(SyncChannelRedisService));
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        public void DeletePrivateChannels(List<string> urls, Action sqlAction, bool ignoreCase = true)
        {
            if (urls == null || urls.Count == 0)
            {
                logger.Info("No urls to be deleted.");
                return;
            }
            DeleteNodesFromCache(TenantLocalValue.LogonGroupId, urls, sqlAction, ignoreCase);
            logger.Info("Delete redis successfully.");
        }

        #region AbstractSyncNodeRedisService
        protected override string GenerateCacheKey(string tenantGroupId)
        {
            return RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.PrivateChannel);
        }

        protected override string GenerateFieldKeyForGroup(RMCompatibleRemoteNode aosSyncNode)
        {
            throw new NotImplementedException();
        }

        protected override string GenerateFieldKeyForGroup(RemoteNodePara daoGroup)
        {
            throw new NotImplementedException();
        }

        protected override List<RemoteNodePara> GetAllGroupsInDB()
        {
            logger.Warn("private channel group cant by cache");
            return new List<RemoteNodePara>();
        }

        protected override IEnumerable<List<SyncRemoteNodePara>> GetAllNodesInDBByPage()
        {
            var pageSize = 1000;
            for (var pageIndex = 0; ; pageIndex++)
            {
                var res = RemoteNodeService.GetAllPrivateChannelByPage(pageIndex, pageSize);
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
        //        nodes = RemoteNodeService.GetAllPrivateChannel();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Failed to get all channels. Exception is {0}.", ex.ToString());
        //    }
        //    return nodes;
        //}
        #endregion
    }
}
