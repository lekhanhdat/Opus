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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Browser.Model;
using AvePoint.RA.Browser.Service.Impl;
using AvePoint.RA.Common.SharePointBrowser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.Browser.SPO
{
    public class SPOBposChildrenBrowser : SPOBaseBrowser
    {

        private static readonly string ADVANCED_SEARCH = "SPTreeAdvancedSearch";

        private static readonly AgentBrowserService browserService = new AgentBrowserService();

        private BrowserType mBrowserType = BrowserType.SharePointOnline;
        public SPOBposChildrenBrowser(BrowserType browserType)
        {
            mBrowserType = browserType;
        }

        protected override async Task<BrowseResult> BrwoseAsync(SPTreeNodeDto node)
        {
            var result = new BrowseResult();
            var nodeTemp = node;
            try
            {
                Logger.Info($"Begin browse sharepoint tree node by parent id: {node?.ID}, type: {node.Level}.");
                var nodePath = new List<SPTreeNodeDto>();
                if (nodeTemp != null && nodeTemp.Level == NodeLevel.WebApplication)
                {
                    nodePath.Add(nodeTemp);
                }

                while (nodeTemp != null && nodeTemp.Level > NodeLevel.WebApplication)
                {
                    if (nodeTemp.Level == NodeLevel.SiteCollections || nodeTemp.Level == NodeLevel.Office365GroupEntire) // teams treenode level
                    {
                        nodeTemp = nodeTemp.Parent;
                        continue;
                    }
                    nodePath.Insert(0, nodeTemp);
                    nodeTemp = nodeTemp.Parent;
                }

                var browseContract = new SharePointBrowserContract();
                if (node == null || node.NodeExtension.AgentType == null)
                {
                    browseContract.AgentType = string.Empty;
                }
                else
                {
                    browseContract.AgentType = node.NodeExtension.AgentType;
                }
                browseContract.ParentNodes = nodePath;
                browseContract.StartIndex = node == null ? 0 : node.StartIndex;
                browseContract.PageInfo = node?.PageInfo;
                browseContract.PerPage = uint.MaxValue;
                if (node != null && (node.Level == NodeLevel.Sites || node.Level == NodeLevel.Lists || node.Level == NodeLevel.Folders))
                {
                    browseContract.IsAdvancedSearchEnable = node.IsAdvancedSearchEnable;
                }

                if (node.PolicyInfo != null)
                {
                    browseContract.FilterPolicy = node.PolicyInfo;
                    browseContract.AgentType = ADVANCED_SEARCH;
                }

                if (node != null && node.SPType == SPType.BPOS)
                {
                    browseContract.IsBPOS = true;
                }

                var treeType = node.NodeExtension?.TreeType ?? TreeType.Undefined;
                var request = new BrowserMessage()
                {
                    TreeType = treeType,
                    BrowserContract = browseContract
                };
                var response = await browserService.HandleMessageAsync(request, mBrowserType);
                if (response != null)
                {
                    var browserResult = response.BrowserContract as SharePointBrowserContract;
                    if (browseContract.HasError)
                    {
                        Logger.Error("");
                    }
                    else
                    {
                        List<SPTreeNodeDto> children = browserResult.ChildenNodes;
                        if (node != null && children != null)
                        {
                            Logger.Info($"Browse children completed. Children number is {children.Count}.");
                            children.ForEach(c => { c.FarmID = node.FarmID; c.ID = Guid.NewGuid().ToString(); c.SPVersion = node.SPVersion; });
                        }
                        result.Children = children;
                        result.PageInfo = browserResult.PageInfo;
                        result.HasNextPage = browserResult.HasNextPage;
                        result.ChildrenCount = browserResult.ChildrenCount;

                        if (result.ChildrenCount == 0)
                        {
                            result.ChildrenCount = result.Children.Count;
                        }

                        if (node != null && node.Level == NodeLevel.Items)
                        {
                            result.ChildrenCount = node.StartIndex + result.ChildrenCount;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while browse sharepoint tree node. Error: {e}");
            }
            return result;
        }
    }
}
