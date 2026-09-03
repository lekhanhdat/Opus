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

namespace AvePoint.Media.Service.ExchangeBackup
{
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    #region using directives

    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.Media.Common;
    using AvePoint.Media.Service.ArchiverBackup;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using AvePoint.RA.CommonUtil;
    using AvePoint.RA.DB.Dao;
    using DocumentFormat.OpenXml.Spreadsheet;
    using ExchangeUtility.Graph;
    using Merged18NResources.MediaServiceExchangeBackUp;
    using Microsoft.SharePoint.Client;
    using Office365GroupRestore;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion using directives

    public class ExchangeRestoreTreeHandler
        : IExchangeRestoreTreeHandler
    {
        private static readonly Object syncIndexItemProceedObject = new Object();
        private RALogger logger = RALogger.GetInstance(typeof(ExchangeRestoreTreeHandler));
        private Boolean isJustCalculateCount;
        private RestoreJobBase restoreJob;
        private EventHandler<IndexItemProceedEventArgs> indexItemProceed;

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

        public IExchangeRestoreIndexService RestoreIndexService { get; set; }
        
        public ExchangeOnlineTreeNodeDto CutTree(ExchangeOnlineTreeNodeDto rootTree)
        {
            ExchangeOnlineTreeNodeDto treeNodeDto = null;
            if (rootTree.Level == NodeLevel.ExchangeOnlineItems)
            {
                if (rootTree.CheckNumber == 1 || rootTree.SelectAll == SelectAllState.Checked)
                    treeNodeDto = rootTree;
                else
                {
                    if (rootTree.ChildrenCount > 0)
                    {
                        foreach (ExchangeOnlineTreeNodeDto item in rootTree.Children)
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
                List<ExchangeOnlineTreeNodeDto> children = new List<ExchangeOnlineTreeNodeDto>();
                foreach (ExchangeOnlineTreeNodeDto child in rootTree.Children)
                {
                    ExchangeOnlineTreeNodeDto selectNodeDto = CutTree(child);
                    if (selectNodeDto != null)
                        children.Add(selectNodeDto);
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
            this.ProcessNodeDtoInternal(treeParam.ExchangeTree.Name, treeParam.ExchangeTree);
        }

        protected virtual void OnIndexItemProceed(IndexItemProceedEventArgs args)
        {
            var temp = indexItemProceed;
            if (temp != null) temp(this, args);
        }

        private void ProcessNodeDtoInternal(string currentPath, ExchangeOnlineTreeNodeDto nodeDto)
        {
            MediaRestoreNode node = new MediaRestoreNode(nodeDto);
            //if (node.IsExpanded && nodeDto.Level == NodeLevel.ExchangeOnlineMailbox && nodeDto.Children[0].Type == NodeType.TeamChannels) node.IsExpanded = false;
            GroupBasicIndex index = null;
            if (!node.IsVirtualNode)
            {
                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessNodeDtoInternalBegin, currentPath);
                bool isContainer = true;
                if (node.Level == NodeLevel.ExchangeOnlineItem) isContainer = false;
                this.logger.Info($"Begin to get index info, currentPath: {currentPath}, BackupTime: {restoreJob.BackupTime}, BackupJobId: {restoreJob.BackupJobId}, OnlyOneJob: {restoreJob.OnlyOneJob}.");
                index = LoadOneData(isContainer, new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
                this.logger.Info($"Finish to get index info, COL_PATH: {index?.Path}, COL_NAME: {index?.Name}, COL_JOB_ID: {index?.JobId} ");
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
            }
            if (node.IsExpanded)//真实的tree被展开
            {
                if (node.Level == NodeLevel.ExchangeOnlineItems)
                {
                    List<ExchangeOnlineTreeNodeDto> items = GetAllFiles(nodeDto, node.IsInverted);
                    if (node.IsInverted)//send unselected items
                        ProcessUnSelectedItems(items, currentPath, nodeDto);
                    else//send selected items
                        ProcessSelectedItems(items, currentPath);
                }
                else
                {
                    if (nodeDto.Children.Count != 0 && nodeDto.Children.Count == 2 && nodeDto.Children[1].Level.Equals(NodeLevel.ExchangeOnlineFolders))
                    {
                        var tempNodeDto = new ExchangeOnlineTreeNodeDto();
                        tempNodeDto.Children.Add(nodeDto.Children[0]);
                        tempNodeDto.Children.Add(nodeDto.Children[1]);
                        nodeDto = tempNodeDto;
                    }
                    foreach (ExchangeOnlineTreeNodeDto subNode in nodeDto.Children)
                    {
                        if (subNode.IsVirtualNode())
                            ProcessNodeDtoInternal(currentPath, subNode);
                        else
                            ProcessNodeDtoInternal(currentPath + ServiceConstants.Delimiter + subNode.Name, subNode);
                    }
                }
            }
            else//真实的tree没有展开
            {
                if (node.IsChecked)//check number一定是1
                {
                    if (!RestoreConfig.EntirePlannerPlan && (node.Level == NodeLevel.Office365Planner || node.Level == NodeLevel.Office365PlannerPlan)) RestoreConfig.EntirePlannerPlan = true;
                    this.ProcessSubContainers(currentPath, nodeDto, (index as GroupContainerIndex)?.NodeId);
                }
                else if (node.Level == NodeLevel.ExchangeOnlineItems)
                {
                    List<ExchangeOnlineTreeNodeDto> items = GetAllFiles(nodeDto, node.IsInverted);
                    if (node.IsInverted)//send unselected items
                        ProcessUnSelectedItems(items, currentPath, nodeDto);
                }
                else
                    throw new NodeCheckStateException("The leaf node check number must be 1.");
            }
        }

        private void ProcessSubContainers(String currentPath, ExchangeOnlineTreeNodeDto nodeDto, string nodeId = null)
        {
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessSubContainersBegin, currentPath);
            if (nodeDto.Level == NodeLevel.ExchangeOnlineMailbox || nodeDto.Level == NodeLevel.ExchangeOnlineFolder || nodeDto.Level == NodeLevel.ExchangeOnlineItems)
                this.ProcessSubItems(currentPath, nodeDto, nodeId);
            if (nodeDto.Level == NodeLevel.ExchangeOnlineMailbox || nodeDto.Level == NodeLevel.ExchangeOnlineFolder || nodeDto.Level == NodeLevel.ExchangeOnlineFolders ||
                nodeDto.Level == NodeLevel.Office365Planner || nodeDto.Level == NodeLevel.Office365PlannerPlan)
            {
                if (nodeDto.ChildrenLoaded)
                    return;
                List<GroupBasicIndex> subContainers = LoadFolders(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
                foreach (GroupBasicIndex index in subContainers)
                {
                    RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
                    this.ProcessSubData(index.Path, nodeDto, nodeId: (index as GroupContainerIndex)?.NodeId);
                }
            }
        }

        private void ProcessSubData(String currentPath, ExchangeOnlineTreeNodeDto nodeDto, bool isPlanner = false, string nodeId = null)
        {
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessSubDataBegin, currentPath);
            if (!isPlanner)
            {
                this.ProcessSubItems(currentPath, nodeDto, nodeId);
            }
            else
            {
                this.ProcessPlanSubItems(currentPath, nodeDto);
            }
            List<GroupBasicIndex> subContainers = LoadFolders(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
            foreach (GroupBasicIndex index in subContainers)
            {
                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
                var isPlannerIndex = index is PlannerIndex;
                var currentNodeId = isPlannerIndex ? null : (index as GroupContainerIndex)?.NodeId;
                this.ProcessSubData(index.Path, nodeDto, isPlannerIndex, currentNodeId);
            }
        }

        private void ProcessUnSelectedItems(List<ExchangeOnlineTreeNodeDto> items, string currentPath, ExchangeOnlineTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessUnSelectedItemsBegin, currentPath);
            bool send = true;
            var childNodeDto = new ExchangeOnlineTreeNodeDto();
            var allItems = RestoreIndexService.LoadItems(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
            foreach (GroupBasicIndex indexItem in allItems)
            {
                send = true;
                foreach (ExchangeOnlineTreeNodeDto child in items)
                {
                    if (child.Name.Equals(indexItem.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        send = false;
                        break;
                    }
                }
                if (send)
                {
                    if (nodeDto.Children.Exists(node => node.Name.EqualsIgnoreCase(indexItem.Name)))
                        childNodeDto = nodeDto.Children.Find(node => node.Name.EqualsIgnoreCase(indexItem.Name));
                    GroupBasicIndex index = RestoreIndexService.Load(false, new ExchangeIndexInfo(indexItem.Path, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
                    RestoreMarkMessage markMsg = new RestoreMarkMessage(childNodeDto);
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
                }
            }
        }

        private void ProcessSelectedItems(List<ExchangeOnlineTreeNodeDto> items, string currentPath)
        {
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessSelectedItemsBegin, currentPath);
            foreach (ExchangeOnlineTreeNodeDto childNode in items)
            {
                GroupBasicIndex index = RestoreIndexService.Load(false, new ExchangeIndexInfo(currentPath + ServiceConstants.Delimiter + childNode.Name, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
                RestoreMarkMessage markMsg = new RestoreMarkMessage(childNode);
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
            }
        }

        private void ProcessSubItems(String currentPath, ExchangeOnlineTreeNodeDto nodeDto, string parentId = null)
        {
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessSubItemsBegin, currentPath);
            Int64 itemCount = RestoreIndexService.GetItemsCount(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob) { ParentId = parentId });
            if (isJustCalculateCount)
            {
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = itemCount });
            }
            else if (itemCount > 0)
            {
                Dictionary<string, string> monthTime = GetMonthStartAndEndTime(currentPath, parentId);
                foreach (var timeInfo in monthTime.Values)
                {
                    string[] times = timeInfo.Split('-');
                    var topicIds = RestoreIndexService.GetTopicIds(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, long.Parse(times[0]), long.Parse(times[1]), restoreJob.OnlyOneJob) { ParentId = parentId });
                    foreach (var topicId in topicIds)
                    {
                        int offset = 0;
                        int range = 1000;
                        var hasTopicIdHistory = RestoreIndexService.IsTopicIdHistoryExist(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, long.Parse(times[0]), topicId, restoreJob.OnlyOneJob) { ParentId = parentId });
                        if (hasTopicIdHistory)
                        {
                            logger.Info("This topic id: {0} is exist in previous month and skip to handle.", topicId);
                            continue;
                        }
                        var leftCount = RestoreIndexService.GetOneConversationItemsCount(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, topicId, restoreJob.OnlyOneJob) { ParentId = parentId });
                        bool isTopic = true;
                        while (leftCount > 0)
                        {
                            var indexInfo = new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob);
                            indexInfo.OffSet = offset;
                            indexInfo.Length = (int)(leftCount > 1000 ? range : leftCount);
                            indexInfo.SortId = topicId;
                            indexInfo.MonthStartTime = long.Parse(times[0]);
                            indexInfo.MonthEndTime = long.Parse(times[1]);
                            indexInfo.ParentId = parentId;
                            logger.Info("Start to load next {0} items.", indexInfo.Length);
                            var allItems = RestoreIndexService.LoadConversationItems(indexInfo);
                            foreach (GroupBasicIndex index in allItems)
                            {
                                var exchangeId = index.Name.Substring(index.Name.LastIndexOf(ExchangeConstants.PathParser) + 1);
                                if (isTopic) RestoreConfig.TopicItemIds.Add(exchangeId);
                                isTopic = false;
                                if (!RestoreConfig.ItemCreateTimeInfo.ContainsKey(index.Name))
                                    RestoreConfig.ItemCreateTimeInfo.Add(index.Name, index.CreateTime);
                                else
                                    logger.Error("Skip the same item to restore, the item name is  {0}.", index.Name);
                                RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);//0 stand for agent overwrite
                                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
                            }
                            offset += indexInfo.Length;
                            leftCount -= indexInfo.Length;
                        }
                    }
                }
                logger.Info("Process items finish, parent folder path: {0}, items total count: {1}.", currentPath, itemCount);
            }
        }

