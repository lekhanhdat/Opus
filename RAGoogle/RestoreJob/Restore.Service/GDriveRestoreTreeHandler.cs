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




namespace RAGoogle.Restore.Service
{
    #region using directives
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Tree;
    using AvePoint.Media.Service.DomainModel;
    using System;
    using System.Collections.Generic;
    #endregion using directives

    public class GDriveRestoreTreeHandler
        : GDriveRestoreServiceTreeHandlerBase
    {
        Dictionary<String, Boolean> currentVersionFlagMap = new Dictionary<String, Boolean>();

        protected override List<TreeNodeInfo> LoadFolders(TreeIndexParameter parameter)
        {
            var indexInfo = new ArchiverIndexInfo()
            {
                Path = parameter.Path,
                EndTime = parameter.EndTime
            };
            var indexes = this.RestoreIndexService.LoadFolders(indexInfo);
            return indexes.ConvertAll(index => new TreeNodeInfo()
            {
                Name = index.Name,
                Type = index.Type.ToString(),
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.Name,
                ItemVersionNumber = index.Version,
            });
        }

        protected override List<TreeNodeInfo> LoadItems(TreeIndexParameter parameter)
        {
            var indexInfo = new ArchiverIndexInfo()
            {
                Path = parameter.Path,
                EndTime = parameter.EndTime
            };
            var indexes = this.RestoreIndexService.LoadItems(indexInfo);
            return indexes.ConvertAll(index => new TreeNodeInfo()
            {
                Name = index.Name,
                Type = index.Type.ToString(),
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.Name,
                ItemVersionNumber = index.Version,
            });
        }

        protected override Int64 GetItemsCount(TreeIndexParameter parameter)
        {
            return this.RestoreIndexService.GetItemsCount(parameter.Path, parameter.EndTime);
        }
        protected override List<GoogleBasicIndex> LoadDocumentVersions(int topCount, string ItemId, long endTime)
        {
            var result = this.RestoreIndexService.LoadItemVersionsByItemId(topCount, ItemId, endTime);
            return result;
        }
        protected override TreeNodeInfo Load(TreeIndexParameter parameter)
        {
            var index = this.RestoreIndexService.Load(parameter.Path, parameter.EndTime);

            if (index == null)
            {
                throw new NullReferenceException(string.Format("Cannot find the index with the path:{0} and end time:{1}", parameter.ItemId, parameter.EndTime));
            }
            if (index.Type == (int)GDriveDataType.File && index.RetentionStatus == (int)FilterDeletedType.Soft)
            {
                this.RestoreIndexService.UpdateRetentionStatus(parameter.Path, parameter.EndTime);
            }
            return new TreeNodeInfo
            {
                Name = index.Name,
                Type = index.Type.ToString(),
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.Name,
                ItemVersionNumber = index.Version,
            };
        }

        protected override Boolean IsSelectCurrentVersion(TreeNodeInfo info, Dictionary<String, Boolean> currentVersionFlagMap, List<TreeNodeInfo> items)
        {
            var isSelectCurrentVersion = default(Boolean);
            if (!info.Name.Equals(info.ItemName, StringComparison.OrdinalIgnoreCase))
            {
                if (currentVersionFlagMap.ContainsKey(info.ItemName))
                    isSelectCurrentVersion = currentVersionFlagMap[info.ItemName];
                else
                {
                    foreach (TreeNodeInfo child in items)
                    {
                        if (info.ItemName.Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            isSelectCurrentVersion = true;
                            break;
                        }
                    }
                    currentVersionFlagMap.Add(info.ItemName, isSelectCurrentVersion);
                }
            }
            else
                isSelectCurrentVersion = true;
            return isSelectCurrentVersion;
        }

        protected override Boolean IsSelectCurrentVersion(TreeNodeInfo info, List<GoogleDriveTreeNodeDto> items, Boolean isInverted)
        {
            var isSelectCurrentVersion = default(Boolean);
            if (!info.Name.Equals(info.ItemName, StringComparison.OrdinalIgnoreCase))
            {
                if (this.currentVersionFlagMap.ContainsKey(info.ItemName))
                    isSelectCurrentVersion = this.currentVersionFlagMap[info.ItemName];
                else
                {
                    foreach (GoogleDriveTreeNodeDto child in items)
                    {
                        if (info.ItemName.Equals(child.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            isSelectCurrentVersion = isInverted ? false : true;
                            break;
                        }
                    }
                    this.currentVersionFlagMap.Add(info.ItemName, isSelectCurrentVersion);
                }
            }
            else
                isSelectCurrentVersion = true;
            return isSelectCurrentVersion;
        }

        protected override List<TreeNodeInfo> LoadItemAndVersions(TreeIndexParameter parameter)
        {
            var info = this.Load(parameter);
            var result = new List<TreeNodeInfo>();
            result.Add(info);
            return result;
        }
        //public void ProcessTreeNodeForApps(GoogleDriveTreeNodeDto treeNode, GDriveRestoreJob job)
        //{
        //    GoogleDriveTreeNodeDto node = null;
        //    for (Int32 nodeCount = 0; nodeCount < treeNode.Children.Count; nodeCount++)
        //    {
        //        node = treeNode.Children[nodeCount];
        //        switch (node.Level)
        //        {
        //            case NodeLevel.GoogleMyDrive:
        //            case NodeLevel.GoogleSharedDrive:
        //                this.ProcessSitesNode(node, job);
        //                break;
        //            default:
        //                break;
        //        }
        //        ProcessTreeNodeForApps(node, job);
        //    }
        //}

        //private void ProcessSitesNode(GoogleDriveTreeNodeDto sitesNode, GDriveRestoreJob job)
        //{
        //    if (!sitesNode.Expanded && sitesNode.ChildrenCount <= 0 && sitesNode.SelectAll == SelectAllState.Checked)
        //    {
        //        String parentFullPath = sitesNode.Parent.Name.EqualsIgnoreCase(".") ? sitesNode.Parent.Parent.FullPath : sitesNode.Parent.FullPath;
        //        var appsNode = sitesNode.Parent.Children.Find(node => node.Level == NodeLevel.Apps);
        //        if (appsNode != null && (appsNode.Expanded || appsNode.ChildrenCount > 0 || appsNode.SelectAll != SelectAllState.Checked) && appsNode.Children.Count <= 0)
        //        {
        //            sitesNode.Expanded = true;
        //            var indexes = this.RestoreIndexService.LoadFolders(
        //                new ArchiverIndexInfo()
        //                {
        //                    Path = parentFullPath,
        //                    EndTime = job.BackupTime,
        //                    OffSet = 0,
        //                    Length = Int32.MaxValue - 1
        //                });
        //            var siteList = indexes.FindAll(index => index.Type == "W" && !index.Name.EqualsIgnoreCase("."));
        //            siteList.ForEach(index =>
        //            {
        //                var siteNode = this.ConvertIndexToSPTreeNode(index);
        //                sitesNode.Children.Add(siteNode);
        //                siteNode.Parent = sitesNode;
        //            });
        //        }
        //    }
        //}

        //private GoogleDriveTreeNodeDto ConvertIndexToSPTreeNode(GoogleBasicIndex index)
        //{
        //    var result = new GoogleDriveTreeNodeDto();
        //    result.CheckNumber = 1;
        //    result.Property = PropertyState.Checked;
        //    result.Security = SecurityState.Checked;
        //    result.SelectAll = SelectAllState.Checked;
        //    result.FullPath = index.SitePath + "\\" + index.Name;
        //    result.Name = index.Name.Substring(index.Name.LastIndexOfIgnoreCase("/") + 1);
        //    result.Level = index.Type.EqualsIgnoreCase("P") ? NodeLevel.AppData : NodeLevel.Site;
        //    return result;
        //}

        //private GoogleDriveTreeNodeDto GenerateSitesNode(GoogleDriveTreeNodeDto parentNode)
        //{
        //    GoogleDriveTreeNodeDto sitesNode = null;
        //    sitesNode = new GoogleDriveTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Sites, Level = NodeLevel.Sites, CanChildrenBeLoaded = true, FarmID = parentNode.FarmID, FarmName = parentNode.FarmName, SPVersion = parentNode.SPVersion, Expanded = true };
        //    parentNode.Children.Add(sitesNode);
        //    sitesNode.Parent = parentNode;
        //    return sitesNode;
        //}
    }
}