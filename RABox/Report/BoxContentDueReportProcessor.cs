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
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RABox.Converters;
using RABox.Report.Base;
using RABox.RuleManagement;
using RABox.Util;


namespace RABox.Report
{
    public class BoxContentDueReportProcessor : ReportProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(BoxContentDueReportProcessor));

        private DateTime _timePoint;

        public BoxContentDueReportProcessor(string jobId, JobType jobType, string profileId) : base(profileId)
        {
            JobId = jobId;
            JobType = jobType;
        }

        protected override void Initialize()
        {
            _timePoint = ReportCenter.GetTimePoint(ProfileDto.Extension1);
            var ruleCache = RuleManager.LoadBoxRules();
            foreach (var rule in ruleCache.Values)
            {
                try
                {
                    ModifyTimeCriteria(rule, _timePoint);
                }
                catch (Exception e)
                {
                    _logger.Warn($"[{rule.Name}] ModifyTimeCriteria error:{e}");
                }
            }
            var termCache = TermManager.LoadTerms();
            var membershipsCache = TermManager.LoadTermSetMemberShips().Result;
            RuleManager.AssembleTermRuleMappingAsync(ruleCache, termCache, membershipsCache);
        }

        protected override void ProcessFiles(Guid folderId)
        {
            bool hasNext = true;
            string pageIndex = string.Empty;
            while (hasNext)
            {
                using CheckJobStopScope subJScope = new CheckJobStopScope();
                Tuple<IEnumerable<Record>, string> result = RecordManager.QueryFileRecordsByParent(folderId, pageIndex, RMRecordStatus.Destroyed);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                List<Record> datas = result.Item1.ToList();
                foreach (var file in datas)
                {
                    ProcessFile(file);
                }
            }
        }

        protected override void ProcessFile(Record record)
        {
            List<Rule> rules;
            _logger.Info($"Process File {record.Id}");
            try
            {
                if (RuleManager.TryGetRulesByTermIdFromCache(record.TermId, out rules))
                {
                    var boxRuleManagement = new BoxRuleManagement(rules);
                    var boxItemInfo = record.ConvertBoxItemInfo();
                    Tuple<Rule, TimeSpan> matchedRule = null;
                    try
                    {
                        matchedRule = boxRuleManagement.MatchPotentialRule(boxItemInfo);
                    }
                    catch (Exception e)
                    {
                        _logger.Warn($"CheckCriteria Rule Exception: Save this rule and try again: {(e.Data.Contains("ruleName") ? e.Data["ruleName"] : "")}. Error: {e}");
                    }

                    if (matchedRule != null && matchedRule.Item1 != null)
                    {
                        _logger.Info($"{record.Id} fit rule :{matchedRule.Item1.Name}");
                        bool onHold = RecordManager.IsRecordsHold(new List<Guid>() { record.Id }, _timePoint.Ticks);
                        if (onHold)
                        {
                            _logger.Info($"Current file is on hold. id:[{record.Id}]");
                            ReportCenter.RecordSkip(record.GenerateReportJobDetail(I18NResource.ReportSkipOnHold), record.NodeType);
                        }
                        else
                        {
                            ReportCenter.SendReport(GenerateDueDisposalReport(record, matchedRule.Item1), record.GenerateReportJobDetail());
                        }
                    }
                }
                else
                {
                    _logger.Info($"Process file skip {record.Id}");
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Disposal file failed {record.DirPath} : {e}");
            }
        }

        private void ModifyTimeCriteria(Rule rule, DateTime timePoint)
        {
            var soFilters = rule.BoxRule?.Filters;
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
                    Level = rule.BoxRule.PolicyLevel,
                    Rule = new CreatedRule() { Value1 = "Created Time" },
                    RuleType = PolicyRuleType.CreatedTime,
                    Value = new PolicyValue(timePoint.ToString(APIDateTimeFormat.DATETYPEForAPI003)),
                    SequenceNo = 1
                });

                _logger.Info($"Before convert and or express:{rule.BoxRule.AndOrExpression[PolicyLevel.BoxDocument]}");
                var tempStrs = rule.BoxRule.AndOrExpression[PolicyLevel.BoxDocument].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
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
                rule.BoxRule.AndOrExpression = new Dictionary<PolicyLevel, string>()
                {
                    { rule.BoxRule.PolicyLevel, andOrExpression }
                };
                _logger.Info($"After convert and or express:{rule.BoxRule.AndOrExpression[PolicyLevel.BoxDocument]}");
            }
        }

        private List<ReportRelatedRecords> GetRelatedRecords(Guid id)
        {
            var currRecord = RecordManager.GetRecordsByIds(new List<Guid> { id }).FirstOrDefault();
            List<ReportRelatedRecords> reportRelatedRecords = new List<ReportRelatedRecords>();
            if (!string.IsNullOrEmpty(currRecord?.RelatedRecords))
            {
                List<RMRelatedItemInfo> relatedRecords = SerializerHelper.DeserializeFromXmlString<List<RMRelatedItemInfo>>(currRecord.RelatedRecords);
                if (relatedRecords.Any())
                {
                    reportRelatedRecords.AddRange(relatedRecords
                        .Where(i => i.SourceFlag == (int)SourceFlag.Box)
                        .Select(r => new ReportRelatedRecords() { Name = r.recId, Url = r.url }));
                }
            }
            return reportRelatedRecords;
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

        private DueDisposalReport GenerateDueDisposalReport(Record record, Rule rule)
        {
            TermManager.TryGetTerm(record.TermId, out var term);
            DueDisposalReport report = new DueDisposalReport();

            report.TitleOrName = record.LeafName;
            report.Url = record.DirPath;
            report.BCSTermId = record.TermId.ToString();
            report.BCSTermName = term?.Name;
            report.ObjectLevel = (int)RMReportObjectLevel.BoxFile;
            report.CreatedBy = record.CreatedBy;
            report.CreatedTime = record.TimeCreated;
            report.LastModifiedBy = record.ModifiedBy;
            report.LastModifiedTime = record.TimeModified;
            report.AppliedRuleId = rule.Id;
            report.AppliedRuleName = rule.Name;
            report.ManualApproval = rule.BoxRule.IsManualApproval ? RMDisposalManualApproval.Yes : RMDisposalManualApproval.No;
            report.DisposalClass = rule.DisposalClass;
            report.RelatedRecords = SerializerHelper.SerializeToXmlString(GetRelatedRecords(record.Id));
            report.RelatedRecordsAction = (int)rule.BoxRule.RelatedRecordOption;
            report.DisposalAction = (int)GetDisposalAction(rule.BoxRule);
            report.ExportType = (RMExportTypeValue)AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None;

            return report;
        }

        private RMContentDisposalAction GetDisposalAction(Rule rule)
        {
            if (rule == null)
            {
                return RMContentDisposalAction.None;
            }
            else
            {
                var deleteOption = RMContentDisposalAction.Remove;
                if (rule.RelatedRecordOption == AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption.Both)
                {
                    deleteOption |= RMContentDisposalAction.RelatedRecords;
                }
                return deleteOption;
            }
        }

    }
}