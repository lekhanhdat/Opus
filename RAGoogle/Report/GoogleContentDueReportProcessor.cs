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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Util;
using System.Collections.Concurrent;
using Util;

namespace RAGoogle.Report
{
    public class GoogleContentDueReportProcessor : BaseReportProcessor
    {
        #region properties
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleContentDueReportProcessor));
        private DateTime _timePoint;
        private Dictionary<Guid, RMRuleItemCollection> _termAndRulesMapping;
        private ConcurrentDictionary<string, Rule> _ruleDics;
        #endregion

        public GoogleContentDueReportProcessor(string jobId, string profileId) : base(jobId, profileId)
        {
            this.jobType = JobType.GoogleItemsFilesDueDisposalReport;
            _ruleDics = new();
            _termAndRulesMapping = new();
        }

        protected override void InitializeReport()
        {
            _timePoint = ReportCenter.GetTimePoint(ProfileDto.Extension1);
            _termAndRulesMapping = ReportService.GetTermAndRuleMappingsNew(_timePoint, AvePoint.RA.Contract.Explorer.SourceFlag.Google);
            _ruleDics = RuleManager.LoadRules();
            foreach (var rule in _ruleDics.Values)
            {
                try
                {
                    RebuildTimeRule(rule, _timePoint);
                }
                catch (Exception e)
                {
                    _logger.Warn($"[{rule.Name}] Rebuild time rule error:{e}");
                }
            }

        }

        protected override async Task ProcessDriveAsync(GoogleDriveTreeNodeDto treeNode, DataQueue<GoogleItemData> itemQueue)
        {
            logger.Info($"Start processing node [{treeNode.ID}-{treeNode.Name}].");
            using (var performance = new PerformanceScope("GoogleContentDueReportProcessor:ProcessDriveAsync"))
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
                    logger.Warn("The content due report job has been stopped.");
                    throw new JobStopException("The job has stopped."); ;
                }
                catch (Exception ex)
                {
                    logger.Error($"Failed to process content due report job, Message: {ex}");
                    throw;
                }
            }
        }

        protected override void ProcessFileReport(GoogleItemData file)
        {
            using (CheckJobStopScope jScope = new())
            {
                try
                {
                    var fileInfo = file.ConvertToInfo();
                    Rule? matchedRule = null;
                    RMTerm? matchedTerm = null;
                    foreach (var label in file.MetaInfo.Labels)
                    {
                        RMRuleItemCollection? rulesCollection;
                        var term = TermDao.GetRMTermByLabelId(label.Id, tenantId);
                        if (term != null && _termAndRulesMapping.TryGetValue(term.UniqueId, out rulesCollection))
                        {
                            List<Rule> rules = [];
                            foreach (var rmRule in rulesCollection.Rules)
                            {
                                if (_ruleDics.TryGetValue(rmRule.RuleId, out var rule))
                                {
                                    rules.Add(rule);
                                }
                            }
                            matchedRule = RuleManager.MatchedPotentialRule(fileInfo, rules)?.Item1;
                            matchedTerm = term;
                            if (matchedRule != null)
                            {
                                break;
                            }
                        }
                    }

                    if (matchedRule != null && matchedTerm != null)
                    {
                        _logger.Info($"File {file.Name} fit rule: {matchedRule.Name}");
                        Record? record = null;
                        try
                        {
                            record = RecordManager.QueryHoldRecordById(file.UniqueId);
                        }
                        catch (Exception ex)
                        {
                            _logger.Warn($"Error occurred while getting hold record by ids. Error: {ex.Message}");
                        }
                        if (record != null && record.HoldReleaseTime > _timePoint.Ticks)
                        {
                            _logger.Warn("File is on explorer hold. The file should not be reported. Record id: {0}", file.UniqueId);
                            ReportCenter.RecordSkip(record.GenerateReportJobDetail(I18NResource.ReportSkipOnHold), record.NodeType);
                            return;
                        }
                        var report = GenerateDueDisposalReport(file, matchedRule, matchedTerm);
                        ReportCenter.SendReport(report, file.GenerateReportJobDetail());
                    }
                    else
                    {
                        _logger.Info($"File {file.Name} not fit any rules.");
                    }
                }
                catch (JobStopException)
                {
                    logger.Warn("The job has stopped.");
                    throw new JobStopException("The job has stopped.");
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        private DueDisposalReport GenerateDueDisposalReport(GoogleItemData item, Rule rule, RMTerm term)
        {
            DueDisposalReport report = new DueDisposalReport()
            {
                TitleOrName = item.Name,
                Url = item.RelativePath,
                BCSTermId = term.UniqueId.ToString(),
                BCSTermName = term.Name,
                ObjectLevel = (int)RMReportObjectLevel.GoogleFile,
                CreatedBy = item.CreatedBy,
                CreatedTime = item.CreatedTime.Ticks,
                LastModifiedBy = item.ModifiedBy,
                LastModifiedTime = item.ModifiedTime.Ticks,
                AppliedRuleId = rule.Id,
                AppliedRuleName = rule.Name,
                ManualApproval = rule.GoogleDriveRule.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No,
                DisposalClass = rule.DisposalClass,
                DisposalAction = (int)GetDisposalAction(rule),
                ExportType = (RMExportTypeValue)(rule.GoogleDriveRule.ExportInfo?.exportType ?? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None),

            };
            return report;
        }

        private RMContentDisposalAction GetDisposalAction(Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            if (rule.GoogleDriveRule is { spMoveOption: not null })
            {
                return RMContentDisposalAction.Move;
            }
            if (rule.GoogleDriveRule.ExportInfo is { exportSPDataOption: ExportSPDataOption.ExportWithoutArchive })
            {
                return RMContentDisposalAction.ExportOnly;
            }
            if ((rule.GoogleDriveRule.KeepDataOption & (int)KeepDataOption.Archive) == (int)KeepDataOption.Archive)
            {
                return RMContentDisposalAction.ArchiveToStorage;
            }
            var deleteOption = RMContentDisposalAction.Remove;
            if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
            {
                deleteOption |= RMContentDisposalAction.RelatedRecords;
            }
            return deleteOption;
        }

        private void RebuildTimeRule(Rule rule, DateTime timePoint)
        {
            var soFilters = rule.GoogleDriveRule?.Filters;
            if (soFilters != null)
            {
                _logger.Info($"rule name: {rule.Name}");
                foreach (var filter in soFilters)
                {
                    ModifyOlderThanCriteria(filter, timePoint);
                    filter.SequenceNo += 1;
                }
                //add created time criteria
                soFilters.Add(new SOFilterPolicy()
                {
                    Condition = PolicyCondition.Before,
                    Level = PolicyLevel.GoogleDriveDocument,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });

                _logger.Info($"Before convert and or express:{rule.GoogleDriveRule.AndOrExpression[PolicyLevel.GoogleDriveDocument]}");
                var tempStrs = rule.GoogleDriveRule.AndOrExpression[PolicyLevel.GoogleDriveDocument].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                string andOrExpression = "(1 And (";
                foreach (var str in tempStrs)
                {
                    int sequenceNo = 0;
                    if (int.TryParse(str, out sequenceNo))
                    {
                        sequenceNo++;
                        andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
                    }
                    else
                    {
                        andOrExpression = string.Format("{0} {1}", andOrExpression, str);
                    }
                }
                andOrExpression += "))";
                rule.GoogleDriveRule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                {
                    { PolicyLevel.GoogleDriveDocument, andOrExpression }
                };
                _logger.Info($"After convert and or express:{rule.GoogleDriveRule.AndOrExpression[PolicyLevel.GoogleDriveDocument]}");
            }
        }

        private void ModifyOlderThanCriteria(FilterPolicy filter, DateTime timePoint)
        {
            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule)
            {
                switch (filter.Condition)
                {
                    case PolicyCondition.OlderThan:
                        int num;
                        DateTime tempDt = DateTime.UtcNow;
                        if (int.TryParse(filter.Value.Value1, out num))
                        {
                            if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                            {
                                tempDt = timePoint.AddDays(-num);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                            {
                                tempDt = timePoint.AddDays(-num * 7);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                            {
                                tempDt = timePoint.AddMonths(-num);
                            }
                            else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                            {
                                tempDt = timePoint.AddYears(-num);
                            }
                            filter.Value.Value1 = tempDt.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                            filter.Condition = PolicyCondition.Before;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
