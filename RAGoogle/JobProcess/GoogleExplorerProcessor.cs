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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.Wrapper.Common;
using Google;
using Newtonsoft.Json;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Util;
using Util;
using static RAGoogle.Util.GoogleConstant;

namespace RAGoogle.JobProcess
{
    public class GoogleExplorerProcessor : BaseProcessor
    {
        protected IRALogger logger = RALogger.GetInstance(typeof(GoogleExplorerProcessor));
        private IUniqueIdSettingDao mUniqueIdSettingDao;
        private int mFinishCount = 0;

        public IUniqueIdSettingDao UniqueIdSettingDao
        {
            get
            {
                if (mUniqueIdSettingDao == null)
                {
                    mUniqueIdSettingDao = new UniqueIdSettingDao();
                }
                return mUniqueIdSettingDao;
            }
        }
        private ExplorerDao _explorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        private static string uniqueIdPrefix;

        public GoogleExplorerProcessor(string jobId) : base(jobId, JobType.GoogleDataSynchronization)
        {
            ReportCenter.InitCurrentJobInfo(jobId, JobType.GoogleDataSynchronization);
            uniqueIdPrefix = UniqueIdSettingDao.LoadingUniqueIdSetting()?.Prefix ?? string.Empty;
        }

        public override async Task RunNowAsync(RMGoogleSetting setting, GoogleDriveTreeNodeDto node)
        {
            using (var performance = new PerformanceScope("GoogleExplorerProcessor.RunNowAsync"))
            using (CheckJobStopScope stopScope = new CheckJobStopScope())
            {
                try
                {
                    if (node is null || setting is null)
                    {
                        logger.Error("Setting node or Node info are invalid.");
                        throw new ArgumentNullException("Setting node or Node info are invalid.");
                    }
                    if (setting.IsSyncData == false)
                    {
                        logger.Info("Setting node hasnt been enabled data sync, skipping this node...");
                        return;
                    }
                    WrapperConfiguration.JobDir = AveEnv.AgentTempFolder;
                    RecordManager.Config();
                    var lastTimeScan = ReportCenter.GetLastRunTime();
                    logger.Info($"Node {node.ID} last scan time is {lastTimeScan}");
                    if (RecordManager.IsLoadedCache(node))
                    {
                        RecordManager.LoadCache();
                    }
                    var itemQueue = new DataQueue<GoogleItemData>();

                    var task2 = RecordFeedingAsync(node, itemQueue, setting);
                    var task = ProcessItemDataAsync(itemQueue, node, setting);
                    await Task.WhenAll(task, task2);
                    RecordManager.Commit();

                    if (!setting.RunAutoFullJob || lastTimeScan > 0)
                    {
                        logger.Info($"Start process updating any changed rule of label.");
                        await ProcessRuleChangedItems(lastTimeScan, node.ID);
                        RecordManager.WaitComplete();
                    }
                    mFinishCount++;
                    if (!ReportCenter.IsLimitExceeded)
                    {
                        ReportCenter.StorageFailedItems();
                        ReportCenter.UpsertLastRunTime(scanTimeTicks);
                    }
                    lastTimeScan = ReportCenter.GetLastRunTime();
                    logger.Info($"Node {node.ID} next collection time is {lastTimeScan}");
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has been stopped.");
                    throw new JobStopException("This Job is stopped.");
                }
                catch (Exception ex)
                {
                    logger.Error("An error occur while running data sync job, Message: {0}", ex);
                    if (ex is GoogleApiException gex && (gex.HttpStatusCode == System.Net.HttpStatusCode.NotFound && gex.Message.Contains(node.ObjectId)))
                    {
                        throw new NotFoundDriveException(I18NEntity.GetString("RM_JM_JD_NotFound_Drive"));
                    }
                    throw;
                }
            }
        }
        private async Task RecordFeedingAsync(GoogleDriveTreeNodeDto node, DataQueue<GoogleItemData> itemQueue, RMGoogleSetting setting)
        {
            using (var performance = new PerformanceScope("RecordsDisposalProcessor.RecordFeedingAsync"))
            using (CheckJobStopScope jScope = new CheckJobStopScope())
            {
                try
                {
                    await ProcessDiscoveryItemsData(node, setting, itemQueue, true);
                }
                catch (JobStopException)
                {
                    logger.Warn("The records feeding job has been stopped.");
                    throw new JobStopException("The job has stopped."); ;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to record feeding, Message: {ex}");
                    throw;
                }
                finally
                {
                    logger.Info("The discover data has finished.");
                    itemQueue.Complete();
                }
            }
        }

