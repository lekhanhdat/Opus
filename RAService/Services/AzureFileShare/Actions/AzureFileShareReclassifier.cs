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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Object.RealTime;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.AzureFileShare.Api;
using AvePoint.RA.Service.Services.AzureFileShare.Converters;
using AvePoint.RA.Service.Services.AzureFileShare.RuleManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.Tenant;

namespace AvePoint.RA.Service.Services.AzureFileShare.Actions
{
    public class AzureFileShareReclassifier
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AzureFileShareReclassifier));

        private static IRMRecordsUpdateTempDao RecordsUpdateTempDao =>
            PlatformWindsorManager.GetService<IRMRecordsUpdateTempDao>();

        private static IRMClassificationHistoryDao ClassificationHistoryDao =>
            PlatformWindsorManager.GetService<IRMClassificationHistoryDao>();

        private static IRecordsHistoryService RecordsHistoryService =>
            PlatformWindsorManager.GetService<IRecordsHistoryService>();

        private static ITermRuleAssociationDao TermRuleAssociationDao =>
PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private static IRuleManagerService RuleManagerService =>
    PlatformWindsorManager.GetService<IRuleManagerService>();

        private static ITermSetDao TermSetDao =>
PlatformWindsorManager.GetService<ITermSetDao>();

        private static ITermDao TermDao =>
