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




namespace AvePoint.Media.Service
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.StorageOptimization.Connector;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using Merged18NResources.MediaServiceApplicationModel;
    using Microsoft.Online.SharePoint.TenantAdministration;
    #endregion

    public abstract class RestoreServiceTreeHandlerBase : IRestoreServiceTreeHandler
    {
        const int specifiedVersionPageSize = 1000;
        readonly static Object syncIndexItemProceedObject = new Object();
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Dictionary<String, Boolean> currentVersionFlagMap = new Dictionary<String, Boolean>();
        String currentSiteCollectionUrl;
        Boolean isJustCalculateCount;
        protected Boolean isPreview;
        RestoreJobBase restoreJob;
        List<IndexItemProceedEventArgs> attachementInfos;

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

        public ISqlBuilder _SqlBuilder { get; set; }
        public ISqlBuilder SqlBuilder
        {
            get
            {
                if (_SqlBuilder == null)
                {
                    _SqlBuilder = new ArchiverSqlBuilder();
                    return _SqlBuilder;
                }
                else
                {
                    return _SqlBuilder;
                }
            }
            set { }
        }

        static readonly String selectSectionForCount = "select distinct COL_PATH_MD5 ";
        static readonly String selectSectionForRestore = "select MAX(COL_ARCHIVE_TIME),*";
        Dictionary<string, TreeNodeInfo> parentTreeNodeDic = [];
        private PolicyLevel _filterLevel = PolicyLevel.None;
        private ArchiverRestoreFilter _filter = null;

        public SPTreeNodeDto CutTree(SPTreeNodeDto rootTree)
        {
            SPTreeNodeDto treeNodeDto = null;
            if (rootTree.Level == NodeLevel.Items)
            {
                if (rootTree.CheckNumber == 1 || rootTree.SelectAll == SelectAllState.Checked)
                    treeNodeDto = rootTree;
                else
                {
                    if (rootTree.ChildrenCount > 0)
                    {
                        foreach (SPTreeNodeDto item in rootTree.Children)
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
                List<SPTreeNodeDto> children = new List<SPTreeNodeDto>();
                foreach (SPTreeNodeDto child in rootTree.Children)
                {
                    SPTreeNodeDto selectedNodeDto = CutTree(child);
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
            this.restoreJob = treeParam.RestoreJob;
            this.isJustCalculateCount = treeParam.IsJustCalculateCount;
            this.isPreview = treeParam.IsPreview;
            this.currentSiteCollectionUrl = treeParam.CurrentTree.Name;
            this.attachementInfos = new List<IndexItemProceedEventArgs>();
            this.parentTreeNodeDic = [];
            logger.Info($"node backuptime:{treeParam.CurrentTree.NodeExtension?.BackupTime}, restoreJob backuptime: {restoreJob.BackupTime}");
            this.ProcessNodeDtoInternal(treeParam.CurrentTree.Name, treeParam.CurrentTree);
        }

        protected virtual void OnIndexItemProceed(IndexItemProceedEventArgs args)
        {
            var temp = indexItemProceed;
            if (temp != null) temp(this, args);
        }


        protected abstract List<TreeNodeInfo> LoadFolders(TreeIndexParameter parameter, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null);

        protected abstract List<TreeNodeInfo> LoadItems(TreeIndexParameter parameter, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null, bool isJustCalculateCount = false);

        protected abstract List<TreeNodeInfo> LoadCurrentItems(TreeIndexParameter parameter);

        protected abstract long GetItemsCount(TreeIndexParameter parameter, StringBuilder? sql = null, BackupDataSearchContract? searchContract = null);

        protected abstract TreeNodeInfo Load(TreeIndexParameter parameter);
        protected abstract List<ArchiverBasicIndex> LoadDocumentVersions(int topCount,string ItemId,long endTime, bool isRestoreAllVersions);
        protected abstract Dictionary<string, List<ArchiverBasicIndex>> LoadDocumentVersionsByItemIds(int topCount, List<string> itemIds, long endTime, bool isRestoreAllVersions);

        protected abstract List<TreeNodeInfo> LoadItemAndVersions(TreeIndexParameter parameter);

        protected abstract Boolean IsSelectCurrentVersion(TreeNodeInfo info, Dictionary<String, Boolean> currentVersionFlagMap, List<TreeNodeInfo> items);

        protected abstract Boolean IsSelectCurrentVersion(TreeNodeInfo info, List<SPTreeNodeDto> items, Boolean isInverted);

        private void ProcessNodeDtoInternal(string currentPath, SPTreeNodeDto nodeDto)
        {

            if (restoreJob.IsAdvancedRestore && nodeDto.IsVirtualNode())
            {
                nodeDto.SelectAll = SelectAllState.Undefined;
            }
            MediaRestoreNode node = new MediaRestoreNode(nodeDto);
            if (!node.IsVirtualNode)
            {
                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessNodeDtoInternalProcessCurrentNode, currentPath);
                TreeNodeInfo info = Load(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                //OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = new RestoreMarkMessage() 
                { 
                    Security = SecurityState.Checked, 
                    Property = PropertyState.Checked, 
                    IsChecked = true, 
                    VersionFlag = 1, 
                    IsSelected = nodeDto.CheckNumber == 1,
                    ParentIsSelected = NodeUtil.CheckParentWasChecked(nodeDto)
                }
                });
                //var markMessage = new RestoreMarkMessage() { Security = SecurityState.Unchecked, Property = PropertyState.Unchecked, IsChecked = false, VersionFlag = 1 };
                //if (!node.IsExpanded)
                //{
                //    markMessage.Security = SecurityState.Checked;
                //    markMessage.Property = PropertyState.Checked;
                //    markMessage.IsChecked = true;

                //}
                //OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMessage });
            }

            if (this.restoreJob.IsSearchAllRestore && this.restoreJob.SearchContract != null)
            {
                _filter = this.restoreJob.SearchContract.FilterPolicy;
                _filterLevel = _filter.Level;
                this.logger.Info($"Start process for search all restore. isJustCalculateCount: {isJustCalculateCount}, filterObjectLevel: {_filterLevel}");
                // need ProcessSubContainers and use search condition when load folders or items
                //node.IsExpanded = false;
                //node.IsChecked = true;
                ProcessRestoreAllBySearchContract(currentPath, nodeDto);
                return;
            }

            if (node.IsExpanded)//真实的tree被展开
            {
                if (node.Level == NodeLevel.Items)
                {
                    List<SPTreeNodeDto> items = GetAllFiles(nodeDto, node.IsInverted);
                    if (node.IsInverted && !this.restoreJob.IsSearchTree)//send unselected items
                        ProcessUnSelectedItems(items, currentPath, nodeDto);
                    else//send selected items
                        ProcessSelectedItems(items, currentPath, nodeDto);
                }
                else
                {
                    if (nodeDto.Children.Count != 0 && nodeDto.Children.Count == 2 && nodeDto.Children[0].Level.Equals(NodeLevel.Folders))
                    {
                        var tempNodeDto = new SPTreeNodeDto();
                        tempNodeDto.Children.Add(nodeDto.Children[1]);
                        tempNodeDto.Children.Add(nodeDto.Children[0]);
                        nodeDto = tempNodeDto;
                    }
                    foreach (SPTreeNodeDto subNode in nodeDto.Children)
                    {
                        if (subNode.IsVirtualNode())
                            ProcessNodeDtoInternal(currentPath, subNode);
                        else if (node.Level == NodeLevel.Sites && !nodeDto.Parent.Name.Equals(".", StringComparison.OrdinalIgnoreCase))
                            ProcessNodeDtoInternal(currentPath + "/" + subNode.Name, subNode);
                        else
                        {
                            if (node.Level == NodeLevel.Sites && nodeDto.Parent.Name.Equals(".", StringComparison.OrdinalIgnoreCase))
                                currentPath = this.currentSiteCollectionUrl;
                            ProcessNodeDtoInternal(currentPath + "\\" + subNode.Name, subNode);
                        }
                    }
                }
            }
            else//真实的tree没有展开
            {
                if (node.IsChecked)//check number一定是1
                {
                    if (nodeDto.Level == NodeLevel.Sites)
                        currentPath = this.currentSiteCollectionUrl;
                    this.ProcessSubContainers(currentPath, nodeDto);
                }
                else
                    throw new NodeCheckStateException(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessNodeDtoInternalCheckStateError);
            }
            if (nodeDto.Level == NodeLevel.List)
                this.ProcessAttachements();
        }

        void ProcessSubContainers(String currentPath, SPTreeNodeDto nodeDto)
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

        void ProcessRestoreAllBySearchContract(String currentPath, SPTreeNodeDto nodeDto)
        {
            this.logger.Info($"[SAR]Processing Restore All By Search Contract for {currentPath}");
            using var _ = new PerformanceScope("RestoreServiceTreeHandlerBase:ProcessRestoreAllBySearchContract", $"Load datas under: {currentPath}", true);
            var treeIndexParameter = new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = int.MaxValue - 1 };
            List<TreeNodeInfo> subContainers = [];
            var foundNodesBySearch = false;
            subContainers = LoadFolders(treeIndexParameter);
            foreach (TreeNodeInfo info in subContainers)
            {
                // need check by the search condition after load all because they are the nestable node types
                var nodeLevel = info.ConverTypeToLevel();
                var needReset = false;
                if (nodeLevel == NodeLevel.Site || nodeLevel == NodeLevel.Folder)
                {
                    if (foundNodesBySearch)
                    {
                        logger.Info($"[SAR]Parent node {currentPath} found by search, process current node {info.Name}");
                    }
                    else if (!IsNodeMatchSearchCondition(info, nodeLevel))
                    {
                        logger.Info($"[SAR]Node {info.ConverTypeToLevel()}: {info.Name} under {currentPath} does not match search condition. Continue to search children node");
                        //continue;
                    }
                    else
                    {
                        foundNodesBySearch = ProceedParentNodes(currentPath, nodeDto, 1);
                        needReset = true;
                    }
                }

                //if (info.NeedRestore(nodeDto.Level))
                currentPath = this.currentSiteCollectionUrl + "\\" + info.Name;
                if (foundNodesBySearch)
                {
                    RestoreMarkMessage markMsg = new(nodeDto)
                    {
                        Security = SecurityState.Checked,
                        Property = PropertyState.Checked,
                        IsChecked = true,
                        VersionFlag = 1,
                        IsSelected = true,
                        ParentIsSelected = false
                    };
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                }
                else
                {
                    parentTreeNodeDic[currentPath] = info;
                }

                this.ProcessSubData(currentPath, nodeDto, info.ConverTypeToLevel(), foundNodesBySearch);

                parentTreeNodeDic.Remove(currentPath);

                if (needReset)
                {
                    foundNodesBySearch = false;
                }

                if (info.Type.EqualsIgnoreCase("L"))
                    this.ProcessAttachements();
            }
        }

        void ProcessSubData(String currentPath, SPTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSubDataInfo, currentPath);
            this.ProcessSubItems(currentPath, nodeDto);
            List<TreeNodeInfo> subContainers = LoadFolders(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = int.MaxValue - 1 });
            ProcessContainers(currentPath, nodeDto, ReSortApps(subContainers));
        }

        //restore app after list, before sub site
        LinkedList<TreeNodeInfo> ReSortApps(List<TreeNodeInfo> subContainers)
        {
            LinkedList<TreeNodeInfo> res = new LinkedList<TreeNodeInfo>();
            List<TreeNodeInfo> apps = subContainers.Where(container => "Y".Equals(container.Type)).ToList();
            IEnumerable<TreeNodeInfo> containers = subContainers.Where(container => !"Y".Equals(container.Type));

            foreach(TreeNodeInfo info in containers)
            {
                if (info.Type.Equals("W"))
                {
                    foreach(TreeNodeInfo app in apps)
                    {
                        res.AddLast(app);
                    }
                    apps.Clear();
                }
                res.AddLast(info);
            }
            foreach (TreeNodeInfo app in apps)
            {
                res.AddLast(app);
            }
            return res;
        }

        void ProcessContainers(String currentPath, SPTreeNodeDto nodeDto, IEnumerable<TreeNodeInfo> subContainers)
        {
            foreach (TreeNodeInfo info in subContainers)
            {
                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                currentPath = this.currentSiteCollectionUrl + "\\" + info.Name;
                this.ProcessSubData(currentPath, nodeDto);
                if (info.Type.EqualsIgnoreCase("L"))
                    this.ProcessAttachements();
            }
        }

        void ProcessSubData(String currentPath, SPTreeNodeDto nodeDto, NodeLevel level, bool foundNodesBySearch = false)
        {
            this.logger.Info($"[SAR]Processing sub data in {currentPath}, level: {level}, foundNodesBySearch: {foundNodesBySearch}");
            using var _ = new PerformanceScope("RestoreServiceTreeHandlerBase:ProcessSubData", $"Load sub datas under: {currentPath}", true);
            if (level != NodeLevel.Site && level != NodeLevel.SiteCollection)
            {
                this.ProcessSearchAllSubItems(currentPath, nodeDto, level, foundNodesBySearch);
            }
            var treeIndexParameter = new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = int.MaxValue - 1 };
            List<TreeNodeInfo> subContainers = [];
            var isCurrentNodeSelected = false;

            if (!foundNodesBySearch && NeedProcessSearch(level))
            {
                
                var buildInfo = new SqlBuildInfo(selectSectionForRestore, _filter);
                var sql = SqlBuilder.Build(buildInfo);

                var isFilterNestableNode = _filterLevel == PolicyLevel.Site || _filterLevel == PolicyLevel.Folder;
                subContainers = LoadFolders(treeIndexParameter, sql, isFilterNestableNode ? null : restoreJob.SearchContract);
                isCurrentNodeSelected = foundNodesBySearch = !isFilterNestableNode && ProceedParentNodes(currentPath, nodeDto, subContainers.Count);
            }
            else
            {
                subContainers = LoadFolders(treeIndexParameter);
            }
            foreach (TreeNodeInfo info in subContainers)
            {
                var nodeLevel = info.ConverTypeToLevel();
                var needReset = false;
                if (nodeLevel == NodeLevel.Site || nodeLevel == NodeLevel.List || nodeLevel == NodeLevel.Folder)
                {
                    if (foundNodesBySearch)
                    {
                        logger.Info($"[SAR]Parent node {currentPath} found by search, process current node {info.Name}");
                    }
                    else if (!IsNodeMatchSearchCondition(info, nodeLevel))
                    {
                        logger.Info($"[SAR]Node {info.ConverTypeToLevel()}: {info.Name} under {currentPath} does not match search condition. Continue to search children node");
                        //continue;
                        if (nodeLevel == NodeLevel.List && (_filterLevel == PolicyLevel.Site || _filterLevel == PolicyLevel.List))
                        {
                            // skip continue to process children nodes for List/Library when filter level is not child level
                            continue;
                        }
                    }
                    else
                    {
                        isCurrentNodeSelected = foundNodesBySearch = ProceedParentNodes(currentPath, nodeDto, 1);
                        needReset = true;
                    }
                }

                currentPath = this.currentSiteCollectionUrl + "\\" + info.Name;

                if (foundNodesBySearch)
                {
                    RestoreMarkMessage markMsg = new(nodeDto)
                    {
                        Security = SecurityState.Checked,
                        Property = PropertyState.Checked,
                        IsChecked = true,
                        VersionFlag = 1,
                        IsSelected = isCurrentNodeSelected,
                        ParentIsSelected = !isCurrentNodeSelected,
                    };
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                }
                else
                {
                    parentTreeNodeDic[currentPath] = info;
                }

                this.ProcessSubData(currentPath, nodeDto, info.ConverTypeToLevel(), foundNodesBySearch);

                parentTreeNodeDic.Remove(currentPath);

                if (needReset)
                {
                    isCurrentNodeSelected = foundNodesBySearch = false;
                }

                if (info.Type.EqualsIgnoreCase("L"))
                    this.ProcessAttachements();
            }
        }

        private bool IsNodeMatchSearchCondition(TreeNodeInfo info, NodeLevel nodeLevel)
        {
            if (!(nodeLevel == NodeLevel.Site && _filterLevel == PolicyLevel.Site)
                && !(nodeLevel == NodeLevel.List && _filterLevel == PolicyLevel.List)
                && !(nodeLevel == NodeLevel.Folder && _filterLevel == PolicyLevel.Folder)
                )
            {
                return false;
            }

            var result = true;

            #region FilterName
            var filterName = _filter.FilterName;
            var itemName = string.Empty;

            if (info.Index is not ArchiverBasicIndex basicIndex)
            {
                return false;
            }

            if (nodeLevel == NodeLevel.Site)
            {
                int colonIndex = basicIndex.Attributes.IndexOf(':');
                int ctrlCharIndex = basicIndex.Attributes.IndexOf('\u0013');
                if (colonIndex >= 0 && ctrlCharIndex > colonIndex)
                {
                    itemName = basicIndex.Attributes.Substring(colonIndex + 1, ctrlCharIndex - colonIndex - 1);
                }
            }
            else if (nodeLevel == NodeLevel.Folder || nodeLevel == NodeLevel.List)
            {
                int lastBackslash = basicIndex.Name.LastIndexOf('\\');
                itemName = lastBackslash >= 0 ? basicIndex.Name.Substring(lastBackslash + 1) : basicIndex.Name;
            }

            if (!string.IsNullOrEmpty(filterName))
            {
                if (IsUseFullNameMatch(filterName))
                {
                    result = itemName.Equals(filterName.Trim('\"'), StringComparison.OrdinalIgnoreCase);
                }
                else if (filterName.Contains('*') || filterName.Contains('?'))
                {
                    var regexPattern = Regex.Escape(filterName);
                    regexPattern = regexPattern.Replace("\\*", ".*").Replace("\\?", ".");
                    result = Regex.IsMatch(itemName, regexPattern, RegexOptions.IgnoreCase);
                }
                else
                {
                    result = itemName.Contains(filterName, StringComparison.OrdinalIgnoreCase);
                }
            }
            #endregion

            //if (info.Index is ArchiverBasicIndex basicIndex && Convert.ToBoolean(basicIndex.IsSystemFile))
            //{
            //    return result;
            //}

            return result;
        }

        private bool IsUseFullNameMatch(string filterNameValue)
        {
            if (!string.IsNullOrEmpty(filterNameValue))
            {
                return filterNameValue.StartsWith('\"') && filterNameValue.EndsWith('\"');
            }
            return false;
        }

        void ProcessUnSelectedItems(List<SPTreeNodeDto> items, string currentPath, SPTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessUnSelectedItemsInfo, currentPath);
            bool send = true;
            var childNodeDto = new SPTreeNodeDto();
            List<TreeNodeInfo> allItems = LoadItems(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 });
            foreach (TreeNodeInfo indexItem in allItems)
            {
                send = true;
                foreach (SPTreeNodeDto child in items)
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
                        childNodeDto = new SPTreeNodeDto { Property = PropertyState.Checked, Security = SecurityState.Checked };
                    TreeNodeInfo info = Load(new TreeIndexParameter { Path = currentPath + "\\" + indexItem.Name, EndTime = indexItem.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                    RestoreMarkMessage markMsg = new RestoreMarkMessage(childNodeDto, isSelectCurrentVersion ? 1 : 0);
                    if (info.Type.EqualsIgnoreCase("A"))
                        this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    else
                        OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                }
            }
            if (this.currentVersionFlagMap.Count > 0)
                this.currentVersionFlagMap = new Dictionary<String, Boolean>();
        }

        void ProcessSelectedItems(List<SPTreeNodeDto> items, string currentPath, SPTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSelectedItemsInfo, currentPath);
            if (this.restoreJob.RestoreVersionOption != RestoreDocumentVersionsOption.None)
            {
                int topCount = GetRestoreTopCount();
                foreach (SPTreeNodeDto childNode in items)
                {
                    TreeNodeInfo info = Load(new TreeIndexParameter { Path = currentPath + "\\" + childNode.Name, EndTime = childNode.NodeExtension.BackupTime == 0? restoreJob.BackupTime: childNode.NodeExtension.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                    var indexInfo = info.Index as ArchiverBasicIndex;
                    List<ArchiverBasicIndex> versionInfo = new List<ArchiverBasicIndex>();
                    if (topCount != 1)
                    {
                        versionInfo = LoadDocumentVersions(topCount-1, indexInfo.NodeGuid, childNode.NodeExtension.BackupTime == 0 ? restoreJob.BackupTime : childNode.NodeExtension.BackupTime, this.restoreJob.RestoreVersionOption == RestoreDocumentVersionsOption.AllVersions);
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
                if (this.currentVersionFlagMap.Count > 0)
                    this.currentVersionFlagMap = new Dictionary<String, Boolean>();
            }
            else
            {
                foreach (SPTreeNodeDto childNode in items)
                {
                    TreeNodeInfo info = Load(new TreeIndexParameter { Path = currentPath + "\\" + childNode.Name, EndTime = childNode.NodeExtension.BackupTime == 0 ? restoreJob.BackupTime : childNode.NodeExtension.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob });
                    var isSelectCurrentVersion = IsSelectCurrentVersion(info, items, isInverted: false);
                    RestoreMarkMessage markMsg = new RestoreMarkMessage(childNode, isSelectCurrentVersion ? 1 : 0);
                    markMsg.Property = PropertyState.Unchecked;
                    markMsg.IsSelected = true;
                    if (info.Type.EqualsIgnoreCase("A"))
                        this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    else
                        OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                }
                if (this.currentVersionFlagMap.Count > 0)
                    this.currentVersionFlagMap = new Dictionary<String, Boolean>();
            }
        }

        void ProcessSubItems(String currentPath, SPTreeNodeDto nodeDto, bool isSearchRestore = false)
        {
            this.logger.Info(MediaServiceApplicationModelResource.RestoreServiceTreeHandlerBaseProcessSubItemsInfo, currentPath);
            var endTime = isSearchRestore ? restoreJob.BackupTime
                : nodeDto.NodeExtension.BackupTime == 0 ? restoreJob.BackupTime : nodeDto.NodeExtension.BackupTime;
            if (isJustCalculateCount)
            {
                Int64 itemCount = ShouldProcessSpecifiedVersions()
                    ? CalculateSpecifiedVersionSubItemCount(currentPath, endTime)
                    : GetItemsCount(new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 });
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = itemCount });
            }
            else
            {
                var isSelectCurrentVersion = default(Boolean);
                if (ShouldProcessSpecifiedVersions())
                {
                    ProcessSpecifiedVersionSubItems(currentPath, nodeDto, endTime);
                }
                else
                {
                    List<TreeNodeInfo> items = LoadItems(new TreeIndexParameter { Path = currentPath, EndTime = endTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 });
                    foreach (TreeNodeInfo info in items)
                    {
                        isSelectCurrentVersion = this.IsSelectCurrentVersion(info, this.currentVersionFlagMap, items);
                        RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto, isSelectCurrentVersion ? 1 : 0);//0 stand for agent overwrite
                        markMsg.IsSelected = false;
                        markMsg.ParentIsSelected = true;
                        //if (info.Type.EqualsIgnoreCase("A"))
                        //    this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                        //else
                            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    }
                }
            }
            if (this.currentVersionFlagMap.Count > 0)
                this.currentVersionFlagMap = new Dictionary<String, Boolean>();
        }

        private int GetRestoreTopCount()
        {
            return this.restoreJob.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions
                ? this.restoreJob.KeepVersionsNumber
                : -1;
        }

        private bool ShouldProcessSpecifiedVersions()
        {
            return GetRestoreTopCount() > 0;
        }

        private void ProcessSpecifiedVersionSubItems(string currentPath, SPTreeNodeDto nodeDto, long endTime)
        {
            var offset = 0;
            while (true)
            {
                List<TreeNodeInfo> items = LoadCurrentItems(new TreeIndexParameter
                {
                    Path = currentPath,
                    EndTime = endTime,
                    BackupJobId = restoreJob.BackupJobId,
                    OnlyOneJob = restoreJob.OnlyOneJob,
                    OffSet = offset,
                    Length = specifiedVersionPageSize,
                });
                if (items.Count == 0)
                {
                    return;
                }

                ProcessSpecifiedVersionItemPage(items, nodeDto, endTime);
                if (items.Count < specifiedVersionPageSize)
                {
                    return;
                }

                offset += items.Count;
            }
        }

        private long CalculateSpecifiedVersionSubItemCount(string currentPath, long endTime)
        {
            long totalCount = 0;

            var offset = 0;
            while (true)
            {
                List<TreeNodeInfo> items = LoadCurrentItems(new TreeIndexParameter
                {
                    Path = currentPath,
                    EndTime = endTime,
                    BackupJobId = restoreJob.BackupJobId,
                    OnlyOneJob = restoreJob.OnlyOneJob,
                    OffSet = offset,
                    Length = specifiedVersionPageSize,
                });
                if (items.Count == 0)
                {
                    return totalCount;
                }

                totalCount += CalculateSpecifiedVersionItemPageCount(items, endTime);
                if (items.Count < specifiedVersionPageSize)
                {
                    return totalCount;
                }

                offset += items.Count;
            }
        }

        private long CalculateSpecifiedVersionItemPageCount(List<TreeNodeInfo> items, long endTime)
        {
            int topCount = GetRestoreTopCount();
            var versionLookup = LoadSpecifiedVersionPageVersions(items, topCount > 1 ? topCount - 1 : 0, endTime);
            long totalCount = 0;
            foreach (TreeNodeInfo info in items)
            {
                if (info.Type.EqualsIgnoreCase("A") || info.Index is not ArchiverBasicIndex indexInfo)
                {
                    totalCount++;
                    continue;
                }

                totalCount += 1 + GetSpecifiedVersionCount(versionLookup, indexInfo.NodeGuid);
            }

            return totalCount;
        }

        private void ProcessSpecifiedVersionItemPage(List<TreeNodeInfo> items, SPTreeNodeDto nodeDto, long endTime)
        {
            int topCount = GetRestoreTopCount();
            var versionLookup = LoadSpecifiedVersionPageVersions(items, topCount > 1 ? topCount - 1 : 0, endTime);
            foreach (TreeNodeInfo info in items)
            {
                if (info.Type.EqualsIgnoreCase("A") || info.Index is not ArchiverBasicIndex indexInfo)
                {
                    SendSpecifiedVersionIndex(info.Index, info.Type, nodeDto);
                    continue;
                }

                foreach (var version in GetSpecifiedVersionEntries(versionLookup, indexInfo.NodeGuid))
                {
                    SendSpecifiedVersionIndex(version, version.Type, nodeDto);
                }

                SendSpecifiedVersionIndex(info.Index, info.Type, nodeDto);
            }
        }

        private Dictionary<string, List<ArchiverBasicIndex>> LoadSpecifiedVersionPageVersions(List<TreeNodeInfo> items, int versionTopCount, long endTime)
        {
            if (versionTopCount < 0)
            {
                return new Dictionary<string, List<ArchiverBasicIndex>>(StringComparer.OrdinalIgnoreCase);
            }

            HashSet<string> itemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (TreeNodeInfo item in items)
            {
                if (item.Index is not ArchiverBasicIndex currentIndex || string.IsNullOrEmpty(currentIndex.NodeGuid))
                {
                    continue;
                }

                itemIds.Add(currentIndex.NodeGuid);
            }

            return LoadDocumentVersionsByItemIds(versionTopCount, itemIds.ToList(), endTime, false);
        }

        private static int GetSpecifiedVersionCount(Dictionary<string, List<ArchiverBasicIndex>> versionLookup, string itemId)
        {
            return GetSpecifiedVersionEntries(versionLookup, itemId).Count;
        }

        private static List<ArchiverBasicIndex> GetSpecifiedVersionEntries(Dictionary<string, List<ArchiverBasicIndex>> versionLookup, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return new List<ArchiverBasicIndex>();
            }

            if (versionLookup.TryGetValue(itemId, out var versions))
            {
                return versions;
            }

            return new List<ArchiverBasicIndex>();
        }

        private void SendSpecifiedVersionIndex(IndexBase indexItem, string itemType, SPTreeNodeDto nodeDto)
        {
            RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto)
            {
                IsChecked = true,
                IsSelected = false,
                ParentIsSelected = true,
                VersionFlag = 1,
                Security = SecurityState.Checked,
                Property = PropertyState.Unchecked,
            };

            if (itemType.EqualsIgnoreCase("A"))
            {
                this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = indexItem, MarkMessage = markMsg });
                return;
            }

            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = indexItem, MarkMessage = markMsg });
        }

        void ProcessSearchAllSubItems(String currentPath, SPTreeNodeDto nodeDto, NodeLevel level, bool foundNodesBySearch = false)
        {
            if (foundNodesBySearch/* || !NeedProcessSearch(level, true)*/)
            {
                this.logger.Info($"[SAR]Process without searching. foundNodesBySearch {foundNodesBySearch}, currentPath: {currentPath}, NodeLevel: {level}, SearchLevel: {_filterLevel}");
                ProcessSubItems(currentPath, nodeDto, true);
                return;
            }

            if (!NeedProcessSearch(level, true) 
                //restoreJob.SearchContract.FilterPolicy.Level != PolicyLevel.Document
                //&& restoreJob.SearchContract.FilterPolicy.Level != PolicyLevel.DocumentVersion
                //&& restoreJob.SearchContract.FilterPolicy.Level != PolicyLevel.Item
                )
            {
                this.logger.Info($"[SAR]Skip processing search all sub item for {currentPath}. NodeLevel: {level}, SearchLevel: {_filterLevel}");
                return;
            }

            using var _ = new PerformanceScope("RestoreServiceTreeHandlerBase:ProcessSearchAllSubItems", $"Load items under: {currentPath}", true);
            //var isSelectCurrentVersion = false;
            this.logger.Info($"[SAR]Processing all sub items in {currentPath}, RestoreVersionOption: {this.restoreJob.RestoreVersionOption}, topCount: {restoreJob.KeepVersionsNumber}");
            var treeIndexParameter = new TreeIndexParameter { Path = currentPath, EndTime = restoreJob.BackupTime, BackupJobId = restoreJob.BackupJobId, OnlyOneJob = restoreJob.OnlyOneJob, OffSet = 0, Length = -1 };
            var buildInfo = new SqlBuildInfo(selectSectionForRestore, restoreJob.SearchContract.FilterPolicy);
            var sql = SqlBuilder.Build(buildInfo);
            List<TreeNodeInfo> items = LoadItems(treeIndexParameter, sql, restoreJob.SearchContract, isJustCalculateCount);
            ProceedParentNodes(currentPath, nodeDto, items.Count);
            Dictionary<string, (string ItemName, long BackupTime, List<TreeNodeInfo> VersionInfos)> tempNodeVersions = [];
            var tempPreviousNodeGuid = string.Empty;
            foreach (TreeNodeInfo info in items)
            {
                if (info.Index is not ArchiverBasicIndex index)
                {
                    this.logger.Warn($"[SAR] Item {info.ItemName} has not support index type: {info.Index.GetType().Name}");
                    continue;
                }

                // attachment does not have version. Skip ProceedVersionNode
                if (info.Type.EqualsIgnoreCase("A"))
                {
                    RestoreMarkMessage markMsg = new()
                    {
                        IsChecked = true,
                        VersionFlag = 1, //0 stand for agent overwrite
                        Security = SecurityState.Checked,
                        Property = PropertyState.Unchecked,
                        IsSelected = true,
                        ParentIsSelected = false
                    };
                    this.attachementInfos.Add(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
                    continue;
                }

                if (!string.IsNullOrEmpty(tempPreviousNodeGuid) && !string.Equals(tempPreviousNodeGuid, index.NodeGuid, StringComparison.OrdinalIgnoreCase))
                {
                    if (!tempNodeVersions.TryGetValue(tempPreviousNodeGuid, out var exist))
                    {
                        this.logger.Warn($"[SAR] Item {info.ItemName} has NodeGuid: {tempPreviousNodeGuid} not cached");
                        continue;
                    }

                    if (string.Equals(exist.ItemName, info.ItemName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.logger.Info($"[SAR] Item {tempPreviousNodeGuid} has different backup time, will restore the latest one. ExistBackupTime: {exist.BackupTime}, NewBackupTime: {info.BackupTime}");
                        if (exist.BackupTime < info.BackupTime)
                        {
                            tempNodeVersions.Remove(tempPreviousNodeGuid);
                        }
                        else
                        {
                            //tempNodeVersions.Remove(index.NodeGuid);
                            continue;
                        }
                    }
                    else
                    {
                        ProceedVersionNodes(tempPreviousNodeGuid, exist.VersionInfos, restoreJob.KeepVersionsNumber);
                        tempNodeVersions.Remove(tempPreviousNodeGuid);
                    }
                }

                if (tempNodeVersions.TryGetValue(index.NodeGuid, out var value))
                {
                    var versionList = value.VersionInfos;
                    versionList.Add(info);

                    if (value.BackupTime != info.BackupTime)
                    {
                        this.logger.Warn($"File {index.ItemName} has same Id {index.NodeGuid} but different backupTime. Path {currentPath}");
                    }
                    tempNodeVersions[index.NodeGuid] = (info.ItemName, info.BackupTime, versionList);
                }
                else
                {
                    tempNodeVersions[index.NodeGuid] = (info.ItemName, info.BackupTime, [info]);
                }

                tempPreviousNodeGuid = index.NodeGuid;
            }

            this.logger.Info($"[SAR] Process last item in {currentPath}, tempNodeVersions count: {tempNodeVersions.Count}");
            foreach (var item in tempNodeVersions)
            {
                if (!item.Value.VersionInfos.IsNullOrEmpty())
                {
                    ProceedVersionNodes(item.Key ,item.Value.VersionInfos, restoreJob.KeepVersionsNumber);
                }
            }
            
            if (this.currentVersionFlagMap.Count > 0)
                this.currentVersionFlagMap = new Dictionary<String, Boolean>();
        }

        private void ProceedVersionNodes(string tempPreviousNodeGuid, List<TreeNodeInfo> nodeVersions, int keepVersionNumber = 0)
        {
            if (nodeVersions.IsNullOrEmpty())
                return;

            using var _ = new PerformanceScope("RestoreServiceTreeHandlerBase:ProceedVersionNodes", $"ProceedVersionNodes for item: {tempPreviousNodeGuid}", true);
            var isContainCurrentVersion = nodeVersions.LastOrDefault(n => string.Equals(n.Name, n.ItemName, StringComparison.OrdinalIgnoreCase)) != null;
            if (this.restoreJob.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && keepVersionNumber != 1)
            {
                int end = nodeVersions.Count;
                int start = Math.Max(0, end - keepVersionNumber);
                if (isJustCalculateCount)
                {
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = end - start });
                    return;
                }
                for (int i = start; i < end; i++)
                {
                    var info = nodeVersions[i];
                    ProceedSingleVersionNode(isContainCurrentVersion, info);
                }
            }
            else
            {
                if (isJustCalculateCount)
                {
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = nodeVersions.Count });
                    return;
                }
                //var isSelectCurrentVersion = false;
                foreach (var info in nodeVersions)
                {
                    //isSelectCurrentVersion = this.IsSelectCurrentVersion(info, this.currentVersionFlagMap, nodeVersions);
                    ProceedSingleVersionNode(isContainCurrentVersion, info);
                }
            }
        }

        private void ProceedSingleVersionNode(bool isContainCurrentVersion, TreeNodeInfo info)
        {
            RestoreMarkMessage markMsg = new()
            {
                IsChecked = true,
                VersionFlag = isContainCurrentVersion ? 1 : 0, //0 stand for agent overwrite
                Security = SecurityState.Checked,
                Property = PropertyState.Unchecked,
                IsSelected = true,
                ParentIsSelected = false
            };

            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = info.Index, MarkMessage = markMsg });
        }

        private bool ProceedParentNodes(string currentPath, SPTreeNodeDto nodeDto, long itemCount)
        {
            var foundNodesBySearch = false;
            if (itemCount > 0)
            {
                this.logger.Info($"Start proceeding parent nodes for has children node. Path {currentPath}, children count {itemCount}. ");
                foreach (var (parentPath, parentNodeInfo) in parentTreeNodeDic)
                {
                    RestoreMarkMessage markMsg = new(nodeDto)
                    {
                        Security = SecurityState.Checked,
                        Property = PropertyState.Checked,
                        IsChecked = true,
                        VersionFlag = 1,
                        IsSelected = false,
                        ParentIsSelected = false
                    };
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = parentNodeInfo.Index, MarkMessage = markMsg });
                    this.logger.Info($"Finish proceeding for parent node. ParentPath {parentPath}, curentPath: {currentPath}");
                }
                parentTreeNodeDic.Clear();
                foundNodesBySearch = true;
            }
            return foundNodesBySearch;
        }

        List<SPTreeNodeDto> GetAllFiles(SPTreeNodeDto itemsDto, Boolean isInverted)
        {
            var result = new List<SPTreeNodeDto>();
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

        private bool NeedProcessSearch(NodeLevel level, bool isLoadItems = false)
        {
            return _filterLevel switch
            {
                // 1 job 1 site collection, no need to process search all for site collection
                PolicyLevel.SiteCollection => level == NodeLevel.SiteCollection,

                PolicyLevel.Site => level == NodeLevel.SiteCollection || level == NodeLevel.Site,

                PolicyLevel.Folder => !isLoadItems && (level == NodeLevel.List || level == NodeLevel.Library || level == NodeLevel.Folder),

                PolicyLevel.Item
                or PolicyLevel.Document 
                or PolicyLevel.DocumentVersion 
                    => isLoadItems && (level == NodeLevel.Folder || level == NodeLevel.List || level == NodeLevel.Library),

                _ => false
            };
        }
    }
}