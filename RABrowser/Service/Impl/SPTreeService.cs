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
using AvePoint.RA.Browser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Browser.Service.Impl
{
    public class SPTreeService : ITreeService
    {

        private static readonly string ADVANCED_SEARCH = "SPTreeAdvancedSearch";

        public SPTreeMessage Browse(SPTreeMessage request)
        {
            var response = new SPTreeMessage();

            var result = LoadChildren(request.Node,
                GConstants.TreeOperation.BROWSE,
                request.Node != null && request.Node.Level == NodeLevel.Items ? (uint)request.Length : uint.MaxValue,
                request.IsAPI);

            response.Node = request.Node;
            SetTreeCredentialPasswordEmpty(response.Node);
            response.NodeList = result.Children as List<SPTreeNodeDto>;
            response.PageInfo = result.PageInfo;
            response.ChildrenCount = result.ChildrenCount;
            response.HasNextPage = result.HasNextPage;
            response.HasError = result.HasError;
            response.Message = result.ErrorMessage;

            return response;
        }

        private BrowseResult LoadChildren(SPTreeNodeDto currentNode, int operation, uint perPage, bool isAPI = false)
        {
            var result = new BrowseResult();
            if (currentNode.Level == NodeLevel.Site)
            {
                result = BrowseSite(currentNode);
                SetNodesProperties(result.Children, currentNode, currentNode.SPType);
            }
            else if (currentNode.Level == NodeLevel.RootFolder || currentNode.Level == NodeLevel.Folder)
            {
                result = BrowseFolder(currentNode);
                SetNodesProperties(result.Children, currentNode, currentNode.SPType);
            }
            else
            {
                result = BrowseBPOSChildren(currentNode, perPage);
                SetNodesProperties(result.Children, currentNode, SPType.BPOS);
            }
            return result;
        }

        public SPTreeNodeDto FilterTreeBySearchString(SPTreeNodeDto node, string searchFilter)
        {
            throw new NotImplementedException();
        }

        public AveTreeMessage Refresh(AveTreeMessage request)
        {
            throw new NotImplementedException();
        }

        private BrowseResult BrowseSite(SPTreeNodeDto currentNode)
        {
            var result = new BrowseResult()
            {
                Children = new List<SPTreeNodeDto>()
            };

            var lists = CreateVirtualNode(NodeLevel.Lists, GConstants.SPNodeName.Lists, currentNode.SPType);
            lists.FarmID = currentNode.FarmID;
            lists.Offset = 0;
            lists.SPVersion = currentNode.SPVersion;

            var sites = CreateVirtualNode(NodeLevel.Sites, GConstants.SPNodeName.Sites, currentNode.SPType);
            sites.FarmID = currentNode.FarmID;
            sites.Offset = lists.Offset + 1;
            sites.SPVersion = currentNode.SPVersion;

            result.Children.Add(lists);
            result.Children.Add(sites);

            if (currentNode.SPVersion == GConstants.SPVersion.MOSS13)
            {
                var apps = CreateVirtualNode(NodeLevel.Apps, GConstants.SPNodeName.Apps, currentNode.SPType);
                apps.FarmID = currentNode.FarmID;
                apps.Offset = lists.Offset + 2;
                apps.SPVersion = currentNode.SPVersion;
                result.Children.Add(apps);
            }

            result.ChildrenCount = result.Children.Count;

            return result;
        }

        private BrowseResult BrowseFolder(SPTreeNodeDto currentNode)
        {
            var result = new BrowseResult() 
            { 
                Children = new List<SPTreeNodeDto>() 
            };

            var items = CreateVirtualNode(NodeLevel.Items, GConstants.SPNodeName.Items, currentNode.SPType);
            items.PageNodeType = PageNodeType.PreNext;
            items.FarmID = currentNode.FarmID;
            items.Offset = 0;
            items.SPVersion = currentNode.SPVersion;

            var folders = CreateVirtualNode(NodeLevel.Folders, GConstants.SPNodeName.Folders, currentNode.SPType);
            folders.FarmID = currentNode.FarmID;
            folders.Offset = items.Offset + 1;
            folders.SPVersion = currentNode.SPVersion;
            folders.SPVersion = currentNode.SPVersion;

            result.Children.Add(items);
            result.Children.Add(folders);
            result.ChildrenCount = result.Children.Count;

            return result;
        }

        private BrowseResult BrowseBPOSChildren(SPTreeNodeDto currentNode, uint perPage)
        {
            var siteCollectionNode = currentNode;

            while(siteCollectionNode != null && siteCollectionNode.Level != NodeLevel.SiteCollection)
            {
                siteCollectionNode = siteCollectionNode.Parent;
            }

            if(siteCollectionNode != null)
            {
                return BrowseChildrenFromBrowser(currentNode, perPage);
            }
            return new BrowseResult();
        }

        private SPTreeNodeDto CreateVirtualNode(NodeLevel level, string name, SPType spType)
        {
            var id = Guid.NewGuid().ToString();
            var virtualNode = new SPTreeNodeDto()
            {
                ID = id,
                SPObjectId = id,
                Name = name,
                DisplayName = name,
                Level = level,
                FullPath = "",
                SPType = spType
            };
            return virtualNode;
        }

        private void SetNodesProperties(IList<SPTreeNodeDto> children, SPTreeNodeDto currentNode, SPType spType)
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
                    if (currentNode != null)
                    {
                        if (currentNode.SPType == SPType.Moss || currentNode.Level >= NodeLevel.SiteCollection)
                        {
                            child.SPVersion = currentNode.SPVersion;
                        }
                    }
                }
            }
        }

        private void SetTreeCredentialPasswordEmpty(SPTreeNodeDto node)
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

        private readonly AgentBrowserService browserService = new AgentBrowserService();

        private BrowseResult BrowseChildrenFromBrowser(SPTreeNodeDto currentNode, uint perPage)
        {
            var result = new BrowseResult();
            var node = currentNode;
            try
            {
                if(node != null)
                {
                    //log
                }

                var nodePath = new List<SPTreeNodeDto>();
                if (node != null && node.Level == NodeLevel.WebApplication)
                {
                    nodePath.Add(node);
                }

                while(node != null && node.Level > NodeLevel.WebApplication)
                {
                    nodePath.Insert(0, node);
                    node = node.Parent;
                }

                var browseContract = new SharePointBrowserContract();
                if(currentNode == null || currentNode.NodeExtension.AgentType == null)
                {
                    browseContract.AgentType = string.Empty;
                }
                else
                {
                    browseContract.AgentType = currentNode.NodeExtension.AgentType;
                }
                browseContract.ParentNodes = nodePath;
                browseContract.StartIndex = currentNode == null ? 0 : currentNode.StartIndex;
                browseContract.PageInfo = currentNode?.PageInfo;
                browseContract.PerPage = perPage;
                if(currentNode != null && (currentNode.Level == NodeLevel.Sites || currentNode.Level == NodeLevel.Lists || currentNode.Level == NodeLevel.Folders))
                {
                    browseContract.IsAdvancedSearchEnable = currentNode.IsAdvancedSearchEnable;
                }

                if(currentNode.PolicyInfo != null)
                {
                    browseContract.FilterPolicy = currentNode.PolicyInfo;
                    browseContract.AgentType = ADVANCED_SEARCH;
                }

                if(currentNode != null && currentNode.SPType == SPType.BPOS)
                {
                    browseContract.IsBPOS = true;
                }

                var treeType = currentNode.NodeExtension?.TreeType ?? TreeType.Undefined;
                var request = new BrowserMessage()
                {
                    TreeType = treeType,
                    BrowserContract = browseContract
                };
                var response = browserService.HandleMessage(request);
                if(response != null)
                {
                    var browserResult = response.BrowserContract as SharePointBrowserContract;
                    if (browseContract.HasError)
                    {

                    }
                    else
                    {
                        List<SPTreeNodeDto> children = browserResult.ChildenNodes;
                        if (currentNode != null && children != null)
                        {
                            //logger.Info("Browse children completed. Children number is " + children.Count + ".");
                            children.ForEach(c => { c.FarmID = currentNode.FarmID; c.ID = Guid.NewGuid().ToString();  c.SPVersion = currentNode.SPVersion; });
                        }
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
            }
            catch(Exception e)
            {

            }
            return result;
        }

    }
}
