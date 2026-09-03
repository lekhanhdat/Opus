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
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.StorageOptimization.Connector;
    using AvePoint.GCommon.Contract.Tree;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.Media.Service;
    using AvePoint.Media.Service.ArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using Merged18NResources.MediaServiceApplicationModel;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    #endregion

    public abstract class GDriveRestoreServiceTreeHandlerBase : IGDriveRestoreServiceTreeHandler
    {
        readonly static Object syncIndexItemProceedObject = new Object();
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<String, Boolean> currentVersionFlagMap = new Dictionary<String, Boolean>();
        String currentSiteCollectionUrl;
        Boolean isJustCalculateCount;
        public GDriveRestoreJob restoreJob { get; set; }
        List<IndexItemProceedEventArgs> attachementInfos;
        public IGDriveArchiverRestoreIndexService RestoreIndexService { get; set; }
        EventHandler<IndexItemProceedEventArgs> indexItemProceed;
        public event EventHandler<IndexItemProceedEventArgs> IndexItemProceed
        {
            add
            {
                lock (syncIndexItemProceedObject)
                {
                    this.indexItemProceed += value;
                }
            }
            remove
            {
                lock (syncIndexItemProceedObject)
                {
                    this.indexItemProceed -= value;
                }
            }
        }

        public GoogleDriveTreeNodeDto CutTree(GoogleDriveTreeNodeDto rootTree)
        {
            GoogleDriveTreeNodeDto treeNodeDto = null;
            if (rootTree.Level == NodeLevel.Items)
            {
                if (rootTree.CheckNumber == 1 || rootTree.SelectAll == SelectAllState.Checked)
                    treeNodeDto = rootTree;
                else
                {
                    if (rootTree.ChildrenCount > 0)
                    {
                        foreach (GoogleDriveTreeNodeDto item in rootTree.Children)
                        {
                            if (item.CheckNumber == 1)
                            {
                                treeNodeDto = rootTree;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                List<GoogleDriveTreeNodeDto> children = new List<GoogleDriveTreeNodeDto>();
                foreach (GoogleDriveTreeNodeDto child in rootTree.Children)
                {
                    GoogleDriveTreeNodeDto selectedNodeDto = CutTree(child);
                    if (selectedNodeDto != null)
                        children.Add(selectedNodeDto);
                }
                rootTree.Children.Clear();
                rootTree.Children.AddRange(children);
                if (rootTree.CheckNumber == 1 || rootTree.Children.Count > 0)
                    treeNodeDto = rootTree;
            }
            return treeNodeDto;
        }

        public void ProcessTreeNode(TreeNodeParameter treeParam)
        {
            //this.restoreJob = this.restoreJob;
            this.isJustCalculateCount = treeParam.IsJustCalculateCount;
            this.currentSiteCollectionUrl = treeParam.GoogleDriveTree.FullPath;
            this.attachementInfos = new List<IndexItemProceedEventArgs>();
            this.ProcessNodeDtoInternal(treeParam.GoogleDriveTree);
        }

        protected virtual void OnIndexItemProceed(IndexItemProceedEventArgs args)
        {
            var temp = indexItemProceed;
            if (temp != null) temp(this, args);
        }


        protected abstract List<TreeNodeInfo> LoadFolders(TreeIndexParameter parameter);

        protected abstract List<TreeNodeInfo> LoadItems(TreeIndexParameter parameter);

        protected abstract long GetItemsCount(TreeIndexParameter parameter);

        protected abstract TreeNodeInfo Load(TreeIndexParameter parameter);
        protected abstract List<GoogleBasicIndex> LoadDocumentVersions(int topCount, string ItemId, long endTime);

        protected abstract List<TreeNodeInfo> LoadItemAndVersions(TreeIndexParameter parameter);

        protected abstract Boolean IsSelectCurrentVersion(TreeNodeInfo info, Dictionary<String, Boolean> currentVersionFlagMap, List<TreeNodeInfo> items);

        protected abstract Boolean IsSelectCurrentVersion(TreeNodeInfo info, List<GoogleDriveTreeNodeDto> items, Boolean isInverted);

        private void ProcessNodeDtoInternal(GoogleDriveTreeNodeDto nodeDto)
        {

            MediaRestoreNode node = new MediaRestoreNode(nodeDto);
            string currentPath = string.Empty;
            if (!node.IsVirtualNode)
            {
                var path = $"{nodeDto.PathMD5}";
                this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessNodeDtoInternalProcessCurrentNode, path);
                if (node.Level == NodeLevel.GoogleFile)
                {
                    ProcessSelectedItemWithVersions(nodeDto);
                }
                else
                {
                    TreeNodeInfo info = Load(new TreeIndexParameter { Path = path, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });

                    OnIndexItemProceed(new IndexItemProceedEventArgs
                    {
                        IndexCount = 1,
                        IndexItem = info.Index,
                        MarkMessage = new RestoreMarkMessage()
                        {
                            Security = SecurityState.Checked,
                            Property = PropertyState.Checked,
                            IsChecked = true,
                            VersionFlag = 1,
                            IsSelected = nodeDto.CheckNumber == 1,
                            ParentIsSelected = NodeUtil.CheckParentWasChecked(nodeDto)
                        }
                    });
                }
            }
            if (node.IsExpanded)//真实的tree被展开
            {
                foreach (GoogleDriveTreeNodeDto subNode in nodeDto.Children)
                {
                    ProcessNodeDtoInternal(subNode);
                }

            }
            else//真实的tree没有展开
            {
                if (node.IsChecked)//check number一定是1
                {
                    this.ProcessSubContainers(nodeDto.PathMD5, nodeDto);
                }
                else
                    throw new NodeCheckStateException(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessNodeDtoInternalCheckStateError);
            }
        }

        void ProcessSubContainers(String currentPath, GoogleDriveTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSubContainersInfo, currentPath);
            if (nodeDto.Level == NodeLevel.List || nodeDto.Level == NodeLevel.RootFolder || nodeDto.Level == NodeLevel.Folder || nodeDto.Level == NodeLevel.Items)
                this.ProcessSubItems(currentPath, nodeDto);
            List<TreeNodeInfo> subContainers = LoadFolders(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = int.MaxValue - 1 });
            foreach (TreeNodeInfo info in subContainers)
            {
                if (info.NeedRestore(nodeDto.Level))
                {
                    if (!(nodeDto.Level == NodeLevel.Site && info.Name.Equals(".", StringComparison.OrdinalIgnoreCase)))
                    {
                        RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                        markMsg.IsSelected = false;
                        markMsg.ParentIsSelected = true;
                        OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    }
                    currentPath = this.currentSiteCollectionUrl + "\\" + info.Name;
                    this.ProcessSubData(currentPath, nodeDto);
                    if (info.Type.EqualsIgnoreCase("L"))
                        this.ProcessAttachements();
                }
            }
        }

        void ProcessSubData(String currentPath, GoogleDriveTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSubDataInfo, currentPath);
            this.ProcessSubItems(currentPath, nodeDto);
            List<TreeNodeInfo> subContainers = LoadFolders(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = int.MaxValue - 1 });
            foreach (TreeNodeInfo info in subContainers)
            {
                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                currentPath = this.currentSiteCollectionUrl + "\\" + info.Name;
                this.ProcessSubData(currentPath, nodeDto);
            }
        }

        void ProcessUnSelectedItems(List<GoogleDriveTreeNodeDto> items, string currentPath, GoogleDriveTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessUnSelectedItemsInfo, currentPath);
            bool send = true;
            var childNodeDto = new GoogleDriveTreeNodeDto();
            List<TreeNodeInfo> allItems = LoadItems(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 });
            foreach (TreeNodeInfo indexItem in allItems)
            {
                send = true;
                foreach (GoogleDriveTreeNodeDto child in items)
                {
                    if (child.Name.Equals(indexItem.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        send = false;
                        break;
                    }
                }
                if (send)
                {
                    var isSelectCurrentVersion = this.IsSelectCurrentVersion(indexItem, items, isInverted: true);
                    if (nodeDto.Children.Exists(node => node.Name.EqualsIgnoreCase(indexItem.Name)))
                        childNodeDto = nodeDto.Children.Find(node => node.Name.EqualsIgnoreCase(indexItem.Name));
                    else
                        childNodeDto = new GoogleDriveTreeNodeDto { /*Property = PropertyState.Checked, Security = SecurityState.Checked*/ };
                    TreeNodeInfo info = Load(new TreeIndexParameter { Path = currentPath + "\\" + indexItem.Name, EndTime = indexItem.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                    RestoreMarkMessage markMsg = new RestoreMarkMessage(childNodeDto);
                    if (info.Type.EqualsIgnoreCase("A"))
                        this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    else
                        OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                }
            }
            if (this.currentVersionFlagMap.Count > 0)
                this.currentVersionFlagMap = new Dictionary<String, Boolean>();
        }

        void ProcessSelectedItemWithVersions(GoogleDriveTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSelectedItemsInfo, nodeDto.ObjectId);
            if (this.restoreJob.RestoreVersionOption != RestoreDocumentVersionsOption.None)
            {
                int topCount = this.restoreJob.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions ? restoreJob.KeepVersionsNumber : -1;
                TreeNodeInfo info = Load(new TreeIndexParameter { Path = nodeDto.PathMD5, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                var indexInfo = info.Index as GoogleBasicIndex;
                List<GoogleBasicIndex> versionInfo = new List<GoogleBasicIndex>();
                if (topCount != 1)
                {
                    versionInfo = LoadDocumentVersions(topCount - 1, indexInfo.ItemId, restoreJob.BackupTime);
                }
                RestoreMarkMessage markMsg = new RestoreMarkMessage();
                markMsg.IsChecked = true;
                markMsg.IsSelected = true;
                markMsg.VersionFlag = 1;
                markMsg.Security = SecurityState.Checked;
                markMsg.Property = PropertyState.Unchecked;
                foreach (var version in versionInfo)
                {
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = version, MarkMessage = markMsg });
                }
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });

            }
            else
            {
                TreeNodeInfo info = Load(new TreeIndexParameter { Path = nodeDto.PathMD5, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                markMsg.Property = PropertyState.Unchecked;
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
            }
        }

        void ProcessSubItems(String currentPath, GoogleDriveTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSubItemsInfo, currentPath);
            if (isJustCalculateCount)
            {
                Int64 itemCount = GetItemsCount(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 });
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = itemCount });
            }
            else
            {
                var isSelectCurrentVersion = default(Boolean);
                List<TreeNodeInfo> items = LoadItems(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 });
                foreach (TreeNodeInfo info in items)
                {
                    isSelectCurrentVersion = this.IsSelectCurrentVersion(info, this.currentVersionFlagMap, items);
                    RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);//0 stand for agent overwrite
                    markMsg.IsSelected = false;
                    markMsg.ParentIsSelected = true;
                    if (info.Type.EqualsIgnoreCase("A"))
                        this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    else
                        OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                }
            }
            if (this.currentVersionFlagMap.Count > 0)
                this.currentVersionFlagMap = new Dictionary<String, Boolean>();
        }

        List<GoogleDriveTreeNodeDto> GetAllFiles(GoogleDriveTreeNodeDto itemsDto, Boolean isInverted)
        {
            var result = new List<GoogleDriveTreeNodeDto>();
            if (this.restoreJob.IsSearchTree)
                result = itemsDto.Children.FindAll(item => item.CheckNumber == 1);
            else
                result = itemsDto.Children;
            return result;
        }

        void ProcessAttachements()
        {
            if (!this.isJustCalculateCount && attachementInfos.Count > 0)
            {
                this.attachementInfos.ForEach(attachement => this.OnIndexItemProceed(attachement));
                this.attachementInfos.Clear();
            }
        }
    }
}