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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Common;
using RABox.Converters;
using RABox.DataWorker;
using RABox.Disposal;
using RABox.Extensions;
using RABox.RuleManagement;
using RABox.Util;
using Util;

namespace RABox
{
    public class RuleActionProcessor : DataProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(RuleActionProcessor));

        private readonly BoxManualManagement _boxManualManagement;

        public RuleActionProcessor() : base()
        {
            _boxManualManagement = new BoxManualManagement();
            JobType = JobType.BoxRecordsDisposal;
            FlagType = NodeFlagType.BoxDisposal;
        }


        public override async Task ProcessInnerAsync(BoxFolderProxy topFolder, RMBoxService boxService, BoxTreeNode topNode, string scopeId, bool isLastNode = false)
        {
            try
            {
                using (CheckJobStopScope subJScope = new CheckJobStopScope())
                {
                    _logger.Info($"Start running run enforece rule job for the selected node: [{topNode.Id}]");

                    RecordManager.Config();
                    ReportCenter.ConfigFor(scopeId, topNode.ConnectionId);
                    if (RecordManager.IsLoadedCache(topNode))
                    {
                        RecordManager.LoadRuleActionCache();
                    }
                    _boxManualManagement.Build(RecordManager, ReportCenter, JobId);

                    var recordQueue = new DataQueue<(Record, BoxSettingDto)>();

                    var task = Task.Run(() => ProcessDataItemAsync(topNode, boxService, recordQueue));

                    await ProcessFullScanAsync(topFolder, topNode, recordQueue);

                    _logger.Info($"All box folders are scanned completed.");

                    task.Wait();

                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                _logger.Error($"Error occurred while process disposal data. Error: {e}");
                throw;
            }
        }

        private async Task ProcessDataItemAsync(BoxTreeNode topNode, RMBoxService boxService, DataQueue<(Record, BoxSettingDto)> recordQueue)
        {
            try
            {
                await recordQueue.ToIEnumerable().ParallelExecute(async dataItem =>
                {
                    try
                    {
                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            var item = dataItem.Item1;
                            var settingInfo = dataItem.Item2;
                            using (new PerformanceScope("Box:BoxDisposal:ProcessDisposalItem", "", true))
                            {
                                await EnforceRuleActionRecord(item, settingInfo, topNode, boxService);
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
                        ReportCenter.RecordFailedCommon(dataItem.Item1.GenerateDisposalActionJobDetail(string.Empty, string.Empty, I18NResource.DeleteItemFailed), dataItem.Item1.NodeType);
                    }

                }, MaxDegreeOfParallelism, Cts.Token);
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task EnforceRuleActionRecord(Record record, BoxSettingDto settingInfo, BoxTreeNode scanNode, RMBoxService boxService)
        {
            if (!RuleManager.TryGetRule(record.RuleId, out Rule oldRule))
            {
                _logger.Warn($"No Box rules found for the ruleId: {record.RuleId}");
                ReportCenter.RecordSkipCommon(record.GenerateDisposalActionJobDetail(I18NEntity.GetString(I18NResource.RemoveAndDestroyAction), oldRule.Name, string.Format(I18NEntity.GetString(I18NResource.RuleIsNotAvailable), oldRule.Name)), record.NodeType);
                return;
            }

            if (record.HoldStatus && record.HoldReleaseTime > DateTime.UtcNow.Ticks)
            {
                _logger.Warn($"Item [{record.Id}] is RecordsHold.");
                ReportCenter.RecordSkipCommon(record.GenerateDisposalActionJobDetail(I18NEntity.GetString(I18NResource.RemoveAndDestroyAction), oldRule.Name, I18NResource.FileOnHold), record.NodeType);
                return;
            }

            BoxFileProxy lastestFileProxy;
            Record? lastestRecord = null;
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    _logger.Info($"Get the lastest information for record [{record.Id}-{record.ExternalId}]");
                    lastestFileProxy = new BoxFileProxy(boxService.GetUserContext(record.AveSiteId), record.ExternalId);
                    lastestRecord = lastestFileProxy.ConvertToRecord(record, scanNode);
                    if (lastestFileProxy.TrashedAt == null) // cannot use PathCollection for trashed item 
                    {
                        // manually build dirPath and Ancestor again
                        lastestRecord.DirPath = lastestRecord.DirPath.Split("\\")[0] + "\\";
                        var newAncestor = lastestRecord.Ancestors;
                        newAncestor.Reverse();
                        newAncestor = newAncestor.Take(3).ToList();
                        foreach (var folder in lastestFileProxy.PathCollection.Entries)
                        {
                            if (folder.Id != BoxUtility.BoxRootFolderId)
                            {
                                lastestRecord.DirPath += folder.Name + "\\";

                                newAncestor.Add(new BoxFolderProxy(boxService.GetUserContext(record.AveSiteId), folder).UniqueId);
                            }
                        }
                        newAncestor.Reverse();
                        lastestRecord.Ancestors = newAncestor;
                        lastestRecord.DirPath += lastestRecord.LeafName;
                    }
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals("BoxItemIsTrashed"))
                {
                    _logger.Error($"The item [{record.Id}] is trashed!");
                    // get the file from trash to process delete
                    lastestFileProxy = boxService.GetTrashedFile(record.ExternalId);
                }
                else if (ex.Message.Equals("BoxItemNotFound"))
                {
                    _logger.Error($"The item [{record.Id}] is not found!");
                    ReportCenter.RecordFailedCommon(record.GenerateDisposalActionJobDetail(string.Empty, string.Empty, I18NResource.ItemNotFound), record.NodeType);
                    return;
                }
                else
                {
                    throw;
                }
            }

            await ProcessDeleteItem(record, settingInfo, oldRule, lastestFileProxy, lastestRecord);
        }

        public async Task ProcessDeleteItem(Record record, BoxSettingDto settingInfo, Rule rule, BoxFileProxy lastestFileProxy, Record? newRecord = null)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var lastestRecord = newRecord ?? record;
                    if (WrapperConfiguration.IsProcessApprovalDatasOnly && !WrapperConfiguration.IsRecheckRule)
                    {
                        _logger.Info($"will not recheck rule,recordid [{lastestRecord.Id}] with rule [{rule.Id}]");
                    }
                    else if (newRecord != null)
                    {
                        Rule? newRule = RecalculateRule(record, lastestRecord);
                        if (newRule == null) return;

                        if (rule.Id != newRule.Id)
                        {
                            ReportCenter.RecordFailedCommon(record.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.UpdateNewRule), record.NodeType);
                            lastestRecord.RuleId = new Guid(newRule.Id);
                            if (record.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                            {
                                lastestRecord.RemoveManualProperties();
                                RecordManager.UpdateManualProperties(lastestRecord, true);
                            }
                            else
                            {
                                RecordManager.UpdateToNewRuleInfo(lastestRecord);
                            }
                            await ProcessDeleteItem(lastestRecord, settingInfo, newRule, lastestFileProxy);
                            return;
                        }

                    }
                    _logger.Info($"Done recalculate and start processing delete item [{lastestRecord.Id}] with rule [{rule.Id}]");
                    if (lastestRecord.DisposalDueDate > DateTime.UtcNow.Ticks)
                    {
                        _logger.Warn($"The item [{lastestRecord.Id}] has not reached action due date yet.");
                        ReportCenter.RecordSkipCommon(record.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NotYetDueDate), record.NodeType);
                        return;
                    }

                    if (!await _boxManualManagement.IsNeedProcessManualDisposalAsync(rule, settingInfo, lastestRecord))
                    {
                        DeleteBoxItemAndUpdateRecordAsync(rule, lastestRecord, lastestFileProxy);
                    }
                }

            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception)
            {
                throw;
            }
            return;
        }

        private Rule? RecalculateRule(Record record, Record currentRecord)
        {
            var itemInfo = currentRecord.ConvertBoxItemInfo();

            if (!RuleManager.TryGetTermRelatedRules(record.TermId.ToString(), out var termRelatedRules))
            {
                _logger.Warn($"The term [{record.TermId.ToString()}] is not related with rules.");
                return null;
            }

            var matchedRule = new BoxRuleManagement(termRelatedRules).MatchPotentialRule(itemInfo, true);

            if (matchedRule == null)
            {
                _logger.Warn($"The item [{record.Id}] does not match any rule.");

                ReportCenter.RecordSkipCommon(record.GenerateDisposalActionJobDetail(string.Empty, string.Empty, I18NResource.UpdateNoRule), record.NodeType);
                currentRecord.RuleId = Guid.Empty;

                if (record.ManualApprovedStatus != (int)SOApproveDBStatus.None)
                {
                    currentRecord.RemoveManualProperties();
                    RecordManager.UpdateManualProperties(currentRecord, true);
                }
                else
                {
                    RecordManager.UpdateToNewRuleInfo(currentRecord);
                }

                return null;
            }

            return matchedRule.Item1;
        }

        private void DeleteBoxItemAndUpdateRecordAsync(Rule rule, Record lastestRecord, BoxFileProxy lastestFileProxy)
        {
            try
            {
                using (CheckJobStopScope subJScope = new CheckJobStopScope())
                {
                    _logger.Info($"Delete BoxItem And Update Record [{lastestRecord.Id}]");
                    if (lastestFileProxy.DeleteFilePermanently())
                    {
                        RecordManager.UpdateRecordStatusAndDestroyedTime(lastestRecord, rule, 2);
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
                _logger.Error($"An error occurred while deleting item [{lastestRecord?.Id}]. Error: {e}");
                if (lastestRecord != null)
                {
                    if (e.Message.Equals("BoxItemLocked"))
                    {
                        ReportCenter.RecordFailedCommon(lastestRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.ItemIsLocked), lastestRecord.NodeType);
                    }
                    else if (e.Message.Equals("BoxItemNotReachRetentionExpiration"))
                    {
                        ReportCenter.RecordFailedCommon(lastestRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.NotReachRetentionExpiration), lastestRecord.NodeType);
                    }
                    else if (e.Message.Equals("BoxItemUnderLegalHold"))
                    {
                        ReportCenter.RecordFailedCommon(lastestRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.ItemUnderLegalHold), lastestRecord.NodeType);
                    }
                    else
                    {
                        ReportCenter.RecordFailedCommon(lastestRecord.GenerateDisposalActionJobDetail(string.Empty, rule.Name, I18NResource.UnexpectedException), lastestRecord.NodeType);
                    }
                }
            }
        }

        private async Task ProcessFullScanAsync(BoxFolderProxy topFolder, BoxTreeNode topNode, DataQueue<(Record, BoxSettingDto)> recordQueue)
        {
            var runningRunActionJobsScopeIds = ReportCenter.GetRunningJobsScopeId();
            var scanRecordQueue = new Queue<Record>();
            Record topRecord = null;

            if (topFolder.IsRootFolder)
            {
                topRecord = topFolder.ConvertToRecord(null, topNode);
            }
            else if (!RecordManager.TryGetRuleActionRecordValue(topFolder.UniqueId, Convert.ToInt32(new DateTime(topFolder.Created, DateTimeKind.Utc).ToString("yyyyMMdd")), out topRecord))
            {
                _logger.Error($"The current folder [{topFolder.UniqueId}] - [{topFolder.Id}] is running enforce rule action job but not have any record.");
                throw new Exception($"The current node [{topNode.FullPath}] does not have any record. Please sync the content before running enforce rule action");
            }

            scanRecordQueue.Enqueue(topRecord);

            while (scanRecordQueue.Count != 0)
            {
                var scanRecord = scanRecordQueue.Dequeue();
                try
                {
                    if ((scanRecord.ExternalId != topNode.RealId || (topNode.Id == BoxUtility.BoxRootFolderId && topNode.StartJobNodeLevel < RMNodeLevel.BoxUser)) &&
                        runningRunActionJobsScopeIds.Any(item => item == scanRecord.Id.ToString()))
                    {
                        _logger.Warn($"Current box folder record [{scanRecord.Id}] has running enforce rule action job. Skipped it.");
                        continue;
                    }

                    using (new PerformanceScope("Box:BoxDisposal:FolderScan", "", true))
                    {
                        if (scanRecord.ExternalId != topFolder.Id && SettingManager.TryGetScheduleInfo(scanRecord, out var scheduleInfo))
                        {
                            _logger.Info($"The box folder record [{scanRecord.Id}] has unique schedule configure].");
                            continue;
                        }

                        var settingInfo = await SettingManager.GetSettingInfoAsync(topNode, scanRecord);

                        var childRecords = RecordManager.GetRuleActionRecordsByParent(scanRecord.Id);
                        _logger.Info($"The box folder record [{scanRecord.Id}] sub item records count [{childRecords.Count}].");

                        foreach (var record in childRecords)
                        {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {

                                if (record.NodeType == (int)RMNodeLevel.BoxFolder)
                                {
                                    scanRecordQueue.Enqueue(record);
                                    continue;
                                }

                                if (record.NodeType == (int)RMNodeLevel.BoxFile)
                                {
                                    await recordQueue.WriteAsync((record, settingInfo));
                                }
                            }
                        }
                        _logger.Info($"The folder [{scanRecord.Id}] scan subitems succeed.");
                    }
                }
                catch (JobStopException)
                {
                    _logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    _logger.Error($"An error occurred while process folder [{scanRecord?.Id}]. Error: {e}");
                    if (scanRecord != null)
                    {
                        ReportCenter.RecordFailedCommon(scanRecord.GenerateDisposalActionJobDetail(string.Empty, string.Empty, I18NResource.UnexpectedException), scanRecord.NodeType);
                    }
                }

            }
            recordQueue.Complete();
        }
    }
}