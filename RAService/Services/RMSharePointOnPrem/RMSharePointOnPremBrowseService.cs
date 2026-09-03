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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.RACommonUtility.SharePointOnPrem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMSharePointOnPrem
{
    public class RMSharePointOnPremBrowseService: RMServiceBase, IRMSharePointOnPremBrowseService
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(RMSharePointOnPremBrowseService));

        public async Task<List<SPTreeNodeDto>> BrowseAsync(SPTreeNodeDto parent)
        {
            if(parent == null)
            {
                parent = new SPTreeNodeDto { Level = NodeLevel.Root };
            }
            try
            {
                RMDtoConverter.ConvertSPTreeBeforeToJSON(parent);
                Logger.Info($"Begin browse sharepoint on-prem tree by node: Name - [{parent?.Name ?? "Root"}], Full Path - [{parent?.FullPath ?? "Root"}]");
                var message = await SharePointOnPremClient.BrowseAsync(new SPTreeMessage { Node = parent });
                if(message?.NodeList != null)
                {
                    foreach(var nodeDto in message.NodeList)
                    {
                        nodeDto.Parent = parent;
                        nodeDto.ParentId = parent.ID;
                    }
                }
                ArgumentCheck.NotNull(message, nameof(message));
                return message.NodeList;
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while browse sharepoint on-prem tree. Error: [{e}]");
                throw;
            }
        }

        public async Task<List<RMSPTreeNode>> BrowseReportTreeAsync(RMSPTreeNode parent, bool needCheckPermission = false)
        {
            var result = new List<RMSPTreeNode>();
            var nodes = await BrowseAsync(RMDtoConverter.ConvertRMTree2SPTree(parent));

            if(nodes == null)
            {
                return result;
            }
            ArgumentCheck.NotNull(parent, nameof(parent));
            VirtualNodeI18N(nodes, parent.Level);

            foreach (var node in nodes)
            {
                var child = RMDtoConverter.ConvertSPTree2RMTree(node);
                child.Parent = parent;
                result.Add(child);
            }

            return result;
        }

        public async Task<List<RMSPSampleTreeNode>> BrowseSampleTreeAsync(RMSPSampleTreeNode parent, bool needCheckPermission = false)
        {
            var result = new List<RMSPSampleTreeNode>();
            var nodes = await BrowseAsync(parent == null ? null : RMDtoConverter.ConvertRMSampleTree2SPTree(parent));
            if (nodes == null)
            {
                return result;
            }
            ArgumentCheck.NotNull(parent, nameof(parent));
            VirtualNodeI18N(nodes, parent.Level);

            foreach (var node in nodes)
            {
                var child = RMDtoConverter.ConvertSPTree2RMSampleTree(node);
                child.Parent = parent;
                child.ParentId = parent?.Id;
                result.Add(child);
            }
            return result;
        }
        public async Task<List<RMSPSampleTreeNode>> BrowseTreeAsync(RMSPTreeNode parent, bool needCheckPermission = false)
        {
            var result = new List<RMSPSampleTreeNode>();
            var tempParent = RMDtoConverter.ConvertRMTree2SPTree(parent);
            var nodes = await BrowseAsync(parent == null ? null : tempParent);
            if (nodes == null)
            {
                return result;
            }
            ArgumentCheck.NotNull(parent, nameof(parent));
            VirtualNodeI18N(nodes, parent.Level);

            foreach (var node in nodes)
            {
                var child = RMDtoConverter.ConvertSPTree2RMSampleTree(node);
                child.Parent = RMDtoConverter.ConvertSPTree2RMSampleTree(tempParent);
                child.ParentId = parent?.Id;
                result.Add(child);
            }
            return result;
        }
        private void VirtualNodeI18N(List<SPTreeNodeDto> nodes, int parentLevel)
        {
            foreach(var node in nodes)
            {
                if (parentLevel == (int)NodeLevel.Site)
                {
                    if (node.Name == "Lists")
                    {
                        node.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeLists");
                    }
                    if (node.Name == "Sites")
                    {
                        node.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeSites");
                    }
                }
                if (parentLevel == (int)NodeLevel.List)
                {
                    if (node.Name == "Root Folder")
                    {
                        node.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeRootFolder");
                    }
                }
                if (parentLevel == (int)NodeLevel.RootFolder || parentLevel == (int)NodeLevel.Folder)
                {
                    if (node.Name == "Folders")
                    {
                        node.Name = I18N.Core.I18NEntity.GetString("RM_SPS_SPTreeNodeFolders");
                    }
                }
            }
        }
    }
}
