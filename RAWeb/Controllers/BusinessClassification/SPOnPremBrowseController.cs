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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.SharePoint;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.BusinessClassification
{
    [RMApiAuthorize(RMPermissionMasks.SPOnPremEnduser, preferred: false)]
    public class SPOnPremBrowseController : BaseApiController
    {
        private IRMSharePointOnPremBrowseService _SharePointOnPremBrowseService;
        private IRMSharePointOnPremBrowseService SharePointOnPremBrowseService => PlatformWindsorManager.GetService(ref _SharePointOnPremBrowseService);
        private IRMSharePointOnPremSettingsService _SharePointOnPremSettingsService;
        private IRMSharePointOnPremSettingsService SharePointOnPremSettingsService => PlatformWindsorManager.GetService(ref _SharePointOnPremSettingsService);
        private ISPSettingTreeService _RMSPTreeService;
        private ISPSettingTreeService RMSPTreeService => PlatformWindsorManager.GetService(ref _RMSPTreeService);
        private IRMSharePointSettingsService _RMSPSService;
        private IRMSharePointSettingsService RMSPSService => PlatformWindsorManager.GetService(ref _RMSPSService);
        private IRMLocalNodeService _LocalNodeService;
        private IRMLocalNodeService LocalNodeService => PlatformWindsorManager.GetService(ref _LocalNodeService);    

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.CommonModuleAccess)]
        public bool CheckLocalNodesIsInit()
        {
            try
            {
                return LocalNodeService.LocalNodesIsSync();
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while check local nodes is init. Error: {e}");
                throw;
            }
        }

        public async Task<string> BrowseSampleTree([FromBody] RMSPSampleTreeNode currentNode)
        {
            try
            {
                var children = await SharePointOnPremBrowseService.BrowseSampleTreeAsync(currentNode, true);
                await SharePointOnPremSettingsService.LoadSPSettingIconAsync(children);
                return JsonConvert.SerializeObject(children);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while browser sample tree by node: [{currentNode?.Name ?? "root"}]. Error: {e}");
                throw;
            }
        }

        public async Task<RMSPSampleTreeNode> BrowseSampleTreePaged([FromBody] RMSPSampleTreeNode node)
        {
            try
            {
                var children = await SharePointOnPremBrowseService.BrowseSampleTreeAsync(node, true);
                await SharePointOnPremSettingsService.LoadSPSettingIconAsync(children);
                node.Children = children;
                return node;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while browser sample tree by node: [{node?.Name ?? "root"}]. Error: {e}");
                throw;
            }
        }

        [HttpPost]
        public async Task<string> BrowseReportTree([FromBody] string node)
        {
            var currentNode = string.IsNullOrEmpty(node) ? null : SerializerHelper.DeserializeByJsonConvert<RMSPTreeNode>(node);
            try
            {
                var children = await SharePointOnPremBrowseService.BrowseReportTreeAsync(currentNode);
                return JsonConvert.SerializeObject(children);
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while browser report tree by node: [{currentNode?.Name ?? "root"}]. Error: {e}");
                throw;
            }
        }

        [HttpPost]
        public Task<OnPremiseSPTermInfo> GetTermInfoBySiteUrl(string siteUrl)
        {
            return SharePointOnPremClient.GetTermStoreInfoBySiteUrlAsync(siteUrl);
        }

        [HttpPost]
        public string GetSPDesignLists()
        {
            var lists = RMSPSService.GetDesignLists();
            return JsonConvert.SerializeObject(lists);
        }

        [HttpPost]
        public string GetSPTreeInitData()
        {
            var farmNode = RMSPTreeService.LoadFarm()[0];
            if (farmNode == null || string.IsNullOrEmpty(farmNode.Id))
            {
                Logger.Warn("Farm node is null.Please refresh page.");
            }
            else
            {
                if (farmNode.Children != null)
                {
                    farmNode.Children = null;
                }
            }
            return JsonConvert.SerializeObject(farmNode);
        }

    }
}