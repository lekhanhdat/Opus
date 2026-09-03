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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.DB.Dao;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    public class SharePointOnPremJobController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(SharePointOnPremJobController));

        private ISharePointOnPremiseSettingDao _SharePointOnPremiseSettingDao;

        private ISharePointOnPremiseSettingDao SharePointOnPremiseSettingDao => PlatformWindsorManager.GetService(ref _SharePointOnPremiseSettingDao);

        private IRMNodeFlagDao _RMNodeFlagDao;

        private IRMNodeFlagDao RMNodeFlagDao => PlatformWindsorManager.GetService(ref _RMNodeFlagDao);

        [HttpPost]
        public List<RMSPTreeNode> BrowseSPTreeNode([FromBody] RMSPTreeNode node)
        {
            List<RMSPTreeNode> nodes = new List<RMSPTreeNode>();
            return nodes;
        }

        [HttpPost]
        public Task<bool> SetSettingJobTime([FromBody] SPSettingJobInfo info)
        {
            return SharePointOnPremiseSettingDao.SetSettingJobTimeAsync(info.ScopeId, info.SiteId, info.IsFailedColumn, info.IsFailedProperty);
        }

        [HttpGet]
        public long GetAutoJobCollectionTime(int type, Guid folderId, Guid listId, Guid nodeId, Guid groupId)
        {
            return RMNodeFlagDao.GetAutoJobCollectionTime(type, folderId, listId, nodeId, groupId);
        }

        [HttpPost]
        public bool UpdateAutoJobCollectionTime([FromBody] List<NodeFlag> nodeFlags)
        {
            bool result = true;
            try
            {
                foreach (var node in nodeFlags)
                {
                    RMNodeFlagDao.AddListFlagInfo(new DB.Model.RMNodeFlag()
                    {
                        CollectionTime = node.CollectionTime,
                        FolderId = node.FolderId,
                        FullPath = node.FullPath,
                        GroupId = node.GroupId,
                        ListId = node.ListId,
                        NodeFlagType = node.NodeFlagType,
                        NodeId = node.NodeId,
                        RowId = node.RowId,
                        Title = node.Title
                    });
                }
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while UpdateAutoJobCollectionTime, Error:{0}", e.ToString());
                result = false;
            }
            return result;
        }
    }
}
