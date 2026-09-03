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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne;

[Route("api/googleone/nodes")]
public class GoogleOneNodesApiController : GoogleOneApiBaseController
{
    private static RALogger s_logger = RALogger.GetInstance(typeof(GoogleOneNodesApiController));

    private IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();

    private IBrowseTreeService BrowseTreeService => PlatformWindsorManager.GetService<IBrowseTreeService>();

    [HttpGet("containers")]
    public async Task<string> GetContainerNodes()
    {
        try
        {
            var rootNode = RemoteGoogleNodeService.LoadGoogleDriveRoot()[0];
            var result = await BrowseTreeService.BrowseGoogleNodesByPagerAsync(rootNode, false);
            return JsonConvert.SerializeObject(result);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to get Google container nodes: {ex.Message}", ex);
            return JsonConvert.SerializeObject(new RMSampleGoogleTreeNode());
        }
    }

    [HttpPost("browse")]
    public async Task<string> BrowseNode([FromBody] RMSampleGoogleTreeNode node)
    {
        try
        {
            if(node is null) throw new ArgumentNullException("Node cannot be null.");
            var returnNode = await BrowseTreeService.BrowseGoogleNodesByPagerAsync(node, false);
            return JsonConvert.SerializeObject(returnNode);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to browse nodes with current node [{node?.FullPath}]. Ex: {ex.Message}.");
            return JsonConvert.SerializeObject(node ?? new RMSampleGoogleTreeNode());
        }
    }

    [HttpPost("browseforrule")]
    public async Task<string> BrowseSampleTreeForRule([FromBody] RMSampleGoogleTreeNode parentNode)
    {
        List<RMSampleGoogleTreeNode> children = (await BrowseTreeService.BrowseGoogleDriveTreeForRuleAsync(parentNode, false)).Children;
        parentNode.Children = null;
        children?.ForEach(child =>
        {
            child.Parent = parentNode;
            child.ParentId = parentNode.Id;
        });
        return JsonConvert.SerializeObject(children);
    }


    [HttpPost("browseforfulllevel")]
    public async Task<string> BrowseSampleTreeForFullLevel([FromBody] RMSampleGoogleTreeNode parentNode)
    {
        List<RMSampleGoogleTreeNode> children = (await BrowseTreeService.BrowseGoogleDriveTreeForFullLevelAsync(parentNode, false)).Children;
        parentNode.Children = null;
        children?.ForEach(child =>
        {
            child.Parent = parentNode;
            child.ParentId = parentNode.Id;
        });
        return JsonConvert.SerializeObject(children);
    }
}