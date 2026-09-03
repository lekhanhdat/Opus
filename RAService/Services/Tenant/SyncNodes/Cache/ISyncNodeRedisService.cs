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
using AvePoint.RA.Common.SyncNode.Compatible;
using System;
using System.Collections.Generic;
using AOS_SDK = Cloud.Sdk.Data.Aos.Tenant;

namespace AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache
{
    public interface ISyncNodeRedisService
    {
        void InitCache(string tenantGroupId);
        void AddGroupsToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> cacheGroupsDict, Action sqlExecution = null);
        void UpdateGroupToCache(string tenantGroupId, Dictionary<string, RemoteNodePara> cacheNodes, Action sqlAction = null);
        void AddNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> cacheNodes, Action sqlAction = null);
        void DeleteNodesFromCache(string tenantGroupId, List<string> deleteFieldKeys, Action sqlAction, bool ignoreCase = true);
        void UpdateNodesToCache(string tenantGroupId, Dictionary<string, SyncRemoteNodePara> cacheNodes, Action sqlAction = null);
        Dictionary<string, RemoteNodePara> GetGroupsCache(string tenantGroupId, List<RMCompatibleRemoteNode> aosSyncNodes);
        Dictionary<string, SyncRemoteNodePara> GetNodesCache(string tenantGroupId, List<string> urls);
    }
}
