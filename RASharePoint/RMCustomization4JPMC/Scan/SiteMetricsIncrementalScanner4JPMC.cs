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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.Archiver.CAMLHelper;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Base;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Interface;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Implement;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Discovery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Utility;
using AvePoint.RA.RACommonUtility.Extension;
using System.Collections;
using System.Diagnostics;
using AvePoint.Wrapper.Restore;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan
{
    internal class SiteMetricsIncrementalScanner4JPMC : SiteMetricsScanner4JPMCBase
    {
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SiteMetricsIncrementalScanner4JPMC));
        private readonly JPMCTenantConfig mJPMCTenantConfig;
        private List<string> DesignLists = new List<string>();

        private const int FolderCacheCapacity = 2000;
        private const int BatchProgressLogInterval = 20;
        private readonly Dictionary<string, ArchiverNodeItem> mFolderNodeCache = new Dictionary<string, ArchiverNodeItem>(StringComparer.OrdinalIgnoreCase);
        private long mParentFolderCacheHitCount;
        private long mParentFolderCacheHitByUniqueIdCount;
        private long mParentFolderCacheHitByFileDirRefCount;
        private long mParentFolderCacheHitByItemUrlCount;
        private long mParentFolderCacheMissCount;
        private long mParentFolderSlowPathCount;
        private bool mFallbackToFullScan;
        internal bool ShouldFallbackToFullScan => mFallbackToFullScan;

        private IDiscoverNodeWorker mDiscoverWorker = null;
        public override IDiscoverNodeWorker discoverWorker
        {
            get
            {
                if (mDiscoverWorker == null)
                {
                    mDiscoverWorker = new JPMCScanDiscovrerNodeWorker(jobSettings, mConfiguration, mDependencyObjs, mJPMCTenantConfig);
                }
                return mDiscoverWorker;
            }
            set { }
        }

        public SiteMetricsIncrementalScanner4JPMC(ScanJobSettings scanJobSettings, JPMCTenantConfig jpmcConfig, string siteUrl = "")
            : base(scanJobSettings)
        {
            mJPMCTenantConfig = jpmcConfig;
            DesignLists = GetDesignLists();
        }

        protected override bool ShouldCalculateListCount()
        {
            return false;
        }

        protected override Dictionary<Guid, AveDiscoverWeb> GetWebsForSiteCollection(AveDiscoverSite discoverySite)
        {
            return discoverySite?.GetChangeWebs() ?? new Dictionary<Guid, AveDiscoverWeb>();
        }

        protected override bool ShouldProcessFolderContents(ArchiverNodeItem folderNode)
        {
            mLog.Info("Incremental discover skips folder traversal for node:{0}.", folderNode.FullPath);
            return false;
        }

        protected override Dictionary<Guid, AveDiscoverList> GetListsForWeb(AveDiscoverWeb discoverWeb)
        {
            return discoverWeb?.GetChangeLists() ?? new Dictionary<Guid, AveDiscoverList>();
        }

        protected override bool ShouldHandleDeletedList(AveDiscoverList list)
        {
            return list.ChangeType == ChangeType.Delete;
        }

        protected override bool ShouldHandleDeletedWeb(AveDiscoverWeb discoverWeb)
        {
            return discoverWeb.ChangeType == ChangeType.Delete;
        }

        public override bool ListSkipCheck(ArchiverNodeItem listNode)
        {
            try
            {
                var discoverList = (AveDiscoverList)listNode.DiscoverSPObject;
                if (discoverList != null)
                {
                    if (CheckIsDesignList(discoverList.Name + discoverList.ListTemplate.ToString()))
                    {
                        mLog.Info("Skip the design list {0}", discoverList.Name);
                        return true;
                    }
                    if (listNode.SPList != null && NeedSkipGenericList(listNode.SPList))
                    {
                        mLog.Info("Skip general list. List url: {0} .", listNode.FullPath);
                        return true;
                    }
                    foreach (var rule in mConfiguration.RuleCollection)
                    {
                        foreach (var filter in rule.Value.Filters)
                        {
                            if (filter.RuleType == PolicyRuleType.Column)
                            {
                                var exist = false;
                                var columnName = filter.Rule.Value1;
                                if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                                {
                                    var internalName = columnName.Trim(['[', ']']);
                                    exist = (listNode?.SPList?.Fields?.ContainsFieldWithInternalName(internalName)).GetValueOrDefault();
                                }
                                else
                                {
                                    exist = (listNode?.SPList?.Fields?.ContainsField(columnName)).GetValueOrDefault();
                                }
                                if (!exist)
                                {
                                    mLog.Info($"Skip this list, because column {columnName} is not exist, list URL:{listNode.FullPath}");
                                    return true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    mLog.Info("CheckIsDesignList discoverList is null");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"CheckIsDesignList error: ({e})");
            }
            return false;
        }

        public override async Task ProcessListAsync(ArchiverNodeItem list, bool needInitInfo = false)
        {
            mLog.Info("Begin process list,title is:{0}.", list.Title);
            using (var pc = new AvePerformanceScope("ArchiverScan.ProcessList"))
            {
                try
                {
                    using (new CheckJobStopScope()) { }
                    if (ListSkipCheck(list))
                    {
                        return;
                    }

                    if (needInitInfo)
                    {
                        await InitialSPObjectInfoAsync(discoverWorker, list);
                    }

                    if (await discoverWorker.ProcessContainerAsync(list, ProcessType.NeedProcess) == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }

                    var discoverList = list.DiscoverSPObject as AveDiscoverList;
                    mLog.Info("Incremental discover: begin processing list changes. Path:{0}.", list.FullPath);
                    await ProcessListChangesAsync(list, discoverList, discoverWorker);
                    mLog.Info("Incremental discover: finished processing list changes. Path:{0}.", list.FullPath);
                    return;
                }
                catch (JobStopException)
                {
                    throw;
                }
                catch (AveWrapperI18NException IUPEx)
                {
                    mLog.Info("List UserName Or Password Incorrect. Path:{0}. Message:{1}.", list.FullPath, IUPEx.ToString());
                    throw;
                }
                catch (SPObjectReadOnlyException sroe)
                {
                    mLog.Info("List is ReadOnly. Path:{0}. Message:{1}.", list.FullPath, sroe.ToString());

                    throw;
                }
                catch (SPObjectLockedException sle)
                {
                    mLog.Info("List is Locked. Path:{0}. Message:{1}.", list.FullPath, sle.ToString());
                    throw;
                }
                catch (SPObjectNotFoundException ex)
                {
                    mLog.Info("List Not Found. Path:{0}. Message:{1}.", list.FullPath, ex.ToString());
                    throw;
                }
                catch (Exception e)
                {
                    mLog.Error("An unexpected error occurred while processing list node.Path:{0}.Message:{1}.", list.FullPath, e.ToString());
                    throw;
                }
                finally
                {
                    mConfiguration.ProgressDto.UpdateProgress();
                }
            }
        }

        private async Task ProcessListChangesAsync(ArchiverNodeItem listNode, AveDiscoverList discoverList, IDiscoverNodeWorker activeDiscoverWorker)
        {
            if (discoverList == null)
            {
                mLog.Warn("Discover list is null when processing change discover.");
                return;
            }

            var rootFolder = discoverList.GetRootFolder(true);
            if (rootFolder == null)
            {
                mLog.Warn($"Root folder is null for list {listNode.FullPath} when processing change discover.");
                return;
            }

            var siteUrl = mDiscoverSite?.Site?.Url ?? listNode.SiteUrl;

            try
            {
                ResetParentFolderResolveCounters();
                mFolderNodeCache.Clear();
                var rootFolderNode = listNode.GenerateFolderNodeItem(rootFolder, NodeLevel.RootFolder, siteUrl, mConfiguration);
                TryAddFolderNodeCache(GetFolderCacheKey(rootFolder), rootFolderNode);
                var containerResult = await activeDiscoverWorker.ProcessContainerAsync(rootFolderNode, ProcessType.NeedProcess);
                if (containerResult == ProcessResult.SkipCurrentNode)
                {
                    return;
                }

                if (mConfiguration.SkipDiscoverItemForFolderLevelRule)
                {
                    mLog.Info($"Current rule is folder rule and skip discover folder sub items.Path:{rootFolderNode.FullPath}.");
                    return;
                }

                var changedItems = GetChangedItems(discoverList, listNode.WebId);
                LogLatestChangeLogToken(listNode, changedItems);
                if (changedItems.Count == 0)
                {
                    mLog.Info($"No changed items found for list {listNode.FullPath}.");
                    return;
                }

                var deletedNodeIds = ExtractDeletedNodeIds(changedItems);
                if (deletedNodeIds.Count > 0)
                {
                    await RemoveDeletedFoldersAsync(listNode, deletedNodeIds, activeDiscoverWorker);
                    await RemoveDeletedItemsAsync(listNode, deletedNodeIds, activeDiscoverWorker);
                }

                var rowIds = ExtractChangedRowIds(changedItems);
                mLog.Info($"Found {rowIds.Count} changed row ids for list {listNode.FullPath}. Ids:{string.Join(",", rowIds)}");
                if (rowIds.Count == 0)
                {
                    return;
                }

                var spList = listNode.SPList;
                if (spList == null)
                {
                    mLog.Warn($"SPList is null for list {listNode.FullPath}, skip processing change discover.");
                    return;
                }

                await ProcessItemsByRowIdsAsync(spList, rowIds, rootFolderNode, activeDiscoverWorker);
            }
            finally
            {
                mLog.Info("Parent folder cache stats. List:{0}, Hit:{1}, HitByUniqueId:{2}, HitByFileDirRef:{3}, HitByItemUrl:{4}, Miss:{5}, SlowPath:{6}.",
                    listNode?.FullPath,
                    mParentFolderCacheHitCount,
                    mParentFolderCacheHitByUniqueIdCount,
                    mParentFolderCacheHitByFileDirRefCount,
                    mParentFolderCacheHitByItemUrlCount,
                    mParentFolderCacheMissCount,
                    mParentFolderSlowPathCount);
                mFolderNodeCache.Clear();
                rootFolder.Dispose();
            }
        }

        private void ResetParentFolderResolveCounters()
        {
            mParentFolderCacheHitCount = 0;
            mParentFolderCacheHitByUniqueIdCount = 0;
            mParentFolderCacheHitByFileDirRefCount = 0;
            mParentFolderCacheHitByItemUrlCount = 0;
            mParentFolderCacheMissCount = 0;
            mParentFolderSlowPathCount = 0;
        }

        private bool CheckIsDesignList(string listInfo)
        {
            bool isDesignList = false;
            try
            {
                if (DesignLists.Contains(listInfo))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error has occurred when CheckIsDesignList, message:{e.Message}");
            }
            return isDesignList;
        }

        private List<string> GetDesignLists()
        {
            return WebUtil.GetDesignLists(false);
        }

        private bool NeedSkipGenericList(IAveList list)
        {
            return list.BaseType == AveBaseType.GenericList;
        }

        protected override void DownloadScanDbIfExists(string blobPath, string localFilePath)
        {
            if (string.IsNullOrWhiteSpace(blobPath) || string.IsNullOrWhiteSpace(localFilePath))
            {
                return;
            }

            try
            {
                if (!RAStorageUtil.TryGetReportBlobLength(blobPath, out _))
                {
                    mLog.Warn($"No existing scan db blob found at {blobPath}. Fallback to full scan.");
                    TriggerFullScanFallback();
                    return;
                }

                var directory = Path.GetDirectoryName(localFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                RAStorageUtil.DownloadReportBlobToFile(blobPath, localFilePath);
                mLog.Info($"Downloaded scan db from storage path {blobPath}.");
            }
            catch (Exception ex)
            {
                mLog.Warn($"Failed to download scan db from storage path {blobPath}. Error:{ex}");
                TriggerFullScanFallback();
            }
        }

        private void TriggerFullScanFallback()
        {
            mFallbackToFullScan = true;
            if (mConfiguration != null)
            {
                mConfiguration.UseIncrementalDiscover = false;
                mConfiguration.IncrementalDiscoverStartTimeTicks = DateTime.MinValue.Ticks;
                mConfiguration.IncrementalDiscoverEndTimeTicks = DateTime.MinValue.Ticks;
            }
        }

        private Dictionary<string, object> GetChangedItems(AveDiscoverList discoverList, Guid webId)
        {
            Dictionary<string, object> changedItems;
            using (var performanceScope = new AvePerformanceScope("ArchiverScan.GetListChangedItems"))
            {
                changedItems = discoverList.GetListChangedItems(webId);
            }

            return changedItems ?? new Dictionary<string, object>();
        }

        private void LogLatestChangeLogToken(ArchiverNodeItem listNode, Dictionary<string, object> changedItems)
        {
            if (changedItems == null || changedItems.Count == 0)
            {
                mLog.Info("No change log entries found for list {0}; latest change token is unavailable.", listNode?.FullPath);
                return;
            }

            var latestChangedItem = changedItems.Values.LastOrDefault() as Dictionary<string, object>;
            if (latestChangedItem == null)
            {
                mLog.Warn("Failed to resolve the latest change log token for list {0}. Change entry count:{1}.", listNode?.FullPath, changedItems.Count);
                return;
            }

            var latestToken = GetChangeTokenString(latestChangedItem);
            mLog.Info("Last change log entry for list {0}. Token:{1}.",
                listNode?.FullPath,
                string.IsNullOrWhiteSpace(latestToken) ? "N/A" : latestToken);
        }

        private string GetChangeTokenString(Dictionary<string, object> changedItem)
        {
            if (changedItem == null)
            {
                return string.Empty;
            }

            if (changedItem.TryGetValue("ChangeToken", out var rawToken))
            {
                var token = ConvertChangeTokenToString(rawToken);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    return token;
                }
            }

            return string.Empty;
        }

        private string ConvertChangeTokenToString(object rawToken)
        {
            if (rawToken == null)
            {
                return string.Empty;
            }

            if (rawToken is string tokenString)
            {
                return tokenString;
            }

            var stringValueProperty = rawToken.GetType().GetProperty("StringValue");
            if (stringValueProperty?.GetValue(rawToken) is string reflectedToken && !string.IsNullOrWhiteSpace(reflectedToken))
            {
                return reflectedToken;
            }

            var tokenText = rawToken.ToString();
            return string.Equals(tokenText, rawToken.GetType().FullName, StringComparison.Ordinal)
                ? string.Empty
                : tokenText;
        }

        private async Task RemoveDeletedItemsAsync(ArchiverNodeItem listNode, List<Guid> deletedItemIds, IDiscoverNodeWorker activeDiscoverWorker)
        {
            if (deletedItemIds == null || deletedItemIds.Count == 0)
            {
                return;
            }

            if (activeDiscoverWorker is ISiteMetricsDeletionHandler deletionHandler)
            {
                await deletionHandler.RemoveItemDataAsync(listNode.WebId, listNode.ID, deletedItemIds, listNode.FullPath);
            }
            else
            {
                mLog.Warn($"Detected {deletedItemIds.Count} deleted items for list {listNode.FullPath}, but no deletion handler is available; cached data might become stale.");
            }
        }

        private async Task RemoveDeletedFoldersAsync(ArchiverNodeItem listNode, List<Guid> deletedFolderIds, IDiscoverNodeWorker activeDiscoverWorker)
        {
            if (deletedFolderIds == null || deletedFolderIds.Count == 0)
            {
                return;
            }

            if (activeDiscoverWorker is ISiteMetricsDeletionHandler deletionHandler)
            {
                await deletionHandler.RemoveFolderDataAsync(listNode.WebId, listNode.ID, deletedFolderIds, listNode.FullPath);
            }
            else
            {
                mLog.Warn($"Detected {deletedFolderIds.Count} deleted folders for list {listNode.FullPath}, but no deletion handler is available; cached data might become stale.");
            }
        }

        private List<int> ExtractChangedRowIds(Dictionary<string, object> changedItems)
        {
            return changedItems.Values
                .Select(i => i as Dictionary<string, object>)
                .Where(i => i != null && (!i.ContainsKey("Hidden") || !(bool)i["Hidden"]))
                .Where(i => (int)i["ChangeType"] != (int)ChangeType.Delete)
                .Select(i => (int)i["ItemId"])
                .Distinct()
                .ToList();
        }

        private List<Guid> ExtractDeletedNodeIds(Dictionary<string, object> changedItems)
        {
            return changedItems.Values
                .Select(item => item as Dictionary<string, object>)
                .Where(item => item != null && (!item.ContainsKey("Hidden") || !(bool)item["Hidden"]))
                .Where(item => (int)item["ChangeType"] == (int)ChangeType.Delete)
                .Select(item =>
                {
                    if (TryGetItemUniqueId(item, out var uniqueId))
                    {
                        return uniqueId;
                    }

                    mLog.Warn("Change log entry for deleted node is missing UniqueId; skip sqlite cleanup for this entry.");
                    return Guid.Empty;
                })
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();
        }

        private bool TryGetItemUniqueId(Dictionary<string, object> changedItem, out Guid uniqueId)
        {
            uniqueId = Guid.Empty;
            var uniqueKeys = new[] { "UniqueId", "ItemUniqueId", "ObjectId" };
            foreach (var key in uniqueKeys)
            {
                if (changedItem.TryGetValue(key, out var value) && value != null && Guid.TryParse(value.ToString(), out uniqueId))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task ProcessItemsByRowIdsAsync(IAveList list, List<int> rowIds, ArchiverNodeItem rootFolderNode, IDiscoverNodeWorker activeDiscoverWorker)
        {
            if (list == null || rowIds == null || rowIds.Count == 0)
            {
                return;
            }

            var totalBatches = (rowIds.Count + 99) / 100;
            for (int index = 0; index < rowIds.Count; index += 100)
            {
                var batchIds = rowIds.Skip(index).Take(100).ToList();
                AveCamlQuery query = BuildRowIdQuery(batchIds);
                IEnumerable<IAveListItem> partial = null;
                using (var performanceScope = new AvePerformanceScope("ArchiverScan.GetItemsByRowId"))
                {
                    partial = list.GetItemsForRecords(query, index == 0);
                }

                await ProcessChangedListItemsAsync(partial ?? Enumerable.Empty<IAveListItem>(), rootFolderNode, activeDiscoverWorker);

                var currentBatch = (index / 100) + 1;
                if (currentBatch % BatchProgressLogInterval == 0 || currentBatch == totalBatches)
                {
                    var workingSetMb = ProcessUtil.GetProcessMemoryMB();
                    var listUrl = list.FullUrl();
                    mLog.Info("Incremental batch progress for list {0}: batch {1}/{2}, rowIds:{3}, processWorkingSetMB:{4}.",
                        string.IsNullOrWhiteSpace(listUrl) ? rootFolderNode?.FullPath : listUrl,
                        currentBatch,
                        totalBatches,
                        rowIds.Count,
                        workingSetMb);
                }
            }
        }

        private AveCamlQuery BuildRowIdQuery(List<int> rowIds)
        {
            AveCamlQuery query = new AveCamlQuery
            {
                LoadAllItems = false,
                ListItemCollectionPosition = new AveItemCollectionPosition(),
                DatesInUtc = true
            };

            CAMLManager manager = new CAMLManager(Types.ScopeTypes.RecursiveAll);
            QueryGroup group = new QueryGroup();

            foreach (var rowId in rowIds)
            {
                group.Conditions.Add(new QueryCondition(
                    Types.JoinTypes.Or,
                    Types.FieldRefTypes.Name,
                    "ID",
                    Types.FieldTypes.Number,
                    Types.QueryTypes.Eq,
                    rowId.ToString()));
            }

            manager.QueryGroup.AddGroup(group);
            query.ViewXml = manager.GetFullCAML(false);
            mLog.Info($"RowId CAML query ViewXml:{query.ViewXml}");
            return query;
        }

        private async Task ProcessChangedListItemsAsync(IEnumerable<IAveListItem> items, ArchiverNodeItem rootFolderNode, IDiscoverNodeWorker activeDiscoverWorker)
        {
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                using (new CheckJobStopScope()) { }
                if (item.FileSystemObjectType == AveFileSystemObjectType.Folder)
                {
                    await HandleChangedFolderAsync(item, rootFolderNode, activeDiscoverWorker);
                    continue;
                }

                await HandleChangedItemAsync(item, rootFolderNode, activeDiscoverWorker);
            }
        }

        private async Task HandleChangedFolderAsync(IAveListItem item, ArchiverNodeItem rootFolderNode, IDiscoverNodeWorker activeDiscoverWorker)
        {
            using (var performanceScope = new AvePerformanceScope("ArchiverScan.HandleChangedFolder"))
            {
                var aveFolder = item?.Folder;
                if (aveFolder == null)
                {
                    mLog.Warn("Changed entry is a folder but Folder object is null; skip incremental folder handling. Path:{0}.", item?.Url);
                    return;
                }

                var changedFolderNode = await BuildFolderNodeChainAsync(aveFolder, rootFolderNode, activeDiscoverWorker);
                if (changedFolderNode == null)
                {
                    mLog.Warn("Failed to resolve folder node chain for changed folder. Path:{0}.", item?.Url);
                    return;
                }

                await activeDiscoverWorker.ProcessContainerAsync(changedFolderNode, ProcessType.NeedProcess);
            }
        }

        private async Task HandleChangedItemAsync(IAveListItem item, ArchiverNodeItem rootFolderNode, IDiscoverNodeWorker activeDiscoverWorker)
        {
            IAveFolder parentFolder;
            ArchiverNodeItem parentFolderNode;
            var hasParentContext = false;
            using (var performanceScope = new AvePerformanceScope("ArchiverScan.ResolveParentFolderNode"))
            {
                parentFolderNode = rootFolderNode;
                parentFolder = null;

                if (TryResolveParentFolderNodeFromCache(item, out var cachedParentFolderNode))
                {
                    parentFolderNode = cachedParentFolderNode;
                    hasParentContext = true;
                }
                else
                {
                    mParentFolderSlowPathCount++;
                    parentFolder = item?.File?.ParentFolder;
                    if (parentFolder != null)
                    {
                        parentFolderNode = await BuildFolderNodeChainAsync(parentFolder, rootFolderNode, activeDiscoverWorker);
                        if (parentFolderNode == null)
                        {
                            mLog.Warn("Failed to resolve folder node chain for changed item. Path:{0}.", item?.Url);
                            return;
                        }

                        hasParentContext = true;
                    }
                }

                if (hasParentContext)
                {
                    var containerResult = await activeDiscoverWorker.ProcessContainerAsync(parentFolderNode, ProcessType.NeedProcess);
                    if (containerResult == ProcessResult.SkipCurrentNode)
                    {
                        return;
                    }
                }
            }

            if (parentFolderNode == null)
            {
                mLog.Warn("Failed to resolve parent folder node for changed item; skip incremental item handling. Path:{0}.", item?.Url);
                return;
            }

            var itemNode = GenerateItemNode(parentFolderNode, parentFolder, item);

            using (itemNode)
            {
                if (itemNode == null)
                {
                    return;
                }

                await activeDiscoverWorker.ProcessItemAsync(itemNode, parentFolderNode);
            }
        }

        private bool TryResolveParentFolderNodeFromCache(IAveListItem item, out ArchiverNodeItem parentFolderNode)
        {
            parentFolderNode = null;

            var parentUniqueId = TryGetParentUniqueId(item);
            if (!string.IsNullOrWhiteSpace(parentUniqueId) && mFolderNodeCache.TryGetValue(parentUniqueId, out parentFolderNode))
            {
                mParentFolderCacheHitCount++;
                mParentFolderCacheHitByUniqueIdCount++;
                return true;
            }

            var parentFolderPath = TryGetParentFolderServerRelativePath(item, out var pathFromFileDirRef);
            if (!string.IsNullOrWhiteSpace(parentFolderPath) && mFolderNodeCache.TryGetValue(parentFolderPath, out parentFolderNode))
            {
                mParentFolderCacheHitCount++;
                if (pathFromFileDirRef)
                {
                    mParentFolderCacheHitByFileDirRefCount++;
                }
                else
                {
                    mParentFolderCacheHitByItemUrlCount++;
                }
                return true;
            }

            mParentFolderCacheMissCount++;
            return false;
        }

        private string TryGetParentUniqueId(IAveListItem item)
        {
            var fieldValues = item?.FieldValues;
            if (fieldValues == null)
            {
                return string.Empty;
            }

            if (!fieldValues.TryGetValue("ParentUniqueId", out var rawParentUniqueId) || rawParentUniqueId == null)
            {
                return string.Empty;
            }

            return Guid.TryParse(rawParentUniqueId.ToString(), out var parsedParentUniqueId)
                ? parsedParentUniqueId.ToString("D")
                : string.Empty;
        }

        private string TryGetParentFolderServerRelativePath(IAveListItem item, out bool fromFileDirRef)
        {
            fromFileDirRef = false;
            var fieldValues = item?.FieldValues;
            if (fieldValues != null && fieldValues.TryGetValue("FileDirRef", out var rawFileDirRef) && rawFileDirRef != null)
            {
                var normalizedFromField = NormalizeFolderCacheKey(rawFileDirRef.ToString());
                if (!string.IsNullOrWhiteSpace(normalizedFromField))
                {
                    fromFileDirRef = true;
                    return normalizedFromField;
                }
            }

            var itemUrl = item?.Url;
            if (string.IsNullOrWhiteSpace(itemUrl))
            {
                return string.Empty;
            }

            var path = itemUrl;
            if (Uri.TryCreate(itemUrl, UriKind.Absolute, out var absoluteUri))
            {
                path = absoluteUri.AbsolutePath;
            }

            var lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex <= 0)
            {
                return string.Empty;
            }

            return NormalizeFolderCacheKey(path.Substring(0, lastSlashIndex));
        }

        private string NormalizeFolderCacheKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var normalized = key.Trim();
            if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
            {
                normalized = absoluteUri.AbsolutePath;
            }

            return normalized.TrimEnd('/');
        }

        private async Task<ArchiverNodeItem> BuildFolderNodeChainAsync(IAveFolder targetFolder, ArchiverNodeItem rootFolderNode, IDiscoverNodeWorker activeDiscoverWorker)
        {
            var sw = Stopwatch.StartNew();
            var targetPath = targetFolder?.ServerRelativeUrl ?? "(null)";
            if (targetFolder == null || rootFolderNode == null || activeDiscoverWorker == null)
            {
                sw.Stop();
                mLog.Info($"BuildFolderNodeChainAsync skipped. Target:{targetPath}, elapsed:{sw.ElapsedMilliseconds} ms");
                return null;
            }

            var targetKey = GetFolderCacheKey(targetFolder);
            try
            {
                if (!string.IsNullOrWhiteSpace(targetKey) && mFolderNodeCache.TryGetValue(targetKey, out var cachedTarget))
                {
                    return cachedTarget;
                }

                var siteUrl = mDiscoverSite?.Site?.Url ?? rootFolderNode.SiteUrl;
                var ancestorStack = BuildAncestorStack(targetFolder);
                var currentNode = rootFolderNode;

                while (ancestorStack.Count > 0)
                {
                    var ancestorFolder = ancestorStack.Pop();
                    var ancestorKey = GetFolderCacheKey(ancestorFolder);
                    if (!string.IsNullOrWhiteSpace(ancestorKey) && mFolderNodeCache.TryGetValue(ancestorKey, out var cachedAncestor))
                    {
                        currentNode = cachedAncestor;
                        continue;
                    }

                    var discoverAncestorFolder = CreateDiscoverFolder(ancestorFolder);
                    if (discoverAncestorFolder == null)
                    {
                        mLog.Warn("Failed to build discover folder for ancestor while resolving folder chain. Path:{0}.", ancestorFolder?.ServerRelativeUrl);
                        return null;
                    }

                    currentNode = currentNode.GenerateFolderNodeItem(discoverAncestorFolder, NodeLevel.Folder, siteUrl, mConfiguration);
                    if (currentNode == null)
                    {
                        mLog.Warn("Failed to generate folder node for ancestor while resolving folder chain. Path:{0}.", ancestorFolder?.ServerRelativeUrl);
                        return null;
                    }

                    var processResult = await activeDiscoverWorker.ProcessContainerAsync(currentNode, ProcessType.NeedProcess);
                    if (processResult == ProcessResult.SkipCurrentNode)
                    {
                        return null;
                    }

                    if (!string.IsNullOrWhiteSpace(ancestorKey) && currentNode != null)
                    {
                        TryAddFolderNodeCache(ancestorKey, currentNode);
                    }
                }

                var discoverTargetFolder = CreateDiscoverFolder(targetFolder);
                if (discoverTargetFolder == null)
                {
                    mLog.Warn("Failed to build discover folder for changed folder. Path:{0}.", targetFolder.ServerRelativeUrl);
                    return null;
                }

                var targetNode = currentNode.GenerateFolderNodeItem(discoverTargetFolder, NodeLevel.Folder, siteUrl, mConfiguration);
                TryAddFolderNodeCache(targetKey, targetNode);

                return targetNode;
            }
            finally
            {
                sw.Stop();
                mLog.Info($"BuildFolderNodeChainAsync elapsed:{sw.ElapsedMilliseconds} ms for folder:{targetPath}");
            }
        }

        private Stack<IAveFolder> BuildAncestorStack(IAveFolder targetFolder)
        {
            var ancestors = new Stack<IAveFolder>();
            var current = targetFolder?.ParentFolder;
            while (current != null && current.Exists)
            {
                ancestors.Push(current);
                current = current.ParentFolder;
            }

            return ancestors;
        }

        private string GetFolderCacheKey(IAveFolder folder)
        {
            if (folder == null)
            {
                return string.Empty;
            }

            if (folder.UniqueId != Guid.Empty)
            {
                return folder.UniqueId.ToString("D");
            }

            return (folder.ServerRelativeUrl ?? string.Empty).TrimEnd('/');
        }

        private string GetFolderCacheKey(AveDiscoverFolder folder)
        {
            if (folder == null)
            {
                return string.Empty;
            }

            if (folder.DocID != Guid.Empty)
            {
                return folder.DocID.ToString("D");
            }

            return NormalizeFolderCacheKey(folder.FullUrl);
        }

        private void TryAddFolderNodeCache(string key, ArchiverNodeItem node)
        {
            if (string.IsNullOrWhiteSpace(key) || node == null)
            {
                return;
            }

            if (mFolderNodeCache.Count >= FolderCacheCapacity)
            {
                mFolderNodeCache.Clear();
                mLog.Warn($"Folder node cache exceeded capacity {FolderCacheCapacity}, cache has been reset.");
            }

            mFolderNodeCache[key] = node;
        }

        private ArchiverNodeItem GenerateItemNode(ArchiverNodeItem folderNode, IAveFolder parentFolder, IAveListItem item)
        {
            if (folderNode == null || item == null)
            {
                return null;
            }

            AveDiscoverFolder discoverParentFolder = null;
            var shouldDisposeDiscoverParentFolder = false;

            if (parentFolder != null)
            {
                discoverParentFolder = CreateDiscoverFolder(parentFolder);
                shouldDisposeDiscoverParentFolder = true;
            }
            else
            {
                discoverParentFolder = folderNode.DiscoverSPObject as AveDiscoverFolder;
            }

            if (discoverParentFolder == null)
            {
                return null;
            }

            try
            {
                var itemNode = folderNode.GenerateItemNodeItemV2(item, discoverParentFolder, mConfiguration);
                if (itemNode == null)
                {
                    return null;
                }

                PopulateItemNode(itemNode, item);
                return itemNode;
            }
            finally
            {
                if (shouldDisposeDiscoverParentFolder)
                {
                    discoverParentFolder.Dispose();
                }
            }
        }

        private void PopulateItemNode(ArchiverNodeItem itemNode, IAveListItem item)
        {
            if (itemNode == null || item == null)
            {
                return;
            }

            itemNode.FullPath = item.Url;
        }

        private AveDiscoverFolder CreateDiscoverFolder(IAveFolder sourceFolder)
        {
            if (sourceFolder == null)
            {
                return null;
            }

            var discoverFolder = new AveDiscoverFolder(sourceFolder.ParentWeb.Site, sourceFolder.ParentWeb.ID, sourceFolder.ServerRelativeUrl, DiscoverModule.Archive, mFactory, sourceFolder.ParentListId, sourceFolder.ParentWeb)
            {
                ID = sourceFolder.ID,
                DocID = sourceFolder.UniqueId,
                LeafName = sourceFolder.Name,
                SourceName = sourceFolder.Name
            };

            var lastModified = GetLastModifiedTime(sourceFolder);
            if (lastModified.HasValue)
            {
                discoverFolder.TimeLastModified = lastModified.Value;
            }

            return discoverFolder;
        }

        private DateTime? GetLastModifiedTime(IAveFolder sourceFolder)
        {
            if (sourceFolder?.Item != null)
            {
                if (TryGetDateTime(sourceFolder.Item["Modified"], out var modified))
                {
                    return modified;
                }

                if (sourceFolder.Item.FieldValues != null && sourceFolder.Item.FieldValues.TryGetValue("Modified", out var fieldModified) && TryGetDateTime(fieldModified, out modified))
                {
                    return modified;
                }
            }

            var properties = sourceFolder?.Properties;
            if (properties != null)
            {
                if (TryGetDateTimeFromProperties(properties, "TimeLastModified", out var modified))
                {
                    return modified;
                }

                if (TryGetDateTimeFromProperties(properties, "Modified", out modified))
                {
                    return modified;
                }
            }

            return null;
        }

        private bool TryGetDateTimeFromProperties(IDictionary properties, string key, out DateTime value)
        {
            value = default;
            if (properties == null || !properties.Contains(key))
            {
                return false;
            }

            return TryGetDateTime(properties[key], out value);
        }

        private bool TryGetDateTime(object rawValue, out DateTime value)
        {
            value = default;
            if (rawValue == null)
            {
                return false;
            }

            if (rawValue is DateTime dateTime)
            {
                value = dateTime;
                return true;
            }

            return DateTime.TryParse(rawValue.ToString(), out value);
        }
    }
}
