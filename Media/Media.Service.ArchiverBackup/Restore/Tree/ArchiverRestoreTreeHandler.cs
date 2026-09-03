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




namespace AvePoint.Media.Service.ArchiverBackup
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Service.DomainModel;
    #endregion using directives

    public class ArchiverRestoreTreeHandler
        : RestoreServiceTreeHandlerBase
    {
        Dictionary<String, Boolean> currentVersionFlagMap = new Dictionary<String, Boolean>();

        public IArchiverRestoreIndexService RestoreIndexService { get; set; }

        protected override List<TreeNodeInfo> LoadFolders(TreeIndexParameter parameter, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null)
        {
            var indexInfo = new ArchiverIndexInfo()
            {
                Path = parameter.Path,
                EndTime = parameter.EndTime
            };
            var indexes = this.RestoreIndexService.LoadFolders(indexInfo, sql , searchContract);
            return indexes.ConvertAll(index => new TreeNodeInfo()
            {
                Name = index.Name,
                Type = index.Type,
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.ItemName,
                ItemVersionNumber = index.ItemVersionNumber,
            });
        }
        protected override List<TreeNodeInfo> LoadItems(TreeIndexParameter parameter, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null, bool isJustCalculateCount = false)
        {
            var indexInfo = new ArchiverIndexInfo()
            {
                Path = parameter.Path,
                EndTime = parameter.EndTime
            };
            var indexes = this.RestoreIndexService.LoadItems(indexInfo, sql, searchContract);

            // update retention for search all restore and in sendItem process with loaded items count > 0 for document or document version level
            if (!this.isPreview && sql != null && searchContract != null && !isJustCalculateCount && indexes.Count > 0
                && (searchContract.FilterPolicy.Level == PolicyLevel.Document 
                    || searchContract.FilterPolicy.Level == PolicyLevel.DocumentVersion
                   )
               )
            {
                this.RestoreIndexService.UpdateRetentionStatus(indexInfo.Path, indexInfo.EndTime, searchContract);
            }

            return indexes.ConvertAll(index => new TreeNodeInfo()
            {
                Name = index.Name,
                Type = index.Type,
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.ItemName,
                ItemVersionNumber = index.ItemVersionNumber,
            });
        }

        protected override List<TreeNodeInfo> LoadCurrentItems(TreeIndexParameter parameter)
        {
            var indexInfo = new ArchiverIndexInfo()
            {
                Path = parameter.Path,
                EndTime = parameter.EndTime,
                OffSet = parameter.OffSet,
                Length = parameter.Length,
            };
            var indexes = this.RestoreIndexService.LoadCurrentItems(indexInfo);

            return indexes.ConvertAll(index => new TreeNodeInfo()
            {
                Name = index.Name,
                Type = index.Type,
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.ItemName,
                ItemVersionNumber = index.ItemVersionNumber,
            });
        }

        protected override Int64 GetItemsCount(TreeIndexParameter parameter, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null)
        {
            return this.RestoreIndexService.GetItemsCount(parameter.Path, parameter.EndTime, sql, searchContract);
        }
        protected override List<ArchiverBasicIndex> LoadDocumentVersions(int topCount, string ItemId, long endTime, bool isRestoreAllVersions)
        {
            var result = this.RestoreIndexService.LoadItemVersionsByItemId(topCount, ItemId, endTime, isRestoreAllVersions);
            return result;
        }

        protected override Dictionary<string, List<ArchiverBasicIndex>> LoadDocumentVersionsByItemIds(int topCount, List<string> itemIds, long endTime, bool isRestoreAllVersions)
        {
            return this.RestoreIndexService.LoadItemVersionsByItemIds(topCount, itemIds, endTime, isRestoreAllVersions);
        }

        protected override TreeNodeInfo Load(TreeIndexParameter parameter)
        {
            var index = this.RestoreIndexService.Load(parameter.Path, parameter.EndTime);

            if(index == null)
            {
                throw new NullReferenceException(string.Format("Cannot find the index with the path:{0} and end time:{1}", parameter.Path, parameter.EndTime));
            }
            if (!this.isPreview && index.Type == "D" && index.RetentionStatus == (int)FilterDeletedType.Soft)
            {
                this.RestoreIndexService.UpdateRetentionStatus(parameter.Path, parameter.EndTime);
            }
            return new TreeNodeInfo
            {
                Name = index.Name,
                Type = index.Type,
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.ItemName,
                ItemVersionNumber = index.ItemVersionNumber,
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

        protected override Boolean IsSelectCurrentVersion(TreeNodeInfo info, List<SPTreeNodeDto> items, Boolean isInverted)
        {
            var isSelectCurrentVersion = default(Boolean);
            if (!info.Name.Equals(info.ItemName, StringComparison.OrdinalIgnoreCase))
            {
                if (this.currentVersionFlagMap.ContainsKey(info.ItemName))
                    isSelectCurrentVersion = this.currentVersionFlagMap[info.ItemName];
                else
                {
                    foreach (SPTreeNodeDto child in items)
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
        public void ProcessTreeNodeForApps(SPTreeNodeDto treeNode, ArchiverRestoreJob job)
        {
            SPTreeNodeDto node = null;
            for (Int32 nodeCount = 0; nodeCount < treeNode.Children.Count; nodeCount++)
            {
                node = treeNode.Children[nodeCount];
                switch (node.Level)
                {
                    case NodeLevel.Apps:
                        this.ProcessAppsNode(node, job);
                        break;
                    case NodeLevel.Sites:
                        this.ProcessSitesNode(node, job);
                        break;
                    case NodeLevel.App:
                        this.ProcessAppNode(node, job);
                        break;
                    default:
                        break;
                }
                ProcessTreeNodeForApps(node, job);
            }
        }

        private void ProcessAppsNode(SPTreeNodeDto appsNode, ArchiverRestoreJob job)
        {

            if (!appsNode.Expanded && appsNode.ChildrenCount <= 0 && appsNode.SelectAll == SelectAllState.Checked)
            {
                var parentFullPath = appsNode.Parent.Name.EqualsIgnoreCase(".") ? appsNode.Parent.Parent.FullPath : appsNode.Parent.FullPath;
                var sitesNode = appsNode.Parent.Children.Find(node => node.Level == NodeLevel.Sites);
                if (sitesNode != null)
                {
                    if (sitesNode.Expanded || sitesNode.ChildrenCount > 0 || sitesNode.SelectAll != SelectAllState.Checked)
                    {
                        var indexes = this.RestoreIndexService.LoadFolders(
                            new ArchiverIndexInfo()
                            {
                                Path = parentFullPath,
                                EndTime = job.BackupTime,
                                OffSet = 0,
                                Length = Int32.MaxValue - 1
                            });

                        var siteList = indexes.FindAll(index => index.Type == "P");
                        siteList.ForEach(index =>
                        {
                            var siteNode = this.ConvertIndexToSPTreeNode(index);
                            sitesNode.Children.Add(siteNode);
                            siteNode.Parent = sitesNode;
                        });
                    }
                }
                else
                {
                    sitesNode = this.GenerateSitesNode(appsNode.Parent);
                    var indexes = this.RestoreIndexService.LoadFolders(
                        new ArchiverIndexInfo()
                        {
                            Path = parentFullPath,
                            EndTime = job.BackupTime,
                            OffSet = 0,
                            Length = Int32.MaxValue - 1
                        });
                    var siteList = indexes.FindAll(index => index.Type == "P");
                    siteList.ForEach(index =>
                    {
                        var siteNode = this.ConvertIndexToSPTreeNode(index);
                        sitesNode.Children.Add(siteNode);
                        siteNode.Parent = sitesNode;
                    });
                }
            }
        }

        private void ProcessSitesNode(SPTreeNodeDto sitesNode, ArchiverRestoreJob job)
        {
            if (!sitesNode.Expanded && sitesNode.ChildrenCount <= 0 && sitesNode.SelectAll == SelectAllState.Checked)
            {
                String parentFullPath = sitesNode.Parent.Name.EqualsIgnoreCase(".") ? sitesNode.Parent.Parent.FullPath : sitesNode.Parent.FullPath;
                var appsNode = sitesNode.Parent.Children.Find(node => node.Level == NodeLevel.Apps);
                if (appsNode != null && (appsNode.Expanded || appsNode.ChildrenCount > 0 || appsNode.SelectAll != SelectAllState.Checked) && appsNode.Children.Count <= 0)
                {
                    sitesNode.Expanded = true;
                    var indexes = this.RestoreIndexService.LoadFolders(
                        new ArchiverIndexInfo()
                        {
                            Path = parentFullPath,
                            EndTime = job.BackupTime,
                            OffSet = 0,
                            Length = Int32.MaxValue - 1
                        });
                    var siteList = indexes.FindAll(index => index.Type == "W" && !index.Name.EqualsIgnoreCase("."));
                    siteList.ForEach(index =>
                    {
                        var siteNode = this.ConvertIndexToSPTreeNode(index);
                        sitesNode.Children.Add(siteNode);
                        siteNode.Parent = sitesNode;
                    });
                }
            }
        }

        private void ProcessAppNode(SPTreeNodeDto appNode, ArchiverRestoreJob job)
        {
            String parentFullPath = appNode.Parent.Parent.Name.EqualsIgnoreCase(".") ? appNode.Parent.Parent.Parent.FullPath : appNode.Parent.Parent.FullPath;
            var sitesNode = appNode.Parent.Parent.Children.Find(node => node.Level == NodeLevel.Sites);
            var appWebIndex = RestoreIndexService.GetAppWeb(
                new ArchiverIndexInfo()
                {
                    Path = appNode.FullPath,
                });

            if (!appNode.Expanded && appNode.ChildrenCount <= 0 && appNode.SelectAll == SelectAllState.Checked && appWebIndex != null)
                appNode.Children.Add(new SPTreeNodeDto());
            if (appNode.Children.Count > 0 && sitesNode == null)
                sitesNode = this.GenerateSitesNode(appNode.Parent.Parent);
            if (sitesNode != null)
            {
                if (sitesNode.Expanded || sitesNode.ChildrenCount > 0 || sitesNode.SelectAll != SelectAllState.Checked)
                {
                    if (appNode.Children.Count > 0)
                    {
                        appNode.Children.Clear();
                        var appWebNode = this.ConvertIndexToSPTreeNode(appWebIndex);
                        sitesNode.Children.Add(appWebNode);
                        appWebNode.Parent = sitesNode;
                    }
                }
                else
                {
                    sitesNode.Expanded = true;
                    var indexes = this.RestoreIndexService.LoadFolders(new ArchiverIndexInfo()
                    {
                        Path = parentFullPath,
                        EndTime = job.BackupTime,
                        OffSet = 0,
                        Length = Int32.MaxValue - 1
                    });
                    var siteList = indexes.FindAll(index => index.Type == "W" && !index.Name.EqualsIgnoreCase("."));
                    if (appNode.Children.Count > 0)
                    {
                        appNode.Children.Clear();
                        siteList.Add(indexes.Find(item => item.Name == appWebIndex.Name && item.Type == "P"));
                    }
                    siteList.ForEach(index =>
                    {
                        var siteNode = this.ConvertIndexToSPTreeNode(index);
                        sitesNode.Children.Add(siteNode);
                        siteNode.Parent = sitesNode;
                    });
                }
            }
        }

        private SPTreeNodeDto ConvertIndexToSPTreeNode(ArchiverBasicIndex index)
        {
            var result = new SPTreeNodeDto();
            result.CheckNumber = 1;
            result.Property = PropertyState.Checked;
            result.Security = SecurityState.Checked;
            result.SelectAll = SelectAllState.Checked;
            result.FullPath = index.SitePath + "\\" + index.Name;
            result.Name = index.Name.Substring(index.Name.LastIndexOfIgnoreCase("/") + 1);
            result.Level = index.Type.EqualsIgnoreCase("P") ? NodeLevel.AppData : NodeLevel.Site;
            return result;
        }

        private SPTreeNodeDto GenerateSitesNode(SPTreeNodeDto parentNode)
        {
            SPTreeNodeDto sitesNode = null;
            sitesNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Sites, Level = NodeLevel.Sites, CanChildrenBeLoaded = true, FarmID = parentNode.FarmID, FarmName = parentNode.FarmName, SPVersion = parentNode.SPVersion, Expanded = true };
            parentNode.Children.Add(sitesNode);
            sitesNode.Parent = parentNode;
            return sitesNode;
        }
    }
}