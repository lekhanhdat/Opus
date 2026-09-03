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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;

namespace RABox.Report
{
    public class BoxTreeScopeUtil
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(BoxTreeScopeUtil));

        private static readonly IRMBoxBrowser BoxBrowserService = PlatformWindsorManager.GetService<IRMBoxBrowser>();

        public static async Task<List<BoxTreeNodeDto>> AssembleAllTreeNodeForBoxAsync(BoxTreeNode boxTreeNode)
        {
            List<BoxTreeNodeDto> treeNodes = new List<BoxTreeNodeDto>();
            var allTempGroupChildrenIds = (await BoxBrowserService.BrowseAsync(boxTreeNode)).Select(gr => gr.Id).ToList();

            if (allTempGroupChildrenIds == null || allTempGroupChildrenIds.Count == 0)
            {
                _logger.Info($"No connection groups was found.");
                return treeNodes;
            }

            foreach (var groupNode in boxTreeNode.Children)
            {
                if (!allTempGroupChildrenIds.Contains(groupNode.Id))
                {
                    _logger.Info($"The group connection: [{groupNode.Id}-{groupNode.Name}] was removed.");
                    continue;
                }

                var allTempConnectionChildren = (await BoxBrowserService.BrowseAsync(groupNode)).ToList();

                if (allTempConnectionChildren == null || allTempConnectionChildren.Count == 0)
                {
                    _logger.Info($"No connection was found under the group node: [{groupNode.Id}-{groupNode.Name}].");
                    continue;
                }

                if (groupNode.CheckNumber == 1)
                {
                    _logger.Info($"The group connection [{groupNode.Id}-{groupNode.Name}] was fully selected.");
                    foreach (var connectionNode in allTempConnectionChildren)
                    {
                        connectionNode.CheckNumber = 1;
                        await ProcessConnectionNode(allTempConnectionChildren, connectionNode, groupNode, treeNodes);
                    }
                    continue;
                }

                if (groupNode.CheckNumber == 2 && groupNode.Children != null)
                {
                    _logger.Info($"The group connection [{groupNode.Id}-{groupNode.Name}] was half-selected.");
                    foreach (var connectionNode in groupNode.Children)
                    {
                        await ProcessConnectionNode(allTempConnectionChildren, connectionNode, groupNode, treeNodes, true);
                    }

                    foreach (var child in allTempConnectionChildren)
                    {
                        _logger.Info($"Process the newly added connection: [{child.Id}-{child.Name}]");
                        child.CheckNumber = 1;
                        await ProcessConnectionNode(allTempConnectionChildren, child, groupNode, treeNodes);
                    }
                    continue;
                }

                if (groupNode.Children != null)
                {
                    _logger.Info($"The group connection [{groupNode.Id}-{groupNode.Name}] was not selected. Process finding the selected sub-nodes");
                    foreach (var connectionNode in groupNode.Children)
                    {
                        await ProcessConnectionNode(allTempConnectionChildren, connectionNode, groupNode, treeNodes);
                    }
                }
            }
            return treeNodes;
        }

        private static bool HasSelectNodeForBox(BoxTreeNode current)
        {
            if (current.CheckNumber != 0) return true;
            if (current.Children == null || current.Children.Count == 0) return false;
            else
            {
                foreach (var child in current.Children)
                {
                    if (HasSelectNodeForBox(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private static async Task ProcessConnectionNode(List<BoxTreeNode> allTempConnectionChildren, BoxTreeNode connectionNode, BoxTreeNode groupNode, List<BoxTreeNodeDto> treeNodes, bool isIncludeNew = false)
        {
            if (!allTempConnectionChildren.Any(con => con.Id == connectionNode.Id))
            {
                _logger.Info($"The connection: [{connectionNode.Id}-{connectionNode.Name}] was removed from group: [{groupNode.Id}-{groupNode.Name}].");
                return;
            }

            if (isIncludeNew)
            {
                allTempConnectionChildren.RemoveAll(o => o.Id == connectionNode.Id);
            }

            var allTempUserChildren = (await BoxBrowserService.BrowseAsync(connectionNode)).ToList();

            if (connectionNode.CheckNumber == 1)
            {
                foreach (var userNode in allTempUserChildren)
                {
                    userNode.CheckNumber = 1;
                    _logger.Info($"Add user node [{userNode.Id}-{userNode.Name}] to the process node list.");
                    treeNodes.Add(RMDtoConverter.ConvertRMTree2BoxTree(userNode, null, true));
                }
                return;
            }

            if (HasSelectNodeForBox(connectionNode))
            {
                if (allTempUserChildren == null || allTempUserChildren.Count == 0)
                {
                    _logger.Info($"No user was found under the connection node: [{connectionNode.Id}-{connectionNode.Name}].");
                    return;
                }

                foreach (var userNode in connectionNode.Children)
                {
                    if (userNode.CheckNumber == 1)
                    {
                        if (!allTempUserChildren.Any(u => u.Id == userNode.Id))
                        {
                            _logger.Info($"The selected user: [{userNode.Id}-{userNode.Name}] was removed from connection: [{connectionNode.Id}-{connectionNode.Name}].");
                            continue;
                        }

                        _logger.Info($"Add user node [{userNode.Id}-{userNode.Name}] to the process node list.");
                        treeNodes.Add(RMDtoConverter.ConvertRMTree2BoxTree(userNode, null, true));
                    }

                    if (isIncludeNew)
                    {
                        allTempUserChildren.RemoveAll(o => o.Id == userNode.Id);
                    }
                }

                if (isIncludeNew)
                {
                    foreach (var userNode in allTempUserChildren)
                    {
                        userNode.CheckNumber = 1;
                        _logger.Info($"Add user node [{userNode.Id}-{userNode.Name}] to the process node list.");
                        treeNodes.Add(RMDtoConverter.ConvertRMTree2BoxTree(userNode, null, true));
                    }
                }
            }
        }
    }
}