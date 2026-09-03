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
using AvePoint.RA.Api.Web.Public.Common;
using AvePoint.RA.Api.Web.Public.Filters;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.SharePoint;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Public.Controllers
{
    [APIScopeFilter(AvePoint.RA.Contract.Common.ContractConstants.HybridAgentScope)]
    public class SharePointOnPremLocalNodeController : RAWebApiBase
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointOnPremLocalNodeController));

        private IRMLocalNodeService _LocalNodeService;

        public IRMLocalNodeService LocalNodeService => PlatformWindsorManager.GetService(ref _LocalNodeService);

        [HttpGet]
        public async Task<List<OnPremiseSPLocalNode>> GetRecordsLocalNodes(int pageIndex, int total, string parentId)
        {
            try
            {
                Logger.Info($"Start get sharepoint on-premise records nodes. Page index: [{pageIndex}], Total: [{total}], Parent id: [{parentId}].");
                return await LocalNodeService.GetPageNodesByParentIdAsync(pageIndex, total, parentId);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get sharepoint on-prem records node. Error: {e}");
            }
            return new List<OnPremiseSPLocalNode>();
        }

        [HttpPost]
        public OnPremSPScanNodeResult BatchAddRecordsLocalNodes([FromBody] List<OnPremiseSPLocalNode> localNodes)
        {
            try
            {
                Logger.Info("Start batch add sharepoint on-premise records node.");
                LocalNodeService.BatchAdd(localNodes);
                return new OnPremSPScanNodeResult();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while sharepoint on-premise batch add records node. Error: {e}");
                return new OnPremSPScanNodeResult(false, e.Message);
            }
        }

        [HttpPost]
        public OnPremSPScanNodeResult BatchUpdateRecordsLocalNodes([FromBody] List<OnPremiseSPLocalNode> localNodes)
        {
            try
            {
                Logger.Info("Start batch update sharepoint on-premise records node.");
                LocalNodeService.UpdateNodes(localNodes);
                return new OnPremSPScanNodeResult();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while sharepoint on-premise batch update records node. Error: {e}");
                return new OnPremSPScanNodeResult(false, e.Message);
            }
        }

        [HttpPost]
        public OnPremSPScanNodeResult BatchDeleteRecordsLocalNodes([FromBody] List<OnPremiseSPLocalNode> localNodes)
        {
            try
            {
                Logger.Info("Start batch delete sharepoint on-premise records node.");
                LocalNodeService.DeleteNodesByIDs(localNodes.Select(item => item.Id));
                return new OnPremSPScanNodeResult();
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while sharepoint on-premise batch delete records node. Error: {e}");
                return new OnPremSPScanNodeResult(false, e.Message);
            }
        }
    }
}
