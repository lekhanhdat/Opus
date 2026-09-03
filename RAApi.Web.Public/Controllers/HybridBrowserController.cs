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
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [Route("api/hybridbrowser/[action]")]
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    public class HybridBrowserController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(HybridBrowserController));

        private IFileSystemTreeCacheDao _FileSystemTreeCacheDao;
        private IFSConnectionDao _FSConnectionDao;

        public IFileSystemTreeCacheDao FileSystemTreeCacheDao => PlatformWindsorManager.GetService(ref _FileSystemTreeCacheDao);
        public IFSConnectionDao FSConnectionDao => PlatformWindsorManager.GetService(ref _FSConnectionDao);

        [HttpPost]
        public async Task<bool> NodeSave([FromBody] HBNodeRequestInfo nodeRequestInfo)
        {
            var safeLogInfo = new
            {
                tenantId = nodeRequestInfo.tenantId,
                nodes = nodeRequestInfo.nodes?.Select(n => new
                {
                    Id = string.IsNullOrEmpty(n.Id) ? n.Id : n.Id.LogBase64(),
                    Name = n.Name,
                    Url = string.IsNullOrEmpty(n.Url) ? n.Url : n.Url.LogBase64(),
                    BatchId = n.BatchId
                })
            };
            logger.Info($"nodeRequestInfo:{JsonConvert.SerializeObject(safeLogInfo)}");
            //TenantLocalValue.LogonGroupId = new Guid("b9b1c679-d428-4b57-865c-a9683fb2355a").ToString();
            TenantLocalValue.LogonGroupId = nodeRequestInfo.tenantId;
            return await execute(nodeRequestInfo.tenantId, nodeRequestInfo.nodes);
        }

        private async Task<bool> execute(string tenantId, List<HBTreeNode> nodes)
        {
            //Task<bool> task = new Task<bool>(() => { return false; });
            try
            {
                var info = new FileSystemTreeCache();
                info.BatchId = Guid.Parse(nodes.First().BatchId);
                info.TreeData = JsonConvert.SerializeObject(nodes);
                return FileSystemTreeCacheDao.SaveTreeNodeInfo(info) > 0;
            }
            catch (Exception e)
            {
                logger.Error($"execute NodeSave {e.ToString()}");
                return false;
            }
        }


        [HttpGet]
        public async Task<bool> Test()
        {
            //var batchId = Guid.Parse("BC3D7903-26EF-443A-8077-364C307819FD");
            //HBNodeRequestInfo info = new HBNodeRequestInfo();
            //info.nodes = new List<HBTreeNode>();
            //info.nodes.Add(new HBTreeNode() { Id = Guid.NewGuid().ToString(), BatchId = batchId.ToString(), Name = "Path1", Url = @"\\10.1.54.151\C$\conn01a" });
            //info.nodes.Add(new HBTreeNode() { Id = Guid.NewGuid().ToString(), BatchId = batchId.ToString(), Name = "Path2", Url = @"\\10.1.54.151\C$\conn01b" });
            //var task = NodeSave(info);
            Task<bool> task = new Task<bool>(() =>
            {
                return true;
            });
            task.RunSynchronously();
            return await task;
        }

        [HttpPost]
        public async Task<bool> AddSucceedValidateConnectionIds([FromBody] FileSystemValidateSucceedConnectionInfo connectionInfo)
        {
            try
            {
                TenantLocalValue.LogonGroupId = connectionInfo.TenantId;

                return await FileSystemTreeCacheDao.AddValidateConnectionIds(
                    connectionInfo.BatchId,
                    connectionInfo.ConnectionIds);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while adding succeed validate connection ids. Error: {e}");
                return false;
            }
        }
    }
}