        private void ProcessPlanSubItems(String currentPath, ExchangeOnlineTreeNodeDto nodeDto)
        {
            this.logger.Info(MediaServiceExchangeBackupResource.ExchangeRestoreTreeHandlerProcessSubItemsBegin, currentPath);
            Int64 itemCount = RestoreIndexService.GetItemsCount(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob));
            if (isJustCalculateCount)
            {
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = itemCount });
            }
            else if (itemCount > 0)
            {
                int offset = 0;
                int range = 1000;
                var leftCount = itemCount;
                while (leftCount > 0)
                {
                    var indexInfo = new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob);
                    indexInfo.OffSet = offset;
                    indexInfo.Length = (int)(leftCount > 1000 ? range : leftCount);
                    logger.Info("Start to load next {0} items.", indexInfo.Length);
                    var allItems = RestoreIndexService.LoadItems(indexInfo);
                    foreach (var index in allItems)
                    {
                        RestoreMarkMessage markMsg = new RestoreMarkMessage(nodeDto);//0 stand for agent overwrite
                        OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = index, MarkMessage = markMsg });
                    }

                    offset += indexInfo.Length;
                    leftCount -= indexInfo.Length;
                }
                logger.Info("Process items finish, parent folder path: {0}, items total count: {1}.", currentPath, itemCount);
            }
        }

        private Dictionary<string, string> GetMonthStartAndEndTime(string currentPath, string parentId)
        {
            var monthTime = new Dictionary<string, string>();
            try
            {
                List<long> itemCreatedTimes = RestoreIndexService.GetItemCreatedTime(new ExchangeIndexInfo(currentPath, restoreJob.BackupTime, restoreJob.BackupJobId, restoreJob.OnlyOneJob) { ParentId = parentId });
                var firstItemTime = new DateTime(itemCreatedTimes[0]);
                var lastItemTime = new DateTime(itemCreatedTimes[itemCreatedTimes.Count - 1]);
                monthTime = GetMonthTime(firstItemTime, lastItemTime);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred while to get channel conversation created time. Reason: {0}. ", ex);
            }
            return monthTime;
        }

        private static Dictionary<string, string> GetMonthTime(DateTime firstItemTime, DateTime lastItemTime)
        {
            var monthTime = new Dictionary<string, string>();
            if (firstItemTime.Year == lastItemTime.Year)
            {
                for (int month = firstItemTime.Month; month <= lastItemTime.Month; month++)
                {
                    DateTime monthStartTime = month != firstItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", firstItemTime.Year, month, 1)) : firstItemTime;
                    DateTime monthEndTime = month != lastItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", firstItemTime.Year, month + 1, 1)) : lastItemTime;
                    monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                }
            }
            else if (firstItemTime.Year < lastItemTime.Year)
            {
                for (int year = firstItemTime.Year; year <= lastItemTime.Year; year++)
                {
                    if (year == firstItemTime.Year)
                    {
                        for (int month = firstItemTime.Month; month <= 12; month++)
                        {
                            DateTime monthStartTime = month != firstItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, 1)) : firstItemTime;
                            DateTime monthEndTime = month == 12 ? DateTime.Parse(string.Format("{0}/{1}/{2}", year + 1, 1, 1)) : DateTime.Parse(string.Format("{0}/{1}/{2}", year, month + 1, 1));
                            monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                        }
                    }
                    else if (year < lastItemTime.Year)
                    {
                        for (int month = 1; month <= 12; month++)
                        {
                            DateTime monthStartTime = DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, 1));
                            DateTime monthEndTime = month == 12 ? DateTime.Parse(string.Format("{0}/{1}/{2}", year + 1, 1, 1)) : DateTime.Parse(string.Format("{0}/{1}/{2}", year, month + 1, 1));
                            monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                        }
                    }
                    else
                    {
                        for (int month = 1; month <= lastItemTime.Month; month++)
                        {
                            DateTime monthStartTime = DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, 1));
                            DateTime monthEndTime = month != lastItemTime.Month ? DateTime.Parse(string.Format("{0}/{1}/{2}", year, month + 1, 1)) : lastItemTime;
                            monthTime.Add(monthStartTime.ToString("yyyyMM"), string.Format("{0}-{1}", monthStartTime.Ticks, monthEndTime.Ticks));
                        }
                    }
                }
            }
            return monthTime;
        }

        private List<ExchangeOnlineTreeNodeDto> GetAllFiles(ExchangeOnlineTreeNodeDto itemsDto, Boolean isInverted)
        {
            var result = new List<ExchangeOnlineTreeNodeDto>();
            if (this.restoreJob.IsSearchTree)
                result = itemsDto.Children.FindAll(item => item.CheckNumber == 1);
            else
                result = itemsDto.Children.FindAll(item => item.CheckNumber == (isInverted ? 0 : 1));
            return result;
        }

        private GroupBasicIndex LoadOneData(bool isContainer, ExchangeIndexInfo indexInfo)
        {
            try
            {
                return RestoreIndexService.Load(isContainer, indexInfo);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Equals(RestoreConstants.CONVERT_TYPE_EXCEPTION, StringComparison.OrdinalIgnoreCase))
                    throw;
                logger.Warn("Load one date with exception: {0}", ex.ToString());
                this.RestoreIndexService.ProcessColumnUpgrate();
                return RestoreIndexService.Load(isContainer, indexInfo);
            }
        }

        private List<GroupBasicIndex> LoadFolders(ExchangeIndexInfo indexInfo)
        {
            try
            {
                return RestoreIndexService.LoadFolders(indexInfo);
            }
            catch (Exception ex)
            {
                if (!ex.Message.Equals(RestoreConstants.CONVERT_TYPE_EXCEPTION, StringComparison.OrdinalIgnoreCase))
                    throw;
                logger.Warn("Load folders with exception: {0}", ex.ToString());
                this.RestoreIndexService.ProcessColumnUpgrate();
                return RestoreIndexService.LoadFolders(indexInfo);
            }
        }

        public void ProcessExchangeNode(TreeNodeParameter treeParam, Dictionary<string, string> mapping)
        {
            this.restoreJob = treeParam.RestoreJob;
            this.isJustCalculateCount = treeParam.IsJustCalculateCount;
            this.ProcessExchangeNodeDtoInternal(mapping);
        }
        private void ProcessExchangeNodeDtoInternal(Dictionary<string, string> mapping)
        {
            logger.Info($"the mapping count is:{mapping.Count}");
            foreach (var map in mapping)
            {
                var items = RestoreIndexService.GetItemsByParentMd5(map.Key);
                logger.Info("Get items by parent md5, parent md5: {0}, items count: {1}", map.Key, items?.Count);
                foreach (var item in items)
                {
                    RestoreMarkMessage markMsg = new RestoreMarkMessage() { ParentName = map.Value, ChildCount = items.Count };
                    OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = item, MarkMessage = markMsg });
                    if (item.HasAttach)
                    {
                        var attachItems = RestoreIndexService.GetItemsByParentMd5(item.PathMD5);
                        logger.Info("Get items by parent md5, parent md5: {0}, items count: {1}", item.PathMD5, attachItems?.Count);
                        foreach (var atta in attachItems)
                        {
                            RestoreMarkMessage markMsg1 = new RestoreMarkMessage() { ParentName = map.Value };
                            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = atta, MarkMessage = markMsg1 });
                        }
                    }
                }
            }
        }

        public void ProcessSiteCollectionNode(string siteUrl, ExchangeRestoreJob restoreJob)
        {
            var siteCollectionNodeIndex = this.RestoreIndexService.GetArchiverBasicIndexByPathMd5(siteUrl.ToMD5HashCode());
            var siteCollectionNode = new TreeNodeInfo
            {
                Name = siteCollectionNodeIndex.Name,
                Type = siteCollectionNodeIndex.Type,
                BackupTime = siteCollectionNodeIndex.ArchiveTime,
                Index = siteCollectionNodeIndex,
                ItemName = siteCollectionNodeIndex.ItemName,
                ItemVersionNumber = siteCollectionNodeIndex.ItemVersionNumber,
                PathMd5 = siteCollectionNodeIndex.PathMD5,
                Path = siteCollectionNodeIndex.SitePath,
                SiteCollectionPath = siteUrl
            };
            RestoreMarkMessage markMsg = new RestoreMarkMessage() { RealPath = siteUrl };
            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = siteCollectionNodeIndex, MarkMessage = markMsg });
            ProcessWebNode(siteCollectionNode);
        }

        private void ProcessWebNode(TreeNodeInfo node)
        {
            var webNodes = LoadSubContainers(node.PathMd5, string.Empty);
            foreach (var web in webNodes)
            {
                RestoreMarkMessage webMarkMsg = new RestoreMarkMessage() { RealPath = web.RealPath };
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = web.Index, MarkMessage = webMarkMsg });
                var listNodes = LoadSubContainers(web.PathMd5, node.SiteCollectionPath);
                foreach (var list in listNodes)
                {
                    ProcessFolderNode(list);
                }
            }
        }

        private void ProcessFolderNode(TreeNodeInfo node)
        {
            RestoreMarkMessage listMarkMsg = new RestoreMarkMessage() { RealPath = node.RealPath };
            OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = node.Index, MarkMessage = listMarkMsg });
            ProcessItems(node);
            var folderNodes = LoadSubContainers(node.PathMd5, node.SiteCollectionPath);
            foreach(var folder in folderNodes)
            {
                ProcessFolderNode(folder);
            }
        }

        private void ProcessItems(TreeNodeInfo node)
        {
            var itemIndexs = this.RestoreIndexService.GetArchiverBasicIndexItemsInBodyByParentPathMd5(node.PathMd5);
            itemIndexs = FilterSpecifiedVersionItems(itemIndexs);
            foreach(var item in itemIndexs)
            {
                RestoreMarkMessage markMsg = new RestoreMarkMessage() { ParentPath = node.Path, RealPath = Path.Combine(node.RealPath, item.Name), ChildCount = itemIndexs.Count, SiteCollectionPath = node.SiteCollectionPath};
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = item, MarkMessage = markMsg });
            }
        }

        private List<ArchiverBasicIndex> FilterSpecifiedVersionItems(List<ArchiverBasicIndex> itemIndexs)
        {
            if (!ShouldProcessSpecifiedVersions() || itemIndexs == null || itemIndexs.Count == 0)
            {
                return itemIndexs;
            }

            var keepVersionsNumber = restoreJob.KeepVersionsNumber;
            if (keepVersionsNumber <= 0)
            {
                return itemIndexs;
            }

            var filteredItems = new List<ArchiverBasicIndex>();
            foreach (var itemGroup in itemIndexs.GroupBy(item => string.IsNullOrEmpty(item.NodeGuid) ? item.PathMD5 : item.NodeGuid, StringComparer.OrdinalIgnoreCase))
            {
                var orderedItems = itemGroup
                    .OrderBy(item => item.ItemVersionNumber)
                    .ThenBy(item => item.ArchiveTime)
                    .ToList();

                var startIndex = Math.Max(0, orderedItems.Count - keepVersionsNumber);
                filteredItems.AddRange(orderedItems.Skip(startIndex));
            }

            return filteredItems;
        }

        private bool ShouldProcessSpecifiedVersions()
        {
            return restoreJob.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions;
        }

        private List<TreeNodeInfo> LoadSubContainers(string parentPathMD5, string siteCollectionPath)
        {
            var indexes = this.RestoreIndexService.GetArchiverBasicIndexItemsByParentPathMd5(parentPathMD5);
            return indexes.ConvertAll(index => new TreeNodeInfo
            {
                Name = index.Name,
                Type = index.Type,
                BackupTime = index.ArchiveTime,
                Index = index,
                ItemName = index.ItemName,
                ItemVersionNumber = index.ItemVersionNumber,
                PathMd5 = index.PathMD5,
                SiteCollectionPath = siteCollectionPath,
                Path = BuildPath(index.Name, index.SitePath, index.Type, out var realPath),
                RealPath = realPath
            });
        }

        public string BuildPath(string name, string sitePath, string level, out string realPath)
        {
            string path = string.Empty;
            try
            {
                if (sitePath.StartsWith("http://"))
                {
                    path = sitePath.Remove(0, "http://".Length);
                }
                else if (sitePath.StartsWith("https://"))
                {
                    path = sitePath.Remove(0, "https://".Length);
                }
                switch (level)
                {
                    case "E":
                        path = new StringBuilder(path).Append("\\").ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_");
                        realPath = sitePath;
                        break;
                    case "W":
                        if (name.Equals("."))
                        {
                            path = new StringBuilder(path).Append("\\").ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_");
                            realPath = sitePath;
                            break;
                        }
                        string webPath = name;
                        if (webPath.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                        {
                            webPath = webPath.Remove(0, ".\\".Length);
                        }
                        realPath = Path.Combine(sitePath, webPath);
                        var webSplitPath = webPath.TrimStart('/').Split('/');
                        webPath = string.Join("\\", webSplitPath.Select(_ => _.Replace("/", "_").Replace(":", "_").Replace(".", "_")).ToArray());
                        path = Path.Combine(new StringBuilder(path).Append("\\").ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_"), webPath);
                        break;
                    case "L":
                    case "F":
                        string listPath = name;
                        if (listPath.StartsWith(".\\", StringComparison.OrdinalIgnoreCase))
                        {
                            listPath = listPath.Remove(0, ".\\".Length);
                        }
                        realPath = Path.Combine(sitePath, listPath);
                        var listSplitPath = listPath.TrimStart('/').Split('/');
                        listPath = string.Join("\\", listSplitPath.Select(_ => _.Replace("/", "_").Replace(":", "_").Replace(".", "_")).ToArray());
                        if (sitePath.StartsWith("http://"))
                        {
                            path = sitePath.Remove(0, "http://".Length);
                        }
                        else if (sitePath.StartsWith("https://"))
                        {
                            path = sitePath.Remove(0, "https://".Length);
                        }
                        path = new StringBuilder(path).Append("\\").Append(listPath).ToString().Replace("/", "_").Replace(":", "_").Replace(".", "_");
                        break;
                    default:
                        realPath = string.Empty;
                        return string.Empty;
                }
                logger.Info($"Finish to build export path, path[{path}], level: [{level}]");
                return path;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurs while build export path. Ex: {e}");
                realPath = string.Empty;
                return string.Empty;
            }
        }
    }
}