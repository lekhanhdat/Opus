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
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.Contract.SharePoint.OnPrem;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SharePoint
{
    public class RMLocalNodeService : RMServiceBase, IRMLocalNodeService
    {
        private IRMLocalNodeDao LocalNodeDao => PlatformWindsorManager.GetService<IRMLocalNodeDao>();

        public void BatchAdd(IEnumerable<OnPremiseSPLocalNode> nodes)
        {
            LocalNodeDao.BatchAdd(nodes);
        }

        public int DeleteNodesByIDs(IEnumerable<string> ids)
        {
            return LocalNodeDao.DeleteNodesByIDs(ids);
        }

        public int UpdateNodes(IEnumerable<OnPremiseSPLocalNode> nodes)
        {
            return LocalNodeDao.UpdateNodes(nodes);
        }

        public List<SPTreeNodeDto> GetAllNodes()
        {
            return LocalNodeDao.GetAllNodes();
        }

        public RMSiteCollection GetLocalSiteCollectionById(string id)
        {
            var node = LocalNodeDao.GetById(id);
            return Convert2SiteCollection(node);
        }

        public async Task<List<RMSiteCollection>> GetLocalSiteCollectionsByIdListAsync(List<string> ids)
        {
            var nodes = await LocalNodeDao.GetByIdsAsync(ids);
            return nodes.ConvertAll(Convert2SiteCollection);
        }

        public async Task<List<RMSiteCollection>> GetAllLocalSiteCollectionsAsync()
        {
            var nodes = await LocalNodeDao.GetAllNodesByLevelAsync(NodeLevel.SiteCollection);
            return nodes.ConvertAll(Convert2SiteCollection);
        }

        public bool IsLocalSiteCollectionExistByUrl(string url)
        {
            return GetLocalSiteCollectionByUrl(url) != null;
        }

        public RMSiteCollection GetLocalSiteCollectionByUrl(string url)
        {
            var node = LocalNodeDao.GetByUrl(url);
            return Convert2SiteCollection(node);
        }

        public async Task<List<RMWebApplication>> GetAllLocalWebApplicationsAsync()
        {
            var nodes = await LocalNodeDao.GetAllNodesByLevelAsync(NodeLevel.WebApplication);
            return nodes.ConvertAll(Convert2WebApplication);
        }

        public RMWebApplication GetLocalWebApplicationById(string id)
        {
            var node = LocalNodeDao.GetById(id);
            return Convert2WebApplication(node);
        }

        public async Task<List<RMSiteCollection>> GetLocalSiteCollectionsByWebAppIdAsync(string webappId)
        {
            var nodes = await LocalNodeDao.GetByParentIdAsync(webappId);
            return nodes.ConvertAll(Convert2SiteCollection);
        }

        public async Task<List<RMSiteCollection>> GetLocalSiteCollectionsByFarmIdAsync(string farmId)
        {
            var nodes = await LocalNodeDao.GetSitesByFarmIdAsync(farmId);
            return nodes.ConvertAll(Convert2SiteCollection);
        }

        public Task<List<OnPremiseSPLocalNode>> GetPageNodesByParentIdAsync(int pageIndex, int total, string parentId)
        {
            return LocalNodeDao.GetPageNodesByParentIdAsync(pageIndex, total, parentId);
        }
        public bool LocalNodesIsSync() 
        {
            return LocalNodeDao.SyncCount() > 0;
        }

        private RMSiteCollection Convert2SiteCollection(RMLocalNode node)
        {
            if (node == null) return null;
            return new RMSiteCollection
            {
                Id = node.Id,
                SPObjectId = node.ObjectId,
                ParentId = node.ParentId,
                FarmId = node.FarmId,
                Url = node.Url,
                Name = node.Name,
                CreateTime = node.CreateTime,
                ModifiedTime = node.ModifiedDate
            };
        }

        private RMWebApplication Convert2WebApplication(RMLocalNode node)
        {
            if (node == null) return null;
            return new RMWebApplication
            {
                Id = node.Id,
                SPObjectId = node.ObjectId,
                FarmId = node.FarmId,
                Url = node.Url,
                Name = node.Name,
                CreateTime = node.CreateTime,
                ModifiedTime = node.ModifiedDate
            };
        }
    }
}