PlatformWindsorManager.GetService<ITermDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        private readonly ChangeTermOption ChangeTermInfo;

        private readonly string JobId;

        private readonly bool IsRunOnJob;

        public int FailedItemsCount { get; private set; }

        public AzureFileShareReclassifier(ChangeTermOption changeTermInfo, string jobId, bool isRunOnJob)
        {
            ChangeTermInfo = changeTermInfo;
            JobId = jobId;
            IsRunOnJob = isRunOnJob;
        }

        public void Reclassify()
        {
            try
            {
                using(new PerformanceScope("AzureFileShare.Reclassify"))
                {
                    Logger.Info($"Start process reclassify action.");
                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Running);
                    var needProcessItemIds = ChangeTermInfo.SourceAzureFileShareRecordIds;
                    if(needProcessItemIds?.Count == 0)
                    {
                        Logger.Warn($"Has't need process azure file share items.");
                        RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Finished);
                        return;
                    }

                    var items = ExplorerDao.QueryAll(item => needProcessItemIds.Contains(item.Id) && item.NodeType == (int)RMNodeLevel.AzureFileShareFile).ToList();

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Running, JsonConvert.SerializeObject(items.Select(item => item.LeafName)));

                    var rules = GetTermMatchedRules();
                    Reclassify(items, rules);

                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Finished);
                    Logger.Info($"Successful process reclassify action.");
                }
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while execute reclassify action. Error: {e}");
                RecordsUpdateTempDao.InsertUpdateTemp(JobId, "", RecordsConstants.Explorer_RealTime_Failed_All);
            }
        }

        private void Reclassify(List<Record> items, List<Rule> rules)
        {
            Logger.Info($"Need process reclassify action items count: [{items.Count}].");
            var succeedItems = new List<Record>();
            var failedItems = new List<Record>();
            var isNewLogicAccount = TenantService.IsNewOpusTenant();
            var previousTermId = Guid.Empty;
            foreach (var item in items)
            {
                try
                {
                    if(item.NodeType != (int)RMNodeLevel.AzureFileShareFile)
                    {
                        Logger.Warn($"Azure file share reclassify action not support except file node type [{item.Id}].");
                        continue;
                    }

                    if (item.TermId == ChangeTermInfo.TargetTermUniqueId)
                    {
                        AddFailedDetail(item, "RM_JM_GlobalSearch_ChangeTermFailed");
                        failedItems.Add(item);
                        Logger.Warn($"{item.Id} is already classify in this Term");
                        continue;
                    }

                    AzureFileShareSettingDao azureFileShareSettingDao = new AzureFileShareSettingDao();
                    var settingTermSetId = (azureFileShareSettingDao.LoadSetting(new Guid(item.ContainerId), new Guid(item.ContainerId))).TermSetId;
                    var settingTermGroupId = TermSetDao.GetRMTermSetByGuid(settingTermSetId).TermGroupId;

                    var targetTermSetId = TermDao.GetActiveTermById(ChangeTermInfo.TargetTermId).TermSetId;
                    var targetTermGroupId = TermSetDao.GetRMTermSet(targetTermSetId).TermGroupId;

                    if (settingTermGroupId != targetTermGroupId)
                    {
                        AddFailedDetail(item, "RM_FS_FolderReclassify_FileNotInSameTermScope");
                        failedItems.Add(item);
                        Logger.Warn($"Cannot reclassify a Term outside of a setting");
                        continue;
                    }

                    previousTermId = item.TermId;

                    item.TermId = ChangeTermInfo.TargetTermUniqueId;
                    item.TermName = ChangeTermInfo.TargetTermName;
                    if(isNewLogicAccount && previousTermId != ChangeTermInfo.TargetTermUniqueId) item.RemoveManualFields();

                    var fileInfo = AzureFileShareRecordConverter.ConvertAzureFileItem2AzureFileInfo(item);
                    ApplyRuleInfo(fileInfo, item, rules);
                    ExplorerDao.AddOrUpdateRecordWithKeepManual(item, true, isKeepManualColumn: false);

                    AddSucceedDetail(item, previousTermId);

                    succeedItems.Add(item);
                    Logger.Info($"Succeed process record [{item.Id}] reclassify action.");
                }
                catch(Exception e)
                {
                    AddFailedDetail(item, "RM_JM_GlobalSearch_ChangeTermFailed");

                    failedItems.Add(item);
                    Logger.Error($"An error occurred while process record [{item.Id}] reclassify action. Error: {e}");
                }
            }

            AddProcessReclassifyItemsToHistory(succeedItems, failedItems);
        }

        private void ApplyRuleInfo(AzureFileInfo fileInfo, Record record, List<Rule> rules)
        {
            record.RuleId = Guid.Empty;
            record.RuleLevel = (int)PolicyLevel.None;
            record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
            record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;

            var matchedRule = new AzureFileShareRuleManagement(rules).MatchPotentialRule(fileInfo, true);
            if (matchedRule == null)
            {
                Logger.Warn($"The item [{record.Id} - {record.AveSiteId}] is not match any rule.");
                return;
            }

            var ruleInfo = matchedRule.Item1;
            var dueDate = matchedRule.Item2;
            record.RuleId = string.IsNullOrEmpty(ruleInfo.Id) ? Guid.Empty : new Guid(ruleInfo.Id);
            record.RuleLevel = (int)ruleInfo.PolicyLevel;
            record.DisposalDueDate = record.PreviosDisposalDueDate = dueDate == default ? AvePoint.RA.Contract.Common.DueDateUtil.NextJob : DateTime.UtcNow.Add(dueDate).Ticks;
            if (record.HoldStatus)
            {
                if (record.DisposalDueDate == AvePoint.RA.Contract.Common.DueDateUtil.NextJob || record.DisposalDueDate < record.HoldReleaseTime)
                {
                    record.DisposalDueDate = record.HoldReleaseTime;
                    record.PreviosDisposalDueDate = record.HoldReleaseTime;
                }
            }
        }

        private List<Rule> GetTermMatchedRules()
        {
            var termRelatedRuleInfoes = TermRuleAssociationDao.GetTermRuleInfoByTermUniqueId(ChangeTermInfo.TargetTermUniqueId);
            if (termRelatedRuleInfoes.Count == 0)
            {
                Logger.Warn($"Current term [{ChangeTermInfo.TargetTermUniqueId}] not found related rule infoes.");
                return new List<Rule>();
            }

            var ruleIds = termRelatedRuleInfoes.Select(item => item.RuleId).ToList();
            Logger.Info($"Term [{ChangeTermInfo.TargetTermUniqueId}] related rules [{string.Join(", ", ruleIds)}].");
            var rules = RuleManagerService.GetRulesByIds(ruleIds);
            rules = rules.Where(item => item.AzureFileRule != null).OrderBy(item => termRelatedRuleInfoes.First(i => i.RuleId.ToString() == item.Id).RuleOrder).ToList();
            return rules;
        }

        private void AddSucceedDetail(Record item, Guid previousTermId)
        {
            try
            {
                ClassificationHistoryDao.Create(new DB.Model.RMClassificationHistory
                {
                    RecordId = item.Id,
                    PreviousTermId = previousTermId,
                    NewTermId = item.TermId,
                    OperationTime = DateTime.UtcNow.Ticks
                });
                Logger.Info($"Succeed add item [{item.Id}] reclassify action to history.");

                if (IsRunOnJob)
                {
                    ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = item.LeafName,
                        FullPath = AzureFileShareApiUtil.UrlCombin(item.DirPath, item.LeafName),
                        Action = "RM_JS_BCM_Explorer_ChangeTerm",
                        Status = JobDetailsStatus.Successful,
                        Type = item.NodeType == (int)RMNodeLevel.AzureFileShareDirectory ? "RM_RDM_RecordDetails_DataType_AzureFileDirectory" : "RM_JS_Rule_ObjectLevel_Document"
                    });
                }

                Logger.Info($"Add item [{item.Id}] succeed detail completed.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while add item [{item.Id}] succeed detail. Error: {e}");
            }
        }

        private void AddFailedDetail(Record item, string comment)
        {
            try
            {
                if (IsRunOnJob)
                {
                    ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMGlobalSearchActionJobDetails()
                    {
                        ObjectName = item.LeafName,
                        FullPath = AzureFileShareApiUtil.UrlCombin(item.DirPath, item.LeafName),
                        Action = "RM_JS_BCM_Explorer_ChangeTerm",
                        Status = JobDetailsStatus.Failed,
                        Comment = comment,
                        Type = item.NodeType == (int)RMNodeLevel.AzureFileShareDirectory ? "RM_RDM_RecordDetails_DataType_AzureFileDirectory" : "RM_JS_Rule_ObjectLevel_Document"
                    });
                }

                Logger.Info($"Add item [{item.Id}] failed detail completed.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while add item [{item.Id}] failed detail. Error: {e}");
            }
        }

        private void AddProcessReclassifyItemsToHistory(List<Record> succeedItems, List<Record> failedItems)
        {
            try
            {
                if(succeedItems.Any())
                {
                    RecordsHistoryService.AddRecordsHistory(succeedItems.Select(item => item.Id).ToList(), "RM_BCM_Audit_Action_ChangeTerm", ChangeTermInfo.Comment);
                }

                if(failedItems.Any())
                {
                    FailedItemsCount += failedItems.Count;
                    RecordsHistoryService.AddRecordsHistory(failedItems.Select(item => item.Id).ToList(), "RM_JS_Audit_ChangeTermErrorMessage");
                    RecordsUpdateTempDao.InsertUpdateTemp(JobId, string.Join(";", failedItems.Select(item => item.LeafName)), RecordsConstants.Explorer_RealTime_Failed_Partial);
                }

                Logger.Info($"Succeed add process reclassify items to history.");
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while add process reclassify items to history. Error: {e}");
            }
        }
    }
}
