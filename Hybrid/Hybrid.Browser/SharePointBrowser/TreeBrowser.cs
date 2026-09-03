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
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Hybrid.Browser.SharePointBrowser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Hybrid.Browser.SharePointBrowser
{
    public class TreeBrowser
    {

        private static readonly AvePoint.GCommon.AveLogger Logger = AvePoint.GCommon.AveLogger.GetInstance(typeof(TreeBrowser));

        public static AveTreeMessage Browse(AveTreeMessage request)
        {

            var message = request as SPTreeMessage;
            if (message.Node != null)
            {
                var currentNode = message.Node;
                if (currentNode.NodeExtension != null)
                {
                    currentNode.NodeExtension.TreeType = request.TreeType;
                }
                if (message.PolicyInfo != null)
                {
                    currentNode.PolicyInfo = message.PolicyInfo;
                }
                currentNode.IsAdvancedSearchEnable = message.IsAdvancedSearchEnable;
            }
            Logger.Info($"Start browse sharepoint tree, level: {message?.Node?.Level}.");
            var result = LoadChildren(message.Node);
            var response = new SPTreeMessage
            {
                Node = message.Node,
                NodeList = result.Children as List<SPTreeNodeDto>,
                PageInfo = result.PageInfo,
                ChildrenCount = result.ChildrenCount,
                HasNextPage = result.HasNextPage,
                HasError = result.HasError,
                Message = result.ErrorMessage
            };
            Logger.Info($"End browse sharepoint tree, level: {message?.Node?.Level}, children count: {response.ChildrenCount}.");
            return response;
        }

        private static BrowseResult LoadChildren(SPTreeNodeDto currentNode)
        {
            BrowseResult result = null;
            try
            {
                if (currentNode == null || currentNode.Level == NodeLevel.Root)
                {
                    //result = BrowseFarmNode();
                }
                else if (currentNode.Level == NodeLevel.Site)
                {
                    result = BrowseSite(currentNode);
                    SetNodesProperties(result.Children as List<SPTreeNodeDto>, currentNode, currentNode.SPType);
                }
                else if (currentNode.Level == NodeLevel.RootFolder || currentNode.Level == NodeLevel.Folder)
                {
                    result = BrowseFolder(currentNode);
                    SetNodesProperties(result.Children as List<SPTreeNodeDto>, currentNode, currentNode.SPType);
                }
                else
                {
                    result = BrowseChildren(currentNode);
                    SetNodesProperties(result.Children as List<SPTreeNodeDto>, currentNode, currentNode.SPType);
                }
                (result.Children as List<SPTreeNodeDto>).Sort(AveTreeUtil.SPTreeNodeComparision);
            }
            catch (Exception e)
            {

            }
            return result;
        }

        private static BrowseResult BrowseChildren(SPTreeNodeDto currentNode)
        {
            var result = new BrowseResult();
            var node = currentNode;
            try
            {
                if (node != null)
                {
                    Logger.Info($"Browse children. Current node is: {node}.");
                }
                var nodePath = new List<SPTreeNodeDto>();
                if (node != null && node.Level == NodeLevel.WebApplication)
                {
                    nodePath.Add(node);
                }
                while (node != null && node.Level > NodeLevel.WebApplication)
                {
                    nodePath.Insert(0, node);
                    node = node.Parent;
                }
                var browseContract = new SharePointBrowserContract
                {
                    ParentNodes = nodePath,
                    StartIndex = 0,
                    PerPage = int.MaxValue,
                    IsAdvancedSearchEnable = currentNode.IsAdvancedSearchEnable,
                    FilterPolicy = currentNode?.PolicyInfo,
                    IsBPOS = currentNode?.SPType == SPType.BPOS
                };

                var treeType = currentNode?.NodeExtension?.TreeType ?? TreeType.Undefined;
                var request = new BrowserMessage
                {
                    TreeType = treeType,
                    BrowserContract = browseContract
                };
                var response = SharePointBrowserMessageHandler.HandleMessage(request);
                if(response != null)
                {
                    var browserResult = response.BrowserContract as SharePointBrowserContract;
                    if (browserResult.HasError)
                    {
                        result.HasError = true;
                        Logger.Error("An error occurred while browsing node {0}", browserResult.Error);
                    }

                    if(browserResult.ChildenNodes != null)
                    {
                        var children = browserResult.ChildenNodes;
                        if(treeType == TreeType.RCUsageTree || treeType == TreeType.RCAuditReportTree || treeType == TreeType.RCAdminReportTree)
                        {
                            children = children?.Where(a => !a.Hidden).ToList();
                        }
                        if (currentNode != null && children != null)
                        {
                            Logger.Info("Browse children completed. Children number is " + children.Count + ".");
                            children.ForEach(c => { c.FarmID = currentNode.FarmID; c.ID = c.SPObjectId; });
                        }
                        //children.ForEach(c => { c.ID = SPObjectMappingService.GetInternalId(c.SPObjectId, c.Name, c.Level, c.FarmID); });
                        result.Children = children;
                        result.PageInfo = browserResult.PageInfo;
                        result.HasNextPage = browserResult.HasNextPage;
                        result.ChildrenCount = browserResult.ChildrenCount;

                        if (result.ChildrenCount == 0)
                        {
                            result.ChildrenCount = result.Children.Count;
                        }

                        if (currentNode != null && currentNode.Level == NodeLevel.Items)
                        {
                            result.ChildrenCount = currentNode.StartIndex + result.ChildrenCount;
                        }
                    }
                }
                else
                {
                    result.HasError = true;
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
                result.Children = new List<SPTreeNodeDto>();
                result.HasError = true;
            }
            return result;
        }

        //private static BrowseResult BrowseFarmNode()
        //{
        //    var result = new BrowseResult();
        //    var farm = SPFarm.Local;
        //    var node = new SPTreeNodeDto
        //    {
        //        SPType = SPType.Moss,
        //        ID = farm.Id.ToString(),
        //        SPObjectId = farm.Id.ToString(),
        //        Name = farm.Name,
        //        DisplayName = farm.DisplayName,
        //        Level = NodeLevel.Farm,
        //        Type = NodeType.Unused,
        //        CanChildrenBeLoaded = true,
        //        FarmID = farm.Id.ToString()
        //    };
        //    result.Children = new List<SPTreeNodeDto> { node };
        //    result.ChildrenCount = 1;
        //    return result;
        //}

        private static BrowseResult BrowseSite(SPTreeNodeDto currentNode)
        {
            var result = new BrowseResult
            {
                Children = new List<SPTreeNodeDto>()
            };
            var lists = CreateVirtualNode(NodeLevel.Lists, GConstants.SPNodeName.Lists, currentNode.SPType);
            lists.FarmID = currentNode.FarmID;
            lists.Offset = 0;
            result.Children.Add(lists);

            var sites = CreateVirtualNode(NodeLevel.Sites, GConstants.SPNodeName.Sites, currentNode.SPType);
            sites.FarmID = currentNode.FarmID;
            sites.Offset = lists.Offset + 1;
            result.Children.Add(sites);

            result.ChildrenCount = 2;

            return result;
        }

        private static BrowseResult BrowseFolder(SPTreeNodeDto currentNode)
        {
            var result = new BrowseResult
            {
                Children = new List<SPTreeNodeDto>()
            };
            var items = CreateVirtualNode(NodeLevel.Items, GConstants.SPNodeName.Items, currentNode.SPType);
            items.PageNodeType = PageNodeType.PreNext;
            items.FarmID = currentNode.FarmID;
            items.Offset = 0;

            var folders = CreateVirtualNode(NodeLevel.Folders, GConstants.SPNodeName.Folders, currentNode.SPType);
            folders.FarmID = currentNode.FarmID;
            folders.Offset = items.Offset + 1;

            result.Children.Add(items);
            result.Children.Add(folders);
            result.ChildrenCount = 2;

            return result;
        }

        private static void SetTreeCredentialPasswordEmpty(SPTreeNodeDto node)
        {
            if (node != null)
            {
                if (node.Level == NodeLevel.SiteCollection)
                {
                    if (node?.NodeExtension?.BposInfo?.UserAccountInfo != null)
                    {
                        node.NodeExtension.BposInfo.UserAccountInfo.Password = string.Empty;
                    }
                }
                else if (node.Level > NodeLevel.SiteCollection)
                {
                    SetTreeCredentialPasswordEmpty(node.Parent);
                }
            }
        }

        private static void SetNodesProperties(IList<SPTreeNodeDto> children, SPTreeNodeDto currentNode, SPType spType)
        {
            if (children != null)
            {
                foreach (SPTreeNodeDto child in children)
                {
                    if (child.Level != NodeLevel.ItemVersion && child.Level != NodeLevel.AppData)
                    {
                        child.CanChildrenBeLoaded = true;
                    }
                    child.SPType = spType;
                    if (currentNode != null && currentNode.Level >= NodeLevel.SiteCollection)
                    {
                        child.NodeExtension.CompatibilityLevel = currentNode.NodeExtension.CompatibilityLevel;
                        child.NodeExtension.FarmBuildVersion = currentNode.NodeExtension.FarmBuildVersion;
                        if (currentNode.SPType == SPType.Moss)
                        {
                            child.SPVersion = currentNode.SPVersion;
                            child.IsOnlineSite = currentNode.IsOnlineSite;
                        }
                    }
                }
            }
        }

        private static SPTreeNodeDto CreateVirtualNode(NodeLevel level, string name, SPType spType)
        {
            var id = Guid.NewGuid().ToString();
            return new SPTreeNodeDto
            {
                ID = id,
                SPObjectId = id,
                Name = name,
                Level = level,
                FullPath = "",
                SPType = spType
            };
        }
    }
}
