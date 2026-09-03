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

using AvePoint.Common.FilterEngine;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Records.Core.Utilities.Extensions;
using AvePoint.Wrapper.Common;
using RAGoogle.Archive.ApprovalService;
using RAGoogle.Archive.Scan.Base;
using RAGoogle.Archive.Scan.Interface;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.GoogleObjDiscover.Impl;
using RAGoogle.Helper;
using RAGoogle.ManualManagement;
using RAGoogle.Models;
using RAGoogle.Report;
using RAGoogle.Services;
using RAGoogle.Util;

namespace RAGoogle.Archive.Scan.Implement
{
    public class DiscoverNodeWorkerBase : IDiscoverNodeWorker
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(RMGoogleFullDiscover));
        private Dictionary<string, RuleNodeContract> breakInheritNodes { get; set; }
        internal IBackwardDependencyNodeCache<ArchiveApproveReport> mApprovalReportProxy { get; set; }
        internal GoogleConfiguration Config { get; set; }
        protected RuleManager RuleManager { get; set; }
        protected RecordManager RecordManager { get; set; }
        protected ReportCenter ReportCenter { get; set; }
        protected ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private GoogleManualManagement _manualManagement { get; set; }
        public Dictionary<string, RuleNodeContract> BreakInheritNodes
        {
            get { return breakInheritNodes; }
            set { breakInheritNodes = value; }
        }

        public DiscoverNodeWorkerBase(GoogleConfiguration paraConfig)
        {
            Config = paraConfig;
            RecordManager = paraConfig.RecordManager;
            RuleManager = paraConfig.RuleManager;
            ReportCenter = paraConfig.ReportCenter;
            mApprovalReportProxy = new BackwardDependenceNodeCache<ArchiveApproveReport>(new ApprovalReportService(Config));
            _manualManagement = new();
            var settingInfo = ConvertHelper.ConvertRMSetting2Dto(paraConfig.GoogleSetting);
            _manualManagement.Build(paraConfig.RecordManager, ReportCenter, paraConfig.JobId, settingInfo);
        }


        public void Dispose()
        {
            using (mApprovalReportProxy) { }
        }

        public void Init(object obj)
        {
            RuleNodeContract nodeContract = (obj as RuleNodeContract)!;
            breakInheritNodes = nodeContract!.BreakInheritNodesEncryptBySha1;
            if (WrapperConfiguration.IsProcessApprovalDatasOnly)
            {
                breakInheritNodes.Clear();
            }
        }

        public virtual bool IsRuleBreakInheritNode(string sha1URL)
        {
            return breakInheritNodes != null && breakInheritNodes.ContainsKey(sha1URL);
        }


        public void Flush()
        {
            mApprovalReportProxy.Flush();
        }

        public virtual bool NeedSkipCurrentRule(Rule rule)
        {
            return false;
        }

        internal void TransmitToNextLayer(ArchiverNodeItem item)
        {
            using (PerformanceScope pc = new PerformanceScope("ArchiverScan.DiscoverNodeWorkerBase.TransmitToNextLayer"))
            {
                mApprovalReportProxy.PutIn(item.ConvertToArchiveApproveReport(), item.Cache_NodeType, item.ShouldDoArchive);
            }
        }
        public virtual async Task<ProcessResult> ProcessItemAsync(ArchiverNodeItem item, ArchiverNodeItem parent)
        {
            using (PerformanceScope pc = new PerformanceScope("ArchiverScan.DiscoverNodeWorkerBase.ProcessItemAsync"))
            {
                logger.Info(string.Format("begin to scan item, ID:{0}.", item.ID));
                ProcessResult result = ProcessResult.Default;
                Rule resultRule = null;
                //currently, google only support document level rule, only document version has parent(current version)
                if (item.Parent != null && !string.IsNullOrEmpty(item.Parent.RuleId) && item.Parent.DoDelete)
                {
                    item.RuleId = item.Parent.RuleId;
                    item.DoDelete = item.Parent.DoDelete;
                    item.ShouldDoArchive = true;
                    item.ArchiveLevel = true;
                    item.RuleName = item.Parent.RuleName;
                    var rule = Config.RuleCollection.Values.Where(r => r.Id.Equals(item.RuleId))?.FirstOrDefault();
                    if (rule?.GoogleDriveRule != null && (rule.GoogleDriveRule.PolicyLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.GoogleDriveDocument))
                    {
                        item.ForcedReport = true;
                    }
                    item.Term = item.Parent.Term;
                    TransmitToNextLayer(item);
                    return ProcessResult.FitParentRule;
                }

                resultRule = await CheckItemRuleAsync(item);
                ProcessItemCheckResultNode(resultRule, ref item, parent);
                TransmitToNextLayer(item);
                logger.Info(string.Format("finish to scan item, id:{0}. should do archive:{1}", item.ID, item.ShouldDoArchive));
                return result;
            }
        }
        public virtual async Task<ProcessResult> ProcessContainerAsync(ArchiverNodeItem item, ProcessType withType)
        {
            using (PerformanceScope pc = new PerformanceScope("ArchiverScan.DiscoverNodeWorkerBase.ProcessContainerAsync"))
            {
                ProcessResult result = ProcessResult.Default;
                var nodeType = Enum.Parse(typeof(NodeLevel), item.NodeLevel.ToString());
                result = nodeType switch
                {
                    NodeLevel.GoogleMyDrive or NodeLevel.GoogleSharedDrive => ProcessResult.Continue,
                    NodeLevel.GoogleFolder => ProcessResult.Continue,
                    _ => ProcessResult.SkipCurrentNode
                };
                TransmitToNextLayer(item);
                return await Task.FromResult(result);
            }
        }
        internal virtual async Task<Rule> CheckItemRuleAsync(ArchiverNodeItem item)
        {
            using (PerformanceScope pc = new PerformanceScope("ArchiverScan.DiscoverNodeWorkerBase.CheckItemRule"))
            {
                if (item.Cache_NodeType == (int) GoogleCacheNodeType.ItemVersion)
                {
                    logger.Warn("Don't support filter with item version");
                    return null;
                }
                var (rule, term) = CalculateMatchedPotentialRule(item.GoogleItemData, Config.SelectedNode);
                item.Term = term;
                var record = Config.GoogleSetting.IsNullClassificationSetting switch
                {
                    true => ProcessRecordItemManager(item.GoogleItemData, Config.SelectedNode, rule),
                    _ => ProcessRecordItemManager(item.GoogleItemData, Config.SelectedNode, rule, term)
                };
                if (record != null)
                {
                    if (IsSkipProcess(record, item.GoogleItemData, rule))
                    {
                        return null;
                    }
                    if (await _manualManagement.IsNeedProcessManualDisposalAsync(rule, record))
                    {
                        return null;
                    }
                }
                return rule;
            }
        }
        private bool IsSkipProcess(Record record, GoogleItemData item, Rule matchedRule)
        {
            if (record.HoldStatus && record.HoldReleaseTime > DateTime.UtcNow.Ticks)
            {
                logger.Warn($"Item [{record.Id}] is RecordsHold.");
                ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                    I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                    I18NResource.FileOnHold), (int)item.Level);
                return true;
            }

            if (record.DisposalDueDate > DateTime.UtcNow.Ticks)
            {
                logger.Warn($"The item [{item.Id}] has not reached action due date yet.");
                ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(string.Empty,
                    matchedRule.Name,
                    I18NResource.NotYetDueDate), (int)item.Level);
                return true;
            }

            return false;
        }
        private (Rule? rule, RMTerm? term) CalculateMatchedPotentialRule(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    var itemInfo = item.ConvertToInfo();
                    Tuple<Rule, TimeSpan>? matchedRule = null;
                    RMTerm? rmTerm = null;
                    if (Config.GoogleSetting.IsNullClassificationSetting)
                    {
                        matchedRule = RuleManager.MatchedPotentialRule(itemInfo, Config.RuleCollection.Values.ToList());
                    }
                    else
                    {
                        using (GoogleDriveService service = new(Config.AppProfile, item.MemberEmail))
                        {
                            int matchedTermId = -1;
                            List<int> aveLabelIds = [];
                            Dictionary<int, List<Rule>>? associatedRules = null;

                            foreach (var label in item.MetaInfo.Labels)
                            {
                                associatedRules = RuleManager.GetAssociatedRuleAsync(label.Id, selectedNode.TenantId, true);
                                if (associatedRules.IsNullOrEmpty())
                                {
                                    logger.Warn($"Not found any associated rules label, labelId: {label.Id}");
                                    continue;
                                }
                                matchedTermId = associatedRules.FirstOrDefault().Key;
                                foreach (var associatedRule in associatedRules)
                                {
                                    matchedRule = RuleManager.MatchedPotentialRule(itemInfo, associatedRule.Value);
                                    if (matchedRule.Item1 != null)
                                    {
                                        matchedTermId = associatedRule.Key;
                                        break;
                                    }
                                }
                                if (matchedRule?.Item1 != null && matchedTermId > 0)
                                {
                                    break;
                                }
                            }
                            if (matchedTermId > 0)
                            {
                                rmTerm = TermDao.GetRMTermByTermId(matchedTermId);
                            }
                        }
                    }
                    return (matchedRule?.Item1, rmTerm);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"An error occurred while calculate matched rule [{item.Name}]. Error: {ex}");
                throw;
            }
        }
        private Record? ProcessRecordItemManager(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, Rule? matchedRule, RMTerm? rmTerm)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    int createdDate = (int)DateTime.UtcNow.Ticks;
                    bool isProcess = false;
                    if (RecordManager.TryGetRecordValue(item.UniqueId, createdDate, out Record existRecord))
                    {
                        if (rmTerm == null)
                        {
                            logger.Info("Not found any label associated with matched rule applied on item. itemId: {0}", item.Id);
                            existRecord.TermId = Guid.Empty;
                            existRecord.TermName = string.Empty;
                            existRecord.RuleId = Guid.Empty;
                            var approvedStatus = existRecord.IsGControlRecord
                                ? existRecord.GControlManualApprovedStatus
                                : existRecord.ManualApprovedStatus;
                            if (approvedStatus != (int)SOApproveDBStatus.None)
                            {
                                logger.Info("The item change with no matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                        string.Empty, string.Empty,
                                        I18NResource.ItemHaveApprovalStatusIsNotNone, ActionTab.Scan), (int)item.Level);
                                existRecord.RemoveManualProperties();
                            }
                        }
                        else
                        {
                            existRecord.TermId = rmTerm.UniqueId;
                            existRecord.TermName = rmTerm.Name;
                            var oldRuleId = existRecord.RuleId.ToString();
                            if (matchedRule == null && oldRuleId != Guid.Empty.ToString())
                            {
                                logger.Info("Rule changed and not matched. itemId: {0}, old rule id: {1}", item.Id, oldRuleId);
                                var approvedStatus = existRecord.IsGControlRecord
                                    ? existRecord.GControlManualApprovedStatus
                                    : existRecord.ManualApprovedStatus;
                                if (approvedStatus != (int)SOApproveDBStatus.None)
                                {
                                    logger.Info("The item change with no matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                    ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                        string.Empty, string.Empty,
                                        I18NResource.ItemHaveApprovalStatusIsNotNone, ActionTab.Scan), (int)item.Level);
                                    existRecord.RemoveManualProperties();
                                }
                                existRecord.RuleId = Guid.Empty;
                            }

                            if (matchedRule != null)
                            {
                                if (!oldRuleId.Eq(matchedRule.Id))
                                {
                                    logger.Info("Rule changed and matched. itemId: {0}, new rule id: {1}", item.Id, matchedRule.Id);
                                    existRecord.RuleId = new Guid(matchedRule.Id);
                                    var approvedStatus = existRecord.IsGControlRecord
                                        ? existRecord.GControlManualApprovedStatus
                                        : existRecord.ManualApprovedStatus;
                                    if (approvedStatus != (int)SOApproveDBStatus.None)
                                    {
                                        logger.Info("The item change with new matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                        // ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                        //     I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                                        //     I18NResource.NewMatchedRule), (int)item.Level);
                                        existRecord.RemoveManualProperties();
                                    }
                                }
                                isProcess = true;
                            }
                        }
                        RecordManager.UpdateRecordInfo(existRecord, item);
                        RecordManager.UpdateManualProperties(existRecord, true);
                    }
                    else
                    {
                        if (rmTerm == null || matchedRule == null || !matchedRule.GoogleDriveRule.IsManualApproval)
                        {
                            logger.Info("Item does not match rule criteria or does not enabel manual approval. Skip to generate new record. itemId: {0}", item.Id);
                            return null;
                        }
                        existRecord = item.ConvertToRecord(selectedNode, existRecord);
                        existRecord.RuleId = new Guid(matchedRule.Id);
                        existRecord.TermId = rmTerm.UniqueId;
                        existRecord.TermName = rmTerm.Name;
                        RecordManager.AddNewRecord(existRecord);
                        isProcess = true;
                    }

                    if (!isProcess)
                    {
                        return null;
                    }
                    return existRecord;
                }
            }
            catch (JobStopException)
            {
                logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                throw;
            }

        }
        
        private Record? ProcessRecordItemManager(GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, Rule? matchedRule)
        {
            try
            {
                using CheckJobStopScope jScope = new();
                int createdDate = (int)DateTime.UtcNow.Ticks;
                bool isProcess = false;
                if (matchedRule == null || !matchedRule.GoogleDriveRule.IsManualApproval)
                {
                    logger.Info("Item does not match rule criteria or does not enabel manual approval. Skip to generate new record. itemId: {0}", item.Id);
                    return null;
                }
                if (RecordManager.TryGetRecordValue(item.UniqueId, createdDate, out Record existRecord))
                {
                    var oldRuleId = existRecord.RuleId.ToString();

                    if (matchedRule != null)
                    {
                        if (!oldRuleId.Eq(matchedRule.Id))
                        {
                            logger.Info("Rule changed and matched. itemId: {0}, new rule id: {1}", item.Id, matchedRule.Id);
                            existRecord.RuleId = new Guid(matchedRule.Id);
                            existRecord.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
                            existRecord.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
                            var approvedStatus = existRecord.IsGControlRecord
                                ? existRecord.GControlManualApprovedStatus
                                : existRecord.ManualApprovedStatus;
                            if (approvedStatus != (int)SOApproveDBStatus.None)
                            {
                                logger.Info("The item change with new matched rule and item is in manual process, remove manual properties. Item id: {0}", item.Id);
                                // ReportCenter.RecordSkipCommon(item.GenerateDisposalActionJobDetail(
                                //     I18NResource.RemoveAndDestroyAction, matchedRule.Name,
                                //     I18NResource.NewMatchedRule), (int)item.Level);
                                existRecord.RemoveManualProperties();
                            }
                        }
                        isProcess = true;
                    }
                    RecordManager.UpdateRecordInfo(existRecord, item);
                    RecordManager.UpdateManualProperties(existRecord, true);
                }
                else
                {
                    existRecord = item.ConvertToRecord(selectedNode, existRecord);
                    existRecord.RuleId = new Guid(matchedRule.Id);
                    RecordManager.AddNewRecord(existRecord);
                    isProcess = true;
                }

                if (!isProcess)
                {
                    return null;
                }
                return existRecord;
            }
            catch (JobStopException)
            {
                logger.Warn("the job has stopped.");
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while process item [{item.Id}]. Error: {ex}");
                throw;
            }

        }

        private void ProcessItemCheckResultNode(Rule rule, ref ArchiverNodeItem item, ArchiverNodeItem parent)// to do unit test
        {
            if (rule != null)
            {
                item.DoDelete = true;
                item.ShouldDoArchive = item.ArchiveLevel = true;
                item.RuleId = rule.Id;
                item.RuleName = rule.Name;
            }
            else if (parent.ShouldDoArchive)
            {
                item.DoDelete = true;
                item.ShouldDoArchive = true;
                item.ArchiveLevel = true;
                item.RuleId = item.Parent.RuleId;
                item.RuleName = item.Parent.RuleName;
            }
            else
            {
                item.ShouldDoArchive = false;
            }
        }
    }
}