        private async Task ProcessItemDataAsync(DataQueue<GoogleItemData> itemQueue, GoogleDriveTreeNodeDto selectedNode, RMGoogleSetting setting)
        {
            using (CheckJobStopScope jScope = new())
            {
                await itemQueue.ToIEnumerable().ParallelExecute(async item =>
                {
                    try
                    {
                        using (CheckJobStopScope subJScope = new CheckJobStopScope())
                        {
                            using (new PerformanceScope("GoogleExplorerProcessor:ProcessDataItemAsync"))
                            {
                                if (item == null) return;
                                logger.Info($"Process item id:{item.Id}");
                                //if (item.Name.StartsWith("AvePointTestGoogleFileAutomaticException"))
                                //{
                                //    throw new Exception("Automatic throw exception.");
                                //}
                                var isExist = RecordManager.TryGetRecordValue(item.UniqueId, 0, out var recordInDB);
                                if (item.IsDeleted)
                                {
                                    logger.Info($"Process item id:{item.Id}, is deleted.");
                                    if (isExist && recordInDB.RecordStatus == (int)RMRecordStatus.Active)
                                    {
                                        recordInDB.RecordStatus = (int)RMRecordStatus.RMDeleted;
                                        ExplorerDao.UpdateAll(r => r.ScopeId == recordInDB.ScopeId && r.NodeId == recordInDB.NodeId, r =>
                                        {
                                            r.CopyFrom(recordInDB);
                                        });
                                    }
                                    return;
                                }
                                var record = item.ConvertToRecord(selectedNode, recordInDB);
                                if (isExist && record.RecordStatus == (int)RMRecordStatus.RMDeleted)
                                {
                                    record.RecordStatus = (int)RMRecordStatus.Active;
                                }
                                record.MetaInfo = JsonConvert.SerializeObject(item.MetaInfo);
                                if (item.Level == RMNodeLevel.GoogleFile)
                                {
                                    //get label info & rule info
                                    if (item.LableIds.IsNotNullOrEmpty())
                                    {
                                        var labels = item.MetaInfo.Labels;
                                        var latestTerm = new RMTerm();
                                        foreach (var label in labels)
                                        {
                                            var rmTerm = TermDao.GetRMTermByLabelId(label.Id, selectedNode.TenantId);

                                            if (rmTerm != null)
                                            {
                                                if (labels.IndexOf(label) == 0)
                                                {
                                                    latestTerm = rmTerm;
                                                }
                                                record.TermId = rmTerm.UniqueId;
                                                record.TermName = rmTerm.Name;
                                                var oldRuleId = record.RuleId;
                                                if (await RuleManager.ApplyRuleInfo(record))
                                                {
                                                    var newRuleId = record.RuleId;
                                                    if (oldRuleId != newRuleId && newRuleId == Guid.Empty)
                                                    {
                                                        record.RemoveManualProperties();
                                                        RecordManager.UpdateManualProperties(record, true);
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                        logger.Info($"Process item id:{item.Id}, apply term name:{record.TermId}");
                                    }
                                    else if (record.TermName.IsNotNullOrEmpty() || !record.TermId.Equals(Guid.Empty))
                                    {
                                        record.TermId = Guid.Empty;
                                        record.TermName = string.Empty;
                                    }
                                }
                                RecordManager.Add(record);
                            }
                        }
                    }
                    catch (JobStopException)
                    {
                        logger.Warn("The sync content for search job has been stopped.");
                        throw new JobStopException("The job has stopped.");
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                        ReportCenter.RecordFailed(item.ConvertToJobDetail(JobDetailsStatus.Failed, appProfile.TenantId, JobAction.None.ToString(), ex.Message), item.GenerateFailureItemEntity(selectedNode.NodeId, ReportCenter.JobId));
                    }
                }, MaxDegreeOfParallelism, Cts.Token);
            }
        }

        //private async Task<bool> AppyRuleInfo(Record record, string termId, GoogleItemData itemInfo)
        //{
        //    var termRelatedRules = await RuleManager.GetAssociatedRuleAsync(termId);
        //    var result = false;
        //    if (termRelatedRules.IsNullOrEmpty())
        //    {
        //        record.RuleId = Guid.Empty;
        //        record.RuleLevel = (int)PolicyLevel.None;
        //        record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
        //        record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;

        //        logger.Warn($"The label [{termId}] is not related rules.");
        //    }
        //    else
        //    {
        //        (var rule, var time) = RuleManager.MatchedPotentialRule(itemInfo.ConvertToInfo(), termRelatedRules, true);
        //        if (rule != null)
        //        {
        //            record.RuleId = new Guid(rule.Id);
        //            record.RuleLevel = (int)rule.PolicyLevel;
        //            record.DisposalDueDate = time.Ticks;
        //            record.PreviosDisposalDueDate = time.Ticks;
        //            result = true;
        //        }
        //    }
        //    return result;
        //}

        #region label rule change handler
        private async Task ProcessRuleChangedItems(long latestSyncJobProcessTime, string scopeId)
        {
            try
            {
                using (CheckJobStopScope jScope = new CheckJobStopScope())
                {
                    var changedLabels = LabelManager.GetHasChangedLabelIds(latestSyncJobProcessTime);
                    logger.Info($"Changed term [{string.Join(", ", changedLabels.Keys)}] after [{latestSyncJobProcessTime}].");
                    if (changedLabels.Count == 0)
                    {
                        return;
                    }
                    var items = RecordManager.GetRecordsByLabelIds(changedLabels, scopeId);
                    foreach (var batchItems in items)
                    {
                        foreach (var item in batchItems)
                        {
                            using (CheckJobStopScope subJScope = new CheckJobStopScope())
                            {
                                await ProcessHasLabelRuleChangedItems(item);
                            }
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while processing has label changed files. Error: {e}");
            }
        }
        private async Task ProcessHasLabelRuleChangedItems(Record record)
        {
            using (new PerformanceScope("Google:DataSync:ProcessHasLabelRuleChangedItems", "", true))
            {
                try
                {
                    using (CheckJobStopScope jScope = new CheckJobStopScope())
                    {
                        var oldRuleId = record.RuleId;
                        await RuleManager.ApplyRuleInfo(record);
                        var newRuleId = record.RuleId;
                        if (oldRuleId != newRuleId && newRuleId == Guid.Empty)
                        {
                            record.RemoveManualProperties();
                            RecordManager.UpdateManualProperties(record, true);
                        }
                        record.ManualCollectionTime = DateTime.UtcNow.Ticks;

                        logger.Info($"The record [{record.Id}] is applied new rule [{record.RuleId}].");
                        RecordManager.Add(record);
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("the job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception e)
                {
                    logger.Error($"An error occurred while processing term rule changed file [{record.Id}] operation. Error: {e}");
                    ReportCenter.RecordFailed(record.GenerateSyncActionDetail(e.Message), 0);
                }
            }
        }
        #endregion
    }
}
