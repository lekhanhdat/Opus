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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using RAGoogle.Helper;

namespace RAGoogle.Report
{
    public class GoogleTreeScopeUtil
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(GoogleTreeScopeUtil));

        private static IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();

        public static async Task<List<GoogleDriveTreeNodeDto>> AssembleAllTreeNodeForGoogleAsync(RMGoogleTreeNode rootNode)
        {
            List<GoogleDriveTreeNodeDto> treeNodes = new List<GoogleDriveTreeNodeDto>();
            var allContainerChildrenIds = (await RemoteGoogleNodeService.BrowserRMTreeAsync(rootNode)).Select(gr => gr.Id).ToList();

            if (allContainerChildrenIds.IsNullOrEmpty())
            {
                logger.Info($"No containers was found.");
                return treeNodes;
            }

            foreach (var containerNode in rootNode.Children)
            {
                if (!allContainerChildrenIds.Contains(containerNode.Id))
                {
                    logger.Info($"The container: [{containerNode.Id}-{containerNode.Name}] was removed.");
                    continue;
                }

                var allTempDriveChildren = (await RemoteGoogleNodeService.BrowserRMTreeAsync(containerNode)).ToList();

                if (allTempDriveChildren == null || allTempDriveChildren.Count == 0)
                {
                    logger.Info($"No driver was found under the container node: [{containerNode.Id}-{containerNode.Name}].");
                    continue;
                }

                if (containerNode.CheckNumber == 1)
                {
                    logger.Info($"The container node [{containerNode.Id}-{containerNode.Name}] was fully selected.");
                    foreach (var driveNode in allTempDriveChildren)
                    {
                        driveNode.CheckNumber = 1;
                        ProcessDriveNode(allTempDriveChildren, driveNode, containerNode, treeNodes);

                    }
                    continue;
                }

                if (containerNode.CheckNumber == 2 && containerNode.Children.IsNotNullOrEmpty())
                {
                    logger.Info($"The container node [{containerNode.Id}-{containerNode.Name}] was half-selected.");
                    foreach (var driveNode in containerNode.Children)
                    {
                        ProcessDriveNode(allTempDriveChildren, driveNode, containerNode, treeNodes, true);
                    }
                    foreach (var newDriveNode in allTempDriveChildren)
                    {
                        logger.Info($"Process the newly added drive: [{newDriveNode.Id}-{newDriveNode.Name}]");
                        newDriveNode.CheckNumber = 1;
                        ProcessDriveNode(allTempDriveChildren, newDriveNode, containerNode, treeNodes);

                    }
                    continue;
                }

                if (containerNode.Children != null)
                {
                    logger.Info($"The container node [{containerNode.Id}-{containerNode.Name}] was not selected. Process finding the selected sub-nodes");
                    foreach (var driveNode in containerNode.Children)
                    {
                        ProcessDriveNode(allTempDriveChildren, driveNode, containerNode, treeNodes);
                    }
                }
            }

            return treeNodes;
        }

        private static void ProcessDriveNode(List<RMGoogleTreeNode> allDriveChildren, RMGoogleTreeNode driveNode, RMGoogleTreeNode containerNode, List<GoogleDriveTreeNodeDto> treeNodes, bool isIncludeNew = false)
        {
            if (!allDriveChildren.Any(c => c.Id == driveNode.Id))
            {
                logger.Info($"The drive: [{driveNode.Id}-{driveNode.Name}] was removed from container: [{containerNode.Id}-{containerNode.Name}].");
                return;
            }

            if (isIncludeNew)
            {
                allDriveChildren.RemoveAll(c => c.Id == driveNode.Id);
            }

            if (driveNode.CheckNumber == 1)
            {
                logger.Info($"Add drive node [{driveNode.Id}-{driveNode.Name}] to the process node list.");
                treeNodes.Add(ConvertHelper.ConvertGoogleRM2Dto(driveNode));
            }
        }
    }
}