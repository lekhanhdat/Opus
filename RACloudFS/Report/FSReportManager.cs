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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.Report
{
    public class FSReportManager
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(FSCreationAndDestroyedFileReportProcessor));

        private static readonly IRMReportService ReportService = PlatformWindsorManager.GetService<IRMReportService>();

        private static readonly IRMFileSystemBrowserService FileSystemBrowserService = PlatformWindsorManager.GetService<IRMFileSystemBrowserService>();

        private static RMProfileDto ProfileDto { get; set; }
        private static RMFSTreeNode FSTreeNode { get; set; }

        private List<FSTreeNodeDto> SelectedTreeNodes = new List<FSTreeNodeDto>();
        private List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> SelectedTreeNodes4Agent = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>();

        private List<Guid> GroupIds = new List<Guid>();

        public FSReportManager(string profileId, AvePoint.RA.Contract.JobMonitor.JobType jobType)
        {
            ProfileDto = ReportService.GetProfileByIdAsync(profileId).Result;
            if(jobType == AvePoint.RA.Contract.JobMonitor.JobType.FSBCSTermUsageReport)
            {
                FSTreeNode = SerializerHelper.DeserializeByDataContractSerializer<RMFSTreeNode>(ProfileDto.Extension2);
            }
            else
            {
                FSTreeNode = SerializerHelper.DeserializeByJsonSerializer<RMFSTreeNode>(RuleSPTreeUtil.BuildFSTreeJsonStr(ProfileDto.Extension2));
            }         
        }
        public async Task<List<FSTreeNodeDto>> AssembleAllTreeNodeForFSAsync()
        {
            List<FSTreeNodeDto> treeNodes = new List<FSTreeNodeDto>();
            foreach (var tempGroup in FSTreeNode.Children)
            {
                var allTempGroupChildren = await FileSystemBrowserService.FSBrowseAsync(tempGroup);
                if (tempGroup.CheckNumber == 1)
                {
                    allTempGroupChildren.ForEach(o => o.CheckNumber = 1);
                    foreach(var child in allTempGroupChildren)
                    {
                        treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                    }                   
                    if (!GroupIds.Contains(tempGroup.ConnGroupId))
                    {
                        GroupIds.Add(tempGroup.ConnGroupId);
                    }
                }
                else if (tempGroup.CheckNumber == 2)
                {
                    if (tempGroup.Children != null)
                    {
                        foreach (var child in tempGroup.Children)
                        {
                            if (HasSelectNodeForFS(child))
                            {
                                allTempGroupChildren.Remove(allTempGroupChildren.Where(o => o.Id == child.Id).FirstOrDefault());
                                GetSelectedNodeForFS(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                                treeNodes.AddRange(SelectedTreeNodes);
                                SelectedTreeNodes.Clear();
                            }
                            else
                            {
                                allTempGroupChildren.Remove(allTempGroupChildren.Where(o => o.Id == child.Id).FirstOrDefault());
                            }
                        }
                        allTempGroupChildren.ForEach(a => a.CheckNumber = 1);
                        foreach(var child in allTempGroupChildren)
                        {
                            treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                        }                       
                        if (!GroupIds.Contains(tempGroup.ConnGroupId))
                        {
                            GroupIds.Add(tempGroup.ConnGroupId);
                        }
                    }
                }
                else
                {
                    if (tempGroup.Children != null)
                    {
                        foreach (var path in tempGroup.Children)
                        {
                            if (HasSelectNodeForFS(path))
                            {
                                FSTreeNodeDto fsPathNode = RMDtoConverter.ConvertRMTree2FSTree(path, null, true);
                                GetSelectedNodeForFS(fsPathNode);
                                treeNodes.AddRange(SelectedTreeNodes);
                                SelectedTreeNodes.Clear();
                            }
                            else
                            {
                                logger.Debug("No select node");
                            }
                        }
                    }
                    if (!GroupIds.Contains(tempGroup.ConnGroupId))
                    {
                        GroupIds.Add(tempGroup.ConnGroupId);
                    }
                }
            }
            return treeNodes;
        }
        public async Task<List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>> AssembleAllTreeNodeForFSAgentAsync()
        {
            List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto> treeNodes = new List<AvePoint.RA.Contract.Global.Object.FSTreeNodeDto>();
            foreach (var tempGroup in FSTreeNode.Children)
            {
                var allTempGroupChildren = await FileSystemBrowserService.FSBrowseAsync(tempGroup);
                if (tempGroup.CheckNumber == 1)
                {
                    allTempGroupChildren.ForEach(o => o.CheckNumber = 1);
                    foreach (var child in allTempGroupChildren)
                    {
                        treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree4AGent(child, null, true));
                    }
                    if (!GroupIds.Contains(tempGroup.ConnGroupId))
                    {
                        GroupIds.Add(tempGroup.ConnGroupId);
                    }
                }
                else if (tempGroup.CheckNumber == 2)
                {
                    if (tempGroup.Children != null)
                    {
                        foreach (var child in tempGroup.Children)
                        {
                            if (HasSelectNodeForFS(child))
                            {
                                allTempGroupChildren.Remove(allTempGroupChildren.Where(o => o.Id == child.Id).FirstOrDefault());
                                GetSelectedNodeForFS(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                                treeNodes.AddRange(SelectedTreeNodes4Agent);
                                SelectedTreeNodes.Clear();
                            }
                            else
                            {
                                allTempGroupChildren.Remove(allTempGroupChildren.Where(o => o.Id == child.Id).FirstOrDefault());
                            }
                        }
                        allTempGroupChildren.ForEach(a => a.CheckNumber = 1);
                        foreach (var child in allTempGroupChildren)
                        {
                            treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree4AGent(child, null, true));
                        }
                        if (!GroupIds.Contains(tempGroup.ConnGroupId))
                        {
                            GroupIds.Add(tempGroup.ConnGroupId);
                        }
                    }
                }
                else
                {
                    if (tempGroup.Children != null)
                    {
                        foreach (var path in tempGroup.Children)
                        {
                            if (HasSelectNodeForFS(path))
                            {
                                FSTreeNodeDto fsPathNode = RMDtoConverter.ConvertRMTree2FSTree(path, null, true);
                                GetSelectedNodeForFS(fsPathNode);
                                treeNodes.AddRange(SelectedTreeNodes4Agent);
                                SelectedTreeNodes.Clear();
                            }
                            else
                            {
                                logger.Debug("No select node");
                            }
                        }
                    }
                    if (!GroupIds.Contains(tempGroup.ConnGroupId))
                    {
                        GroupIds.Add(tempGroup.ConnGroupId);
                    }
                }
            }
            return treeNodes;
        }
        public List<Guid> GetAllGroupIds()
        {
            return GroupIds;
        }

        public async Task<List<FSTreeNodeDto>> GetSelectedConnectionsAsync()
        {
            List<FSTreeNodeDto> treeNodes = new List<FSTreeNodeDto>();
            foreach (var tempGroup in FSTreeNode.Children)
            {
                tempGroup.Parent = FSTreeNode;
                var allTempGroupChildren = await FileSystemBrowserService.FSBrowseAsync(tempGroup);
                if (tempGroup.CheckNumber == 1)
                {
                    allTempGroupChildren.ForEach(o => o.CheckNumber = 1);
                    foreach (var child in allTempGroupChildren)
                    {
                        child.Parent = tempGroup;
                        treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                    }
                    if (!GroupIds.Contains(tempGroup.ConnGroupId))
                    {
                        GroupIds.Add(tempGroup.ConnGroupId);
                    }
                }
                else if (tempGroup.CheckNumber == 2)
                {
                    if (tempGroup.Children != null)
                    {
                        foreach (var child in tempGroup.Children)
                        {
                            if (HasSelectNodeForFS(child))
                            {
                                allTempGroupChildren.Remove(allTempGroupChildren.Where(o => o.Id == child.Id).FirstOrDefault());
                                GetSelectedNodeForFS(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                                treeNodes.AddRange(SelectedTreeNodes);
                                SelectedTreeNodes.Clear();
                            }
                            else
                            {
                                allTempGroupChildren.Remove(allTempGroupChildren.Where(o => o.Id == child.Id).FirstOrDefault());
                            }
                        }
                        allTempGroupChildren.ForEach(a => a.CheckNumber = 1);
                        foreach (var child in allTempGroupChildren)
                        {
                            child.Parent = tempGroup;
                            treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree(child, null, true));
                        }
                        if (!GroupIds.Contains(tempGroup.ConnGroupId))
                        {
                            GroupIds.Add(tempGroup.ConnGroupId);
                        }
                    }
                }
                else
                {
                    if (tempGroup.Children != null)
                    {
                        foreach (var path in tempGroup.Children)
                        {
                            if (HasSelectNodeForFS(path))
                            {
                                path.Parent = tempGroup;
                                treeNodes.Add(RMDtoConverter.ConvertRMTree2FSTree(path, null, true));
                            }
                            else
                            {
                                logger.Debug("No select connection");
                            }
                        }
                    }
                    if (!GroupIds.Contains(tempGroup.ConnGroupId))
                    {
                        GroupIds.Add(tempGroup.ConnGroupId);
                    }
                }
            }
            return treeNodes;
        }

        private bool HasSelectNodeForFS(RMFSTreeNode current)
        {
            if (current.CheckNumber != 0)
            {
                return true;
            }
            if (current.Children.IsNullOrEmpty())
            {
                return false;
            }
            else
            {
                foreach (RMFSTreeNode child in current.Children)
                {
                    if (HasSelectNodeForFS(child))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private void GetSelectedNodeForFS(FSTreeNodeDto current)
        {
            if (current.CheckNumber == 1)
            {
                SelectedTreeNodes.Add(current);
            }
            else
            {
                if (!current.Children.IsNullOrEmpty())
                {
                    foreach (FSTreeNodeDto child in current.Children)
                    {
                        GetSelectedNodeForFS(child);
                    }
                }
            }
        }
    }
}
