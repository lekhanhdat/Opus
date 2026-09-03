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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using RABox.Converters;
using RABox.DataWorker;
using RABox.Extensions;
using RABox.RuleManagement;
using RABox.Util;
using RADataSynchronize.TermCheck.Model;
using Util;

namespace RABox
{
    public class DataSyncProcessor : DataProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(DataSyncProcessor));

        public DataSyncProcessor(JobType jobType) : base()
        {
            JobType = jobType;
            FlagType = NodeFlagType.BoxSync;

        }

        public override async Task ProcessInnerAsync(BoxFolderProxy topFolder, RMBoxService boxService, BoxTreeNode topNode, string scopeId, bool isLastNode = false)
        {
            try
            {
                _logger.Info($"Start running data sync job for the selected [{topNode.Level.ToString()}] node: [{topNode.Id}]");

                using (CheckJobStopScope stopScope = new CheckJobStopScope())
                {
                RecordManager.Config();
                ReportCenter.ConfigFor(scopeId, topNode.ConnectionId);

                if (RecordManager.IsLoadedCache(topNode))
                {
                    RecordManager.LoadCache();
                }

                (var latestSyncJobProcessTime, string lastStreamPosition) = ReportCenter.GetLastRunTime();

                _logger.Info($"Processing from latestSyncJobProcessTime:{latestSyncJobProcessTime} ,and lastStreamPosition:{lastStreamPosition}.");

                DataQueue<(BoxItemProxy, BoxSettingDto)> itemQueue = new DataQueue<(BoxItemProxy, BoxSettingDto)>();

                string nextStreamPosition = lastStreamPosition;

                var task = Task.Run(() => ProcessDataItemAsync(topNode, itemQueue));

                if (latestSyncJobProcessTime == 0)
                {
                    _logger.Info($"Start full scan.");

                    ProcessDeletedItems(boxService);

                    nextStreamPosition = await ProcessFullScanAsync(topFolder, boxService, topNode, itemQueue);

                    _logger.Info($"All box folders are scanned completed.");

                    task.Wait();
                }
                else if (DateTime.UtcNow.AddDays(-14).Ticks >= latestSyncJobProcessTime)
                {
                    _logger.Info($"Start icremental sync after the last sync 14 or more days.");

                    ProcessDeletedItems(boxService);

                    nextStreamPosition = await ProcessFullScanAsync(topFolder, boxService, topNode, itemQueue);

                    _logger.Info($"All box folders are scanned completed.");

                    task.Wait();
                }
                else
                {
                    _logger.Info($"Start icremental sync.");

                    DataQueue<(Record, BoxSettingDto)> recordQueue = new DataQueue<(Record, BoxSettingDto)>();

                    var task2 = Task.Run(() => ProcessDataItemAsync(recordQueue));

                    nextStreamPosition = await ProcessIncrementalAsync(topFolder, boxService, topNode, lastStreamPosition, itemQueue, recordQueue);

                    _logger.Info($"All box folders are scanned completed.");

                    task.Wait();

                    task2.Wait();
                }

                if (isLastNode)
                {
                    _logger.Info($"Start process updating any changed rule of term.");
                    RecordManager.Commit();

                    ProcessHasTermRuleChangedItems(latestSyncJobProcessTime);
                }

                RecordManager.WaitComplete();

                if (!ReportCenter.IsLimitExceeded && ReportCenter.StorageFailedItems())
                {
                    ReportCenter.UpsertLastRunTime(nextStreamPosition);
                }
            }

            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred while process sync data. Error: {e}");
                throw;
            }
        }

        private async Task<string> ProcessFullScanAsync(BoxFolderProxy topFolder, RMBoxService boxService, BoxTreeNode topNode, DataQueue<(BoxItemProxy, BoxSettingDto)> itemQueue)
        {
            var nextStreamPosition = boxService.InitStreamPosition();

            await ProcessFullContainerAsync(topNode, topFolder, itemQueue);

            return nextStreamPosition;
        }

        private async Task<string> ProcessIncrementalAsync(BoxFolderProxy topFolder, RMBoxService boxService, BoxTreeNode topNode,
            string lastStreamPosition, DataQueue<(BoxItemProxy, BoxSettingDto)> itemQueue, DataQueue<(Record, BoxSettingDto)> recordQueue)
        {
            (var trashedItem, var modifiedItems) = boxService.GetModifiedSubItems(topFolder, ref lastStreamPosition);

            ProcessFailedItemsAsync(topFolder, boxService, topNode, modifiedItems);

            if (trashedItem.Count != 0)
            {
                _logger.Info($"Start process deleting item for [{trashedItem.Count}] trashed items.");

                ProcessDeletedItems(trashedItem);
            }

            if (modifiedItems.Count != 0)
            {
                _logger.Info($"Start process upserting item for [{modifiedItems.Count}] modified properties items.");

                await ProcessModifiedPropertiesItemsAsync(topNode, modifiedItems.Values.ToList(), itemQueue);
            }

            _logger.Info($"Start process updating classification.");

            await ProcessModifiedSettingRecordsAsync(topFolder, topNode, trashedItem.Keys.ToList(), modifiedItems.Keys.ToList(), recordQueue);

            itemQueue.Complete();
            recordQueue.Complete();

            return lastStreamPosition;
        }

        private async Task ProcessDataItemAsync(BoxTreeNode topNode, DataQueue<(BoxItemProxy, BoxSettingDto)> itemQueue)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
            await itemQueue.ToIEnumerable().ParallelExecute(async dataItem =>
            {
                try
                {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {
                    var item = dataItem.Item1;
                    var settingInfo = dataItem.Item2;
                    using (new PerformanceScope("Box:DataSync:ProcessSyncItem", "", true))
                    {
                        ExecuteSyncItem(item, settingInfo, topNode);
                    }
                }
                        }
                        catch (JobStopException)
                        {
                            _logger.Warn("the job has stopped.");
                            throw new JobStopException("The job has stopped.");
                        }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process item [{dataItem.Item1.Id}]. Error: {e}");
                            ReportCenter.RecordFailedCommon(dataItem.Item1.GenerateSyncActionDetail(topNode, e.Message), dataItem.Item1.GenerateFailureItemEntity(topNode, JobId));
                }

            }, MaxDegreeOfParallelism, Cts.Token);
        }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred in ProcessDataItemAsync: {e}");
                throw;
            }
        }

        private async Task ProcessDataItemAsync(DataQueue<(Record, BoxSettingDto)> recordQueue)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
            await recordQueue.ToIEnumerable().ParallelExecute(async dataRecord =>
            {
                try
                {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {
                    var record = dataRecord.Item1;
                    var settingInfo = dataRecord.Item2;
                    using (new PerformanceScope("Box:DataSync:ProcessSyncRecord", "", true))
                    {
                        ExecuteSyncItem(record, settingInfo);
                    }
                }
                        }
                        catch (JobStopException)
                        {
                            _logger.Warn("the job has stopped.");
                            throw new JobStopException("The job has stopped.");
                        }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process record [{dataRecord.Item1.Id}]. Error: {e}");
                            ReportCenter.RecordFailedCommon(dataRecord.Item1.GenerateSyncActionDetail(e.Message), dataRecord.Item1.GenerateFailureItemEntity(JobId));
                }
            }, MaxDegreeOfParallelism, Cts.Token);
        }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred in ProcessDataItemAsync: {e}");
                throw;
            }
        }

        private async Task ProcessFullContainerAsync(BoxTreeNode topNode, BoxFolderProxy topFolder, DataQueue<(BoxItemProxy, BoxSettingDto)> itemQueue)
        {
            var runningSyncJobsScopeIds = ReportCenter.GetRunningJobsScopeId();
            var folderProxyQueue = new Queue<BoxFolderProxy>();
            folderProxyQueue.Enqueue(topFolder);
            while (folderProxyQueue.Any())
            {
                var scanFolder = folderProxyQueue.Dequeue();
                try
                {
                    if (scanFolder.Id != topNode.RealId && runningSyncJobsScopeIds.Any(item => item == scanFolder.UniqueId.ToString()))
                    {

                        _logger.Warn($"Current box folder [{scanFolder.Id}] has running data synchronisation job. Skipped it.");
                        continue;
                    }

                    using (new PerformanceScope("Box:DataSync:FolderScan", "", true))
                    {
                        var settingInfo = await SettingManager.GetSettingInfoAsync(topNode, scanFolder);

                        var itemCount = scanFolder.GetSubItemsCount();
                        _logger.Info($"The box folder [{scanFolder.Id}] sub items count [{itemCount}].");
                        var items = scanFolder.GetSubItems();
                        foreach (var item in items)
                        {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {

                                if (item.Type == BoxType.folder.ToString() && item is BoxFolderProxy folderProxy)
                                {
                                    folderProxyQueue.Enqueue(folderProxy);
                                    continue;
                                }
                                else if (item.Type == BoxType.file.ToString())
                                {
                                    await itemQueue.WriteAsync((item, settingInfo));
                                }
                            }
                        }
                        _logger.Info($"The folder [{scanFolder.Id}] scan subitems succeed.");
                        if (!scanFolder.IsRootFolder)
                        {
                            await itemQueue.WriteAsync((scanFolder, settingInfo));
                        }
                        await SettingManager.ResetSettingInfoAsync(topNode, scanFolder);
                    }
                }
                catch (JobStopException)
                {
                    _logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process folder [{scanFolder.Id}]. Error: {e}");
                    ReportCenter.RecordFailedCommon(scanFolder.GenerateSyncActionDetail(topNode, e.Message), scanFolder.GenerateFailureItemEntity(topNode, JobId));
                }

            }
            itemQueue.Complete();
        }

        private async Task ProcessModifiedSettingRecordsAsync(BoxFolderProxy topFolder, BoxTreeNode topNode, List<Guid> trashedItemIds, List<Guid> modifiedItemIds, DataQueue<(Record, BoxSettingDto)> recordQueue)
        {
            var runningSyncJobsScopeIds = ReportCenter.GetRunningJobsScopeId();
            var folderRecordQueue = new Queue<Record>();

            var isExist = RecordManager.TryGetRecordValue(topFolder.UniqueId, Convert.ToInt32(new DateTime(topFolder.Created, DateTimeKind.Utc).ToString("yyyyMMdd")), out Record scanRecord);

            if (topFolder.IsRootFolder)
            {
                scanRecord = topFolder.ConvertToRecord(null, topNode);
            }
            else if (!isExist)
            {
                // Incremental sync but its record not exist so it was deleted in the last sync job and restored for now.
                _logger.Info($"The box folder [{topFolder.Id}] is restored and should be already process in [ProcessModifiedPropertiesItemsAsync].");
                return;
            }

            folderRecordQueue.Enqueue(scanRecord);

            while (folderRecordQueue.Count != 0)
            {
                var scanFolderRecord = folderRecordQueue.Dequeue();
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                    if (scanFolderRecord.ExternalId != topNode.RealId && runningSyncJobsScopeIds.Any(item => item == scanFolderRecord.Id.ToString()))
                    {
                        _logger.Warn($"Current box folder record [{scanFolderRecord.ExternalId}] has running data synchronisation job. Skipped it.");
                        continue;
                    }

                    using (new PerformanceScope("Box:DataSync:FolderScan", "", true))
                    {
                        var settingInfo = await SettingManager.GetSettingInfoAsync(topNode, scanFolderRecord);

                        var childRecords = RecordManager.GetRecordsByParentId(scanFolderRecord.Id);
                        _logger.Info($"The box folder record [{scanFolderRecord.Id}] sub item records count [{childRecords.Count}].");

                        foreach (var record in childRecords)
                        {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {
                                if (record.NodeType == (int)RMNodeLevel.BoxFolder)
                                {
                                    folderRecordQueue.Enqueue(record);
                                    continue;
                                }

                                if (record.NodeType == (int)RMNodeLevel.BoxFile && !trashedItemIds.Contains(record.Id) && !modifiedItemIds.Contains(record.Id)
                                    && NeedProcessSetting(settingInfo))
                                {
                                    await recordQueue.WriteAsync((record, settingInfo));
                                }
                            }
                        }

                        _logger.Info($"The folder record [{scanFolderRecord.Id}] scan sub item records succeed.");


                        if (scanFolderRecord.ExternalId != BoxUtility.BoxRootFolderId && !trashedItemIds.Contains(scanFolderRecord.Id)
                            && !modifiedItemIds.Contains(scanFolderRecord.Id) && NeedProcessSetting(settingInfo))
                        {
                            await recordQueue.WriteAsync((scanFolderRecord, settingInfo));
                        }

                        await SettingManager.ResetSettingInfoAsync(topNode, scanFolderRecord);
                    }
                }
                }
                catch (JobStopException)
                {
                    _logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process folder [{scanFolderRecord.ExternalId}]. Error: {e}");
                    ReportCenter.RecordFailedCommon(scanFolderRecord.GenerateSyncActionDetail(e.Message), scanFolderRecord.GenerateFailureItemEntity(JobId));
                }
            }
        }


        private void ProcessFailedItemsAsync(BoxFolderProxy topFolder, RMBoxService boxService, BoxTreeNode topNode, Dictionary<Guid, BoxItemProxy> modifiedItems)
        {
            var failureItemQueue = new Queue<SyncFailureItemEntity>();

            if (topFolder.IsRootFolder)
            {
                _logger.Info($"Top folder is root folder. Retrieve all the failed items under root folder.");
                var rootFolderFaiedItems = ReportCenter.GetFailedItems(topNode.ConnectionId, topNode.OwnerId, topFolder.UniqueId.ToString());
                foreach (var item in rootFolderFaiedItems)
                {
                    failureItemQueue.Enqueue(item);
                }
            }
            else if (ReportCenter.TryGetFailedItem(topFolder.UniqueId.ToString(), out var scanFailedItem))
            {
                if (scanFailedItem != null)
                {
                    _logger.Info($"Retrieve the top failed item.");
                    failureItemQueue.Enqueue(scanFailedItem);
                }
            }
            while (failureItemQueue.Count != 0)
            {
                var failedItem = failureItemQueue.Dequeue();
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        if (modifiedItems.ContainsKey(new Guid(failedItem.RowKey)))
                        {
                            _logger.Info($"Current failed item [{failedItem.RowKey}] already in the modified item list. Skipped it.");
                            continue;
                        }

                        BoxItemProxy currentFailedItem = null;

                    if (failedItem.IsDirectory)
                    {
                        var failedItems = ReportCenter.GetFailedItems(topNode.ConnectionId, topNode.OwnerId, failedItem.RowKey);
                        foreach (var item in failedItems)
                        {
                            failureItemQueue.Enqueue(item);
                        }

                            currentFailedItem = new BoxFolderProxy(boxService.GetUserContext(topNode.OwnerId), failedItem.NodeId);
                    }
                        else
                    {
                            currentFailedItem = new BoxFileProxy(boxService.GetUserContext(topNode.OwnerId), failedItem.NodeId);
                    }

                        if (currentFailedItem != null)
                        {
                            _logger.Info($"Add failed item [{failedItem.RowKey}] - [{failedItem.NodeId}] into modified items list to process it later");
                            modifiedItems[currentFailedItem.UniqueId] = currentFailedItem;
                        }
                }
                }
                catch (JobStopException)
                {
                    _logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process failed item [{failedItem.RowKey}] - [{failedItem.NodeId}]. Error: {e}");
                    ReportCenter.RecordFailedCommon(failedItem, e.Message);
                }
            }
        }

        private async Task ProcessModifiedPropertiesItemsAsync(BoxTreeNode topNode, List<BoxItemProxy> modifiedItems, DataQueue<(BoxItemProxy, BoxSettingDto)> itemQueue)
        {
            var runningSyncJobsScopeIds = ReportCenter.GetRunningJobsScopeId();

            var modifiedItemIds = new HashSet<string>(modifiedItems.Select(item => item.Id));
            var modifiedItemsList = modifiedItems
                .Where(item => item.PathCollection.TotalCount > 0 &&
                               item.PathCollection.Entries.All(parent => !modifiedItemIds.Contains(parent.Id)))
                .ToList();

            foreach (var modifiedItem in modifiedItemsList)
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                    if (modifiedItem.Id != topNode.RealId && runningSyncJobsScopeIds.Any(item => item == modifiedItem.UniqueId.ToString()))
                    {

                        _logger.Warn($"Current box folder [{modifiedItem.Id}] has running data synchronisation job. Skipped it.");
                        continue;
                    }

                    using (new PerformanceScope("Box:DataSync:FolderScan", "", true))
                    {
                        if (modifiedItem.Type == BoxType.folder.ToString() && modifiedItem is BoxFolderProxy modifiedFolder && !modifiedFolder.IsRootFolder)
                        {
                            var folderProxyQueue = new Queue<BoxFolderProxy>();
                            folderProxyQueue.Enqueue(modifiedFolder);

                            while (folderProxyQueue.Count != 0)
                            {
                                    using (CheckJobStopScope subJScope = new CheckJobStopScope())
                                    {
                                var scanFolder = folderProxyQueue.Dequeue();
                                var settingInfo = await SettingManager.GetSettingInfoAsync(topNode, scanFolder);
                                var itemCount = scanFolder.GetSubItemsCount();
                                _logger.Info($"The box folder [{scanFolder.Id}] sub items count [{itemCount}].");
                                var items = scanFolder.GetSubItems();
                                foreach (var item in items)
                                {
                                            using (CheckJobStopScope sJScope = new CheckJobStopScope())
                                            {
                                    if (item.Type == BoxType.folder.ToString() && item is BoxFolderProxy folderProxy)
                                    {
                                        folderProxyQueue.Enqueue(folderProxy);
                                        continue;
                                    }
                                    else if (item.Type == BoxType.file.ToString())
                                    {
                                        await itemQueue.WriteAsync((item, settingInfo));
                                    }
                                            }

                                }
                                _logger.Info($"The folder [{modifiedFolder.Id}] scan sub modified items succeed.");
                                if (!scanFolder.IsRootFolder)
                                {
                                    await itemQueue.WriteAsync((scanFolder, settingInfo));
                                }
                                await SettingManager.ResetSettingInfoAsync(topNode, scanFolder);
                            }

                                }
                        }
                        else if (modifiedItem.Type == BoxType.file.ToString())
                        {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {
                                var parentFolder = modifiedItem.Parent;

                                var settingInfo = await SettingManager.GetSettingInfoAsync(topNode, parentFolder);

                                await itemQueue.WriteAsync((modifiedItem, settingInfo));
                            }
                            }
                        }
                    }
                }
                catch (JobStopException)
                {
                    _logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process modified item [{modifiedItem.Id}]. Error: {e}");
                    ReportCenter.RecordFailedCommon(modifiedItem.GenerateSyncActionDetail(topNode, e.Message), modifiedItem.GenerateFailureItemEntity(topNode, JobId));
                }
            }
        }

        private void ExecuteSyncItem(BoxItemProxy item, BoxSettingDto settingInfo, BoxTreeNode scanNode)
        {
            var isExist = RecordManager.TryGetRecordValue(item.UniqueId, Convert.ToInt32(new DateTime(item.Created, DateTimeKind.Utc).ToString("yyyyMMdd")), out var existItem);
            var isBoxItemPropertiesChanged = isExist && existItem.CheckBoxItemPropertiesChanged(item, scanNode);
            var isForceUpdate = isExist && DateTime.UtcNow.AddDays(-14).Ticks >= existItem.CollectTime;

            var record = item.ConvertToRecord(existItem, scanNode);
            if (isExist
                && (existItem.TermId != Guid.Empty || existItem.NodeType == (int)RMNodeLevel.BoxFolder)
                && (
                settingInfo.DeployTermMethod == DeployTermMethod.NoDefaultTerm ||
                (settingInfo.DeployTermMethod == DeployTermMethod.UseAutoClassification &&
                settingInfo.AutoJobOption != AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override) ||
                (settingInfo.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                settingInfo.ApplyExistType != (int)ApplyExistingTermType.OverWrite)))
            {
                if (isBoxItemPropertiesChanged || isForceUpdate)
                {
                    if (record.NodeType == (int)RMNodeLevel.BoxFile)
                    {
                        var oldRuleId = existItem.RuleId;
                        AppyRuleInfo(record, record.TermId.ToString());
                        var newRuleId = record.RuleId;
                        if (oldRuleId != newRuleId && newRuleId == Guid.Empty)
                        {
                            record.RemoveManualProperties();
                            RecordManager.UpdateManualProperties(record);
                        }
                    }
                    RecordManager.Add(record);
                    _logger.Info($"The item [{item.Id}] {(isBoxItemPropertiesChanged ? "properties are modified" : "is force update")}.");
                    return;
                }

                _logger.Warn($"The item [{item.Id}] keeps the current setting.");
                return;
            }

            // check if the item existed and the current job is running on the parent levels for the first time when choosing Auto-populate and Override but not run auto full job
            if (isExist && (existItem.TermId != Guid.Empty || existItem.NodeType == (int)RMNodeLevel.BoxFolder) &&
                settingInfo.DeployTermMethod == DeployTermMethod.UseAutoClassification && settingInfo.AutoJobOption == AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override &&
                !settingInfo.RunAutoFullJob && !isBoxItemPropertiesChanged)
            {
                _logger.Info($"The item [{item.Id}] keeps the current setting.");
                return;
            }

            if (record.NodeType == (int)RMNodeLevel.BoxFile)
            {
                var termInfo = TermManager.GetMatchedTermInfo(item, null, settingInfo, scanNode);
                if (termInfo.IsManually)
                {
                    _logger.Warn($"The item [{item.Id}] is used manually setting.");
                    RecordManager.Add(record);
                    return;
                }

                ApplyTermInfo(record, termInfo);

                var oldRuleId = isExist ? existItem.RuleId : Guid.Empty;
                AppyRuleInfo(record, record.TermId.ToString());
                var newRuleId = record.RuleId;
                if (isExist && oldRuleId != newRuleId && newRuleId == Guid.Empty)
                {
                    record.RemoveManualProperties();
                    RecordManager.UpdateManualProperties(record);
                }
            }

            _logger.Warn($"The item [{item.Id}] is applied new setting.");
            RecordManager.Add(record);
        }

        private void ExecuteSyncItem(Record record, BoxSettingDto settingInfo)
        {
            if ((record.TermId != Guid.Empty)
                &&
                (
                settingInfo.DeployTermMethod == DeployTermMethod.NoDefaultTerm ||
                (settingInfo.DeployTermMethod == DeployTermMethod.UseAutoClassification &&
                settingInfo.AutoJobOption != AvePoint.RA.Contract.TaxonomyModel.AutoJobOption.Override) ||
                (settingInfo.DeployTermMethod == DeployTermMethod.UseDefaultTerm &&
                settingInfo.ApplyExistType != (int)ApplyExistingTermType.OverWrite)))
            {
                _logger.Warn($"The record [{record.Id}] keeps the current setting.");
                return;
            }

            if (record.NodeType == (int)RMNodeLevel.BoxFile)
            {
                var termInfo = TermManager.GetMatchedTermInfo(null, record, settingInfo);
                if (termInfo.IsManually)
                {
                    _logger.Warn($"The record [{record.Id}] is used manually setting.");
                    RecordManager.Add(record);
                    return;
                }

                ApplyTermInfo(record, termInfo);

                var oldRuleId = record.RuleId;
                AppyRuleInfo(record, record.TermId.ToString());
                var newRuleId = record.RuleId;
                if (oldRuleId != newRuleId && newRuleId == Guid.Empty)
                {
                    record.RemoveManualProperties();
                    RecordManager.UpdateManualProperties(record);
                }
            }

            _logger.Warn($"The record [{record.Id}] is applied new setting.");
            RecordManager.Add(record);
        }

        private void ProcessHasTermRuleChangedItems(long latestSyncJobProcessTime)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                var changedTerms = TermManager.GetHasChangedTermIds(latestSyncJobProcessTime);
                _logger.Info($"Changed term [{string.Join(", ", changedTerms.Keys)}] after [{latestSyncJobProcessTime}].");
                if (changedTerms.Count == 0)
                {
                    return;
                }

                var items = RecordManager.GetRecordsByTermIds(changedTerms, JobStartTime);

                foreach (var batchItems in items)
                {
                    foreach (var item in batchItems)
                    {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            ProcessHasTermRuleChangedFile(item);
                        }
                    }
                }
            }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while processing has term changed files. Error: {e}");
            }
        }

        private void ProcessHasTermRuleChangedFile(Record record)
        {
            using (new PerformanceScope("Box:DataSync:ProcessHasTermRuleChangedFile", "", true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var oldRuleId = record.RuleId;
                    AppyRuleInfo(record, record.TermId.ToString());
                        var newRuleId = record.RuleId;
                        if (oldRuleId != newRuleId && newRuleId == Guid.Empty)
                        {
                            record.RemoveManualProperties();
                            RecordManager.UpdateManualProperties(record);
                        }
                    record.ManualCollectionTime = DateTime.UtcNow.Ticks;

                    _logger.Info($"The record [{record.Id}] is applied new rule [{record.RuleId}].");
                    RecordManager.Add(record);
                }
                }
                catch (JobStopException)
                {
                    _logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while processing term rule changed file [{record.Id}] operation. Error: {e}");
                    ReportCenter.RecordFailedCommon(record.GenerateSyncActionDetail(e.Message), record.GenerateFailureItemEntity(JobId));
                }
            }
        }

        private void ProcessDeletedItems(RMBoxService boxService)
        {
            _logger.Info($"Process deleted items.");
            try
            {
                List<BoxItemProxy> trashedItems = boxService.GetTrashedItems();

                foreach (var trashedItem in trashedItems)
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                    if (RecordManager.TryGetRecordByAncestor(trashedItem.UniqueId, Convert.ToInt32(new DateTime(trashedItem.Created, DateTimeKind.Utc).ToString("yyyyMMdd")), out Record? value))
                    {
                        if (trashedItem.Type == BoxType.folder.ToString())
                        {
                                _logger.Info($"Folder [{value.Id}] is deleted. Process delete its record and its child records");
                            Queue<Record> trashedRecords = new();

                            trashedRecords.Enqueue(value);
                            while (trashedRecords.Count > 0)
                            {
                                var trashedRecord = trashedRecords.Dequeue();

                                var childTrashedItems = RecordManager.GetRecordsByParentId(trashedRecord.Id);

                                foreach (var item in childTrashedItems)
                                {
                                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                                        {
                                    if (item.NodeType == (int)RMNodeLevel.BoxFolder)
                                    {
                                        trashedRecords.Enqueue(item);
                                        continue;
                                    }

                                    if (RecordManager.TryGetRecordValue(item.Id, item.CreateDate, out Record? childRecord))
                                    {
                                        RecordManager.Delete(childRecord);
                                    }
                                }
                                    }

                                if (RecordManager.TryGetRecordValue(trashedRecord.Id, trashedRecord.CreateDate, out Record? scanRecord))
                                {
                                    RecordManager.Delete(scanRecord);
                                }
                            }
                        }
                        else if (trashedItem.Type == BoxType.file.ToString())
                        {
                                _logger.Info($"File [{value.Id}] is deleted. Process delete its record");
                            RecordManager.Delete(value);
                        }
                    }
                }
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while processing delete item. Error: {e}");
                throw;
            }
        }

        private void ProcessDeletedItems(Dictionary<Guid, BoxItemProxy> trashedItems)
        {
            try
            {
                foreach (var trashedItem in trashedItems)
                {
                    using (CheckJobStopScope subJScope = new CheckJobStopScope())
                    {
                    if (RecordManager.TryGetRecordValue(trashedItem.Key, Convert.ToInt32(new DateTime(trashedItem.Value.Created, DateTimeKind.Utc).ToString("yyyyMMdd")), out Record? trashRecord))
                    {
                        RecordManager.Delete(trashRecord!);
                    }
                }
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while delete record. Error: {e}");
            }
        }

        private void ApplyTermInfo(Record record, TermInfo termInfo)
        {
            if (termInfo.TermIsDeprecated || termInfo.TermIsRemoved)
            {
                throw new Exception(I18NResource.TermIsInvalid + I18NEntity.Separator + termInfo.TermName);
            }

            if (record.NodeType == (int)RMNodeLevel.BoxFile)
            {
                record.TermId = new Guid(termInfo.TermId);
                record.TermName = termInfo.TermName;
            }
        }

        private void AppyRuleInfo(Record record, string termId)
        {
            var itemInfo = record.ConvertBoxItemInfo();

            if (!RuleManager.TryGetTermRelatedRules(termId, out var termRelatedRules))
            {
                record.RuleId = Guid.Empty;
                record.RuleLevel = (int)PolicyLevel.None;
                record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
                record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;

                _logger.Warn($"The term [{termId}] is not related rules.");

            }
            else
            {
                new BoxRuleManagement(termRelatedRules).ApplyRuleInfo(itemInfo, record);
            }
        }

        private bool NeedProcessSetting(BoxSettingDto settingInfo)
        {
            return (settingInfo.DeployTermMethod == DeployTermMethod.UseDefaultTerm && settingInfo.NeedCheckDefaultValue) ||
                   (settingInfo.DeployTermMethod == DeployTermMethod.UseAutoClassification && settingInfo.RunAutoFullJob);
        }
    }
}