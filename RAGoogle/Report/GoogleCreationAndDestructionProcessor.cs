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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.SystemSetting;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using Newtonsoft.Json;
using RAArchiverCommon.DestructionCache;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Models.Contract;
using RAGoogle.Util;
using Util;

namespace RAGoogle.Report
{
    public class GoogleCreationAndDestructionProcessor : BaseReportProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleCreationAndDestructionProcessor));

        private readonly bool SelectCreated;
        private readonly bool SelectDestroyed;
        private readonly DateTime startUtcTime;
        private readonly DateTime endUtcTime;
        private Dictionary<int, RMAccount> cacheAllUsers;
        private IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        public GoogleCreationAndDestructionProcessor(RMCreationJobMessage msg) : base(msg.JobID, msg.ProfileId)
        {
            jobType = msg.JobType;
            var globalTimeZone = GeneralSettingConfig.FindSystemTimeZoneById(msg.GlobalTimeZoneId.Replace("_", " "));
            startUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.StartTime, globalTimeZone);
            endUtcTime = TimeZoneInfo.ConvertTimeToUtc(msg.EndTime.AddDays(1), globalTimeZone);
            SelectCreated = msg.SelectCreated;
            SelectDestroyed = msg.SelectDestroyed;
            cacheAllUsers = AccountDao.FindAll().ToDictionary(key => key.Id, value => value);
        }

        protected override void InitializeReport()
        {
            LabelManager.LoadTerms();
            RuleManager.InitRulesInfoAsync().Wait();
        }
        protected override void RunNowAsync(GoogleDriveTreeNodeDto treeNode)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    if (SelectCreated)
                    {
                        BuildCreatedReport(treeNode);
                    }
                    if (SelectDestroyed)
                    {
                        LoadDestructionCache(treeNode.ID);
                        BuildDestroyedReportAsync(treeNode);
                        ClearDestructionCache(treeNode.ID);
                    }
                }
            }
            catch (JobStopException)
            {
                logger.Warn("This Job is stopped.");
                ReportCenter.SetJobFinish(JobStatus.Stopped);
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while running job. ", e.ToString());
                throw;
            }
        }
        private bool IsMatchOnCreateTime(GoogleItemData file)
        {
            bool result = false;
            if (file != null && file.CreatedTime.Ticks > startUtcTime.Ticks && file.CreatedTime.Ticks < endUtcTime.Ticks)
            {
                result = true;
            }
            return result;
        }

        private CreateAndDestroyedFileReport GenerateDestroyedReport(GoogleDestructionData data)
        {
            GoogleDestructionMetaData? metadata = JsonConvert.DeserializeObject<GoogleDestructionMetaData>(data.MetaInfo);
            var report = new CreateAndDestroyedFileReport();

            report.TermName = metadata?.TermName;
            report.Title = data.ItemName;
            report.LevelStr = data.Level;
            report.Url = data.FullPath;
            report.CreatedTime = metadata?.CreatedTime ?? 0;
            report.LastModifiedTime = data.DestroyedTime;
            report.FileType = metadata?.ItemExtension;
            report.ApprovalStatus = metadata.ManualApprovedStatus;
            report.InternalApprovedStatus = metadata.ManualInternalApprovedStatus;
            if (cacheAllUsers.TryGetValue(metadata?.ManualApprovedBy ?? 0, out RMAccount approveUser) && metadata?.ManualApprovedStatus != (int)AvePoint.RA.Contract.SOApproveDBStatus.Rejected)
            {
                report.ApprovedBy = approveUser.DisplayName;
                report.ApprovedByUPN = approveUser.UserPrincipalName;
            }
            //recordID
            report.DisposalClass = RuleManager.TryGetRuleInfo(new Guid(data.RuleId), out var ruleInfo) ? ruleInfo.DisposalClass : null;
            report.RuleName = ruleInfo?.RuleName ?? string.Empty;
            report.TermName = metadata?.TermName;
            report.OperationTime = data.DestroyedTime.Equals(DateTime.MinValue) ? string.Empty : data.DestroyedTime.ToString();
            report.OperationBy = I18NEntity.GetString("RM_RC_TimeFrame_ArchiverByRASystem");
            report.Operation = (int)OperationType.Destroyed;

            return report;
        }

        protected override async Task ProcessDriveAsync(GoogleDriveTreeNodeDto treeNode, DataQueue<GoogleItemData> itemQueue)
        {
            logger.Info($"Start processing node [{treeNode.ID}-{treeNode.Name}].");
            using (var performance = new PerformanceScope("GoogleCreationAndDestructionProcessor:ProcessDriveAsync"))
            using (CheckJobStopScope subJScope = new CheckJobStopScope())
            {
                try
                {
                    if (treeNode.Level == NodeLevel.GoogleMyDrive || treeNode.Level == NodeLevel.GoogleSharedDrive)
                    {
                        await ProcessScanTimeRangeDriveAsync(treeNode, itemQueue, default, default);
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The creation and destruction report job has been stopped.");
                    throw new JobStopException("The job has stopped."); ;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to process  creation and destruction report job, Message: {ex}");
                    throw;
                }
            }
        }

        protected override void ProcessFileReport(GoogleItemData file)
        {
            try
            {
                if (IsMatchOnCreateTime(file))
                {
                    ReportCenter.SendReport(GenerateCreatedReport(file), file.GenerateCreateAndDestroyedReportJobDetail());
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Process Google creation and destruction has error:{ex}");
                ReportCenter.RecordFailed(file.GenerateCreateAndDestroyedReportJobDetail(ex.Message));
            }
        }

        private void BuildCreatedReport(GoogleDriveTreeNodeDto treeNode)
        {
            try
            {
                using (new CheckJobStopScope())
                {
                    var itemQueue = new DataQueue<GoogleItemData>();
                    var task = Task.Run(() => ProcessItemDataAsync(itemQueue));
                    ProcessDriveAsync(treeNode, itemQueue).Wait();
                    itemQueue.Complete();
                    task.Wait();
                }
            }
            catch (AggregateException ae)
            {
                if (ae.InnerExceptions != null)
                {
                    foreach (var ex in ae.InnerExceptions)
                    {
                        if (ex is JobStopException)
                        {
                            logger.Warn("The job has stopped.");
                            throw new JobStopException("The job has stopped.");
                        }
                        else
                        {
                            logger.Error("error message: {1}.", ex.ToString());
                        }
                    }
                }
                throw;
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while running job. ", e.ToString());
                throw;
            }
        }

        private void BuildDestroyedReportAsync(GoogleDriveTreeNodeDto treeNode)
        {
            using (new CheckJobStopScope())
            {
                var destroyedItemsInLiteDB = GetEntitiesFromLiteDB(treeNode.ID);
                foreach (var destroyedItem in destroyedItemsInLiteDB)
                {
                    try
                    {
                        using (new CheckJobStopScope())
                        {
                            ReportCenter.SendReport(GenerateDestroyedReport(destroyedItem), destroyedItem.GenerateCreateAndDestroyedReportJobDetail());
                        }
                    }
                    catch (JobStopException)
                    {
                        logger.Warn("The job has stopped.");
                        throw new JobStopException("The job has stopped.");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Process Google creation and destruction has error:{ex}");
                        ReportCenter.RecordFailed(destroyedItem.GenerateCreateAndDestroyedReportJobDetail(ex.Message));
                    }
                }
            }

        }
        private List<GoogleDestructionData> GetEntitiesFromLiteDB(string nodeId)
        {
            List<GoogleDestructionData> entities = new List<GoogleDestructionData>();
            var LiteDBWrapper = GoogleLiteDBWrapper.CreateInstance(GetLiteDBPath(nodeId));
            int index = 0;
            int pageSize = 1000;
            bool hasMore = true;
            List<GoogleDestructionData> items = null;
            GoogleDestructionData destructionData = new GoogleDestructionData();
            do
            {
                using (new PerformanceScope("GoogleCreationAndDestructionProcessor.QueryAllByScopeIdAndPage", addToStatistics: true))
                {
                    items = LiteDBWrapper.QueryAllByScopeIdAndPage(index, pageSize, nodeId);
                }
                if (items != null && items.Count > 0)
                {
                    index++;
                    hasMore = true;
                    items = items.Where(r => r.DestroyedTime >= startUtcTime.Ticks && r.DestroyedTime <= endUtcTime.Ticks).ToList();
                    if (items.Count > 0)
                    {
                        entities.AddRange(items);
                    }
                }
                else
                {
                    hasMore = false;
                }
            } while (hasMore);

            return entities;
        }
        private CreateAndDestroyedFileReport GenerateCreatedReport(GoogleItemData file)
        {
            var report = new CreateAndDestroyedFileReport();
            try
            {
                var itemInfo = file.ConvertToInfo();
                Tuple<Rule, TimeSpan>? matchedRule = null;
                int matchedTermId = -1;
                Dictionary<int, List<Rule>>? associatedRules = null;
                foreach (var label in file.MetaInfo.Labels)
                {
                    associatedRules = RuleManager.GetAssociatedRuleAsync(label.Id, tenantId);
                    if (associatedRules.IsNullOrEmpty())
                    {
                        logger.Warn($"Not found any associated rules label, labelId: {label.Id}");
                        continue;
                    }
                    matchedTermId = associatedRules.FirstOrDefault().Key;
                    foreach (var associatedRule in associatedRules)
                    {
                        matchedRule = RuleManager.MatchedPotentialRule(itemInfo, associatedRule.Value, true);
                        if (matchedRule.Item1 != null)
                        {
                            matchedTermId = associatedRule.Key;
                            break;
                        }
                    }
                }
                RMTerm? rmTerm = null;
                if (matchedTermId > 0)
                {
                    rmTerm = TermDao.GetRMTermByTermId(matchedTermId);
                }
                report.TermName = rmTerm?.Name ?? string.Empty;
                report.Title = file.Name;
                report.LevelStr = (int)file.Level;
                report.Url = file.RelativePath;
                report.CreatedTime = file.CreatedTime.Ticks;
                report.LastModifiedTime = file.ModifiedTime.Ticks;
                report.FileType = file.FileExtension;
                report.OperationTime = file.CreatedTime.Ticks.Equals(DateTime.MinValue) ? string.Empty : file.CreatedTime.Ticks.ToString();
                report.OperationBy = file.CreatedBy;
                report.Operation = (int)OperationType.Created;

            }
            catch (Exception e)
            {
                _logger.Error($"Failed to generate created report. {e}");
            }

            return report;
        }
        private void LoadDestructionCache(string nodeId)
        {
            string filePath = String.Empty;
            using (PerformanceScope scope = new PerformanceScope("CreationAndDestroyedFileReportProcessor.DownloadCacheFromStorage"))
            {
                filePath = DestructionFactory.GetInstance(nodeId, string.Empty).DownloadCacheFromStorage(nodeId.ToString(), startUtcTime, endUtcTime);
            }
            if (!string.IsNullOrWhiteSpace(filePath) && System.IO.Directory.Exists(filePath))
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(filePath);
                foreach (var file in dir.GetFiles())
                {
                    DestructionUtility destructionUtility = new DestructionUtility(file.FullName);
                    int readCount = 0;
                    int pageSize = 100;
                    int pageIndex = 0;
                    do
                    {
                        var records = destructionUtility.SelectValuesFromDB(pageIndex, pageSize);
                        pageIndex += records.Count;
                        readCount = records.Count;
                        GoogleLiteDBWrapper.CreateInstance(GetLiteDBPath(nodeId)).Insert(records.Select(record => ConvertDestructionReportToGoogleDestructionData(record)).ToList());
                    }
                    while (readCount == 100);

                }
                try
                {
                    System.IO.Directory.Delete(filePath, true);
                }
                catch (Exception e)
                {
                    _logger.Warn($"Failed to delete destruction cache file. {e}");
                }
            }
            else
            {
                _logger.Warn("Destruction cache file not exist.");
            }
            DestructionFactory.Dispose(nodeId.ToString(), string.Empty);
        }
        private void ClearDestructionCache(string nodeId)
        {
            DestructionFactory.Dispose(nodeId.ToString(), string.Empty);
            GoogleLiteDBWrapper.CreateInstance(GetLiteDBPath(nodeId.ToString())).Dispose();
        }
        private string GetLiteDBPath(string siteId)
        {
            return SecurityUtils.SafeCombinePath(RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.REPORT_TEMP_FOLDER], "DestructionLiteDB", siteId);
        }
        private GoogleDestructionData ConvertDestructionReportToGoogleDestructionData(DestructionReport destructionReport)
        {
            var metaData = JsonConvert.DeserializeObject<GoogleDestructionMetaData>(destructionReport.JsonMeta);

            GoogleDestructionData data = new GoogleDestructionData();

            data.ScopeId = destructionReport.NodeId;
            data.DestroyedTime = destructionReport.ArchivedTime;
            data.RuleId = destructionReport.RuleID.ToString();
            data.FullPath = destructionReport.FullPath;
            data.MetaInfo = destructionReport.JsonMeta;

            if (metaData != null)
            {
                data.ItemName = metaData.ItemName;
                data.Level = metaData.Level;
                data.TermId = metaData.TermId;
            }
            return data;
        }

        internal enum OperationType
        {
            Created = 0,
            Destroyed = 1
        }

    }
}
