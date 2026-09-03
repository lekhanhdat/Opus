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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common.RMRuleManagement;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMReport;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AvePoint.RA.SharePoint.Discover
{
    public class DAUtil
    {
        private RALogger logger = RALogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public ISharePointSettingDao SPSettingDao { get; set; }
        public ITermSetDao TermSetDAO { get; set; }
        public ITermGroupDao TermGroupDao { get; set; }
        public ITermDao TermDao { get; set; }
        public ITermRuleAssociationDao TermRuleInfos { get; set; }
        public IProfileDao ProfileDao { get; set; }
        public DAUtil()
        {
            TermSetDAO = new TermSetDao();
            SPSettingDao = new SharePointSettingDao();
            TermGroupDao = new TermGroupDao();
            TermRuleInfos = new TermRuleAssociationDao();
            ProfileDao = new ProfileDao();
            TermDao = new TermDao();
        }
        public RMProfileDto GetProfileByIdForReportJob(string Id)
        {
            RMProfile profile = ProfileDao.GetProfileById(int.Parse(Id));
            RMProfileDto profileDto = new RMProfileDto()
            {
                Id = profile.Id,
                ProfileName = profile.Name,
                Description = profile.Description,
                Type = (JobType)profile.Type,
                Extension1 = profile.Extension1,
                Extension2 = profile.Extension2,
                Extension3 = profile.Extension3,
            };
            return profileDto;
        }
        public DateTime GetUtcTimePoint(string ext1)
        {
            var dateTimeObj = JsonConvert.DeserializeObject<DisplayDateTime>(ext1);
            DateTime utcDt = DateTime.Parse(dateTimeObj.StartTime);
            utcDt = DateTime.SpecifyKind(utcDt, DateTimeKind.Utc);
            return utcDt;
        }

        public RMSPTreeNode GetFarmSPTreeNode(string ext2)
        {
            return SerializerHelper.DeserializeByDataContractSerializer<RMSPTreeNode>(ext2);
        }
        public Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappings(DateTime timePoint, List<Rule> daRules, bool addCreatedFilter = true)
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = daRules.ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None && rule.SOFilters != null && rule.SOFilters.Count > 0)
                        {
                            reOrder++;
                            var ruleOBj = CloneSameRuleObject(rule);
                            //var ruleAssember = new RuleAssembler();
                            //ruleOBj = ruleAssember.ConvertToSPRule(ruleOBj);
                            commonRules.Rules.Add(reOrder, ruleOBj);
                            if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                            {
                                rmRules.Add(ConvertRuleChecker(ruleOBj, term, timePoint, addCreatedFilter));
                            }
                            else
                            {
                                ModifyRuleChecker(ruleOBj, term, timePoint);
                            }
                        }

                    }
                }
                if (rmRules.Count > 0)
                {
                    if (rmRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                    {
                        hasUnCamlQueryableCondition = true;
                    }
                }
                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms, timePoint.Ticks);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection();
                        tempRC.TermId = refTerm.UniqueId;
                        tempRC.TermName = refTerm.Name;
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }
                    tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;
                }
            }

            return termRuleMappings;
        }

        public Dictionary<Guid, RMRuleItemCollection> GetTermAndRuleMappingsForDataSync(DateTime timePoint, List<Rule> daRules)
        {
            List<RMTermRuleAssociation> trAssociations = TermRuleInfos.GetTermWithRule();
            Dictionary<int, List<Guid>> termRules = new Dictionary<int, List<Guid>>();
            foreach (var termId in trAssociations.Select(a => a.TermId).Distinct())
            {
                var rules = trAssociations
                    .Where(a => a.TermId == termId)
                    .OrderBy(a => a.RuleOrder)
                    .Select(a => a.RuleId)
                    .ToList();
                if (rules.Count > 0)
                {
                    termRules.Add(termId, rules);
                }
            }

            var termRuleMappings = new Dictionary<Guid, RMRuleItemCollection>();
            Dictionary<Guid, Rule> allRules = daRules.ToDictionary(r => new Guid(r.Id));
            var allHasRuleTerms = TermDao.GetRMTermsByTermIds(termRules.Keys.ToArray());
            foreach (var term in allHasRuleTerms)
            {
                if (term.IsRemoved)
                {
                    continue;
                }
                RuleCollection commonRules = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
                List<RMRuleItem> rmRules = new List<RMRuleItem>();
                bool hasUnCamlQueryableCondition = false;
                Rule rule;
                var ruleIds = termRules[term.Id];
                int reOrder = 0;
                for (int idx = 0; idx < ruleIds.Count; idx++)
                {
                    if (allRules.TryGetValue(ruleIds[idx], out rule))
                    {
                        if (rule.PolicyLevel != PolicyLevel.None && rule.SOFilters != null && rule.SOFilters.Count > 0)
                        {
                            reOrder++;
                            var ruleOBj = CloneSameRuleObject(rule);
                            //var ruleAssember = new RuleAssembler();
                            //ruleOBj = ruleAssember.ConvertToSPRule(ruleOBj);
                            commonRules.Rules.Add(reOrder, ruleOBj);
                            if (ruleOBj.PolicyLevel == PolicyLevel.Item || ruleOBj.PolicyLevel == PolicyLevel.Document || ruleOBj.PolicyLevel == PolicyLevel.Folder)
                            {
                                rmRules.Add(ConvertRuleCheckerForDataSync(ruleOBj, term, timePoint));
                            }
                            else
                            {
                                ModifyRuleCheckerForDataSync(ruleOBj, term, timePoint);
                            }
                        }

                    }
                }
                if (rmRules.Count > 0)
                {
                    if (rmRules.Exists(rc => rc.HasUnCamlQueryableCondition))
                    {
                        hasUnCamlQueryableCondition = true;
                    }
                }
                var refTerms = new List<RMTerm>();
                TermDao.GetAllInheritTermsByRootTerm(term.Id, ref refTerms, timePoint.Ticks);
                foreach (var refTerm in refTerms)
                {
                    RMRuleItemCollection tempRC;
                    if (!termRuleMappings.TryGetValue(refTerm.UniqueId, out tempRC))
                    {
                        tempRC = new RMRuleItemCollection();
                        tempRC.TermId = refTerm.UniqueId;
                        tempRC.TermName = refTerm.Name;
                        termRuleMappings.Add(refTerm.UniqueId, tempRC);
                    }
                    tempRC.HasUnCamlQueryableCondition = hasUnCamlQueryableCondition;
                    tempRC.CommonRules = commonRules;
                    tempRC.Rules = rmRules;
                }
            }

            return termRuleMappings;
        }
        public Rule CloneSameRuleObject(Rule rule)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rule);
            Rule result = SerializerHelper.DeserializeByDataContractSerializer<Rule>(xml);
            return result;
        }
        private void ModifyRuleChecker(Rule rule, RMTerm term, DateTime timePoint)
        {
            foreach (var filter in rule.Filters)
            {
                filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule)
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
                                filter.Value.Value1 = tempDt.ToString(DateTimeUtil.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            rule.Filters.Add(new FilterPolicy()
            {
                Condition = PolicyCondition.Before,
                Level = rule.PolicyLevel,
                Rule = new CreatedRule() { Value1 = "Created Time" },
                RuleType = PolicyRuleType.CreatedTime,
                Value = new PolicyValue(timePoint.ToString(DateTimeUtil.DATETYPEForAPI003)),
                SequenceNo = 1
            });
            logger.Info($"Before convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            //have a bug here should change order Created Time to last
            var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
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
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
        }
        private void ModifyRuleCheckerForDataSync(Rule rule, RMTerm term, DateTime timePoint)
        {
            foreach (var filter in rule.Filters)
            {
                filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule)
                {
                    //switch (filter.Condition)
                    //{
                    //    case PolicyCondition.OlderThan:
                    //        int num;
                    //        DateTime tempDt = DateTime.UtcNow;
                    //        if (int.TryParse(filter.Value.Value1, out num))
                    //        {
                    //            if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                    //            {
                    //                tempDt = timePoint.AddDays(-num);
                    //            }
                    //            else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                    //            {
                    //                tempDt = timePoint.AddDays(-num * 7);
                    //            }
                    //            else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                    //            {
                    //                tempDt = timePoint.AddMonths(-num);
                    //            }
                    //            else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    //            {
                    //                tempDt = timePoint.AddYears(-num);
                    //            }
                    //            filter.Value.Value1 = tempDt.ToString(DateTimeUtil.DATETYPEForAPI003);
                    //            filter.Condition = PolicyCondition.Before;
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }
            //rule.Filters.Add(new FilterPolicy()
            //{
            //    Condition = PolicyCondition.Before,
            //    Level = rule.PolicyLevel,
            //    Rule = new CreatedRule() { Value1 = "Created Time" },
            //    RuleType = PolicyRuleType.CreatedTime,
            //    Value = new PolicyValue(timePoint.ToString(DateTimeUtil.DATETYPEForAPI003)),
            //    SequenceNo = 1
            //});

            logger.Info($"Before convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
            //have a bug here should change order Created Time to last
            var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
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
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, andOrExpression }
            };
            logger.Info($"After convert and or express:{rule.AndOrExpression[rule.PolicyLevel]}");
        }
        private RMRuleItem ConvertRuleChecker(Rule rule, RMTerm term, DateTime timePoint, bool addCreatedFilter)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = RuleHelper.GetOperationType(rule);
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            if (rule.MoveToRecordCenterAndDelareSetting != null)// rule.spMoveOption != null ||  TO DO(fpwang)
            {
                checker.HasUnCamlQueryableCondition = true;
            }
            foreach (var filter in rule.SOFilters)
            {
                var arFilter = new ArchiverRuleFilter(filter);
                checker.RuleFilters.Add(arFilter);
                //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                if (!checker.HasUnCamlQueryableCondition)
                {
                    if (arFilter.Condition == ArchiverFilterCondition.Matches || arFilter.Condition == ArchiverFilterCondition.DoesNotMatch
                        || arFilter.Condition == ArchiverFilterCondition.DoesNotContain || arFilter.Condition == ArchiverFilterCondition.ListIn)
                    {
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.ContentType && arFilter.Condition == ArchiverFilterCondition.Contains)
                    {
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.CreatedBy || arFilter.RuleType == ArchiverFilterRuleType.ModifiedBy || arFilter.RuleType == ArchiverFilterRuleType.ParentListTypeID || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderName || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderNameHeirarchically
                        || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabel || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabelFullName)
                    {
                        checker.HasUnCamlQueryableCondition = true;
                    }
                }
            }

            foreach (var filter in rule.Filters)
            {
                //filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is ContentTypeRule)
                {
                    filter.RuleType = PolicyRuleType.ContentType;
                }
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                {
                    switch (filter.Condition)
                    {
                        #region old
                        // [REC-738] remove timepoint ref FromTo/Before
                        //case PolicyCondition.FromTo:
                        //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                        //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                        //    if (toDt > timePoint)
                        //    {
                        //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                        //    }
                        //    break;
                        //case PolicyCondition.Before:
                        //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                        //    if (ltDt >= timePoint)
                        //    {
                        //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                        //    }
                        //    break;
                        #endregion
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
                                filter.Value.Value1 = tempDt.ToString(DateTimeUtil.DATETYPEForAPI003);
                                filter.Condition = PolicyCondition.Before;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (addCreatedFilter)
            {
                AddCreateTimeRuleChecker(rule, timePoint);
            }
            if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url))
            {
                AddMoveToFilter(rule);
            }
            else if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                AddMoveToFilter(rule);
            }
            return checker;
        }

        private RMRuleItem ConvertRuleCheckerForDataSync(Rule rule, RMTerm term, DateTime timePoint)
        {
            RMRuleItem checker = new RMRuleItem();
            checker.HasUnCamlQueryableCondition = false;
            checker.RuleId = rule.Id;
            checker.RuleName = rule.Name;
            checker.IsMoveRule = RuleHelper.CheckMoveRule(rule);
            checker.ArchiverAction = RuleHelper.GetOperationType(rule);
            checker.IsManualApproval = rule.IsManualApproval;
            checker.ExportType = rule.ExportInfo == null ? AvePoint.GCommon.Contract.StorageOptimization.Object.ExportTypeValue.None : rule.ExportInfo.exportType;
            checker.DeleteRecords = rule.DeleteRecords;
            checker.RelatedRecordOption = (AvePoint.GCommon.Contract.StorageOptimization.Object.RelatedRecordOption)rule.RelatedRecordOption;
            checker.RuleFilters = new List<ArchiverRuleFilter>();
            if (rule.MoveToRecordCenterAndDelareSetting != null)// rule.spMoveOption != null ||  TO DO(fpwang)
            {
                checker.HasUnCamlQueryableCondition = true;
            }
            foreach (var filter in rule.SOFilters)
            {
                var arFilter = new ArchiverRuleFilter(filter);
                checker.RuleFilters.Add(arFilter);
                //不支持SP Query的Rule Type，HasUnCamlQueryableCondition赋值为true
                if (!checker.HasUnCamlQueryableCondition)
                {
                    if (arFilter.Condition == ArchiverFilterCondition.Matches || arFilter.Condition == ArchiverFilterCondition.DoesNotMatch
                        || arFilter.Condition == ArchiverFilterCondition.DoesNotContain || arFilter.Condition == ArchiverFilterCondition.ListIn)
                    {
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.ContentType && arFilter.Condition == ArchiverFilterCondition.Contains)
                    {
                        checker.HasUnCamlQueryableCondition = true;
                    }
                    else if (arFilter.RuleType == ArchiverFilterRuleType.CreatedBy || arFilter.RuleType == ArchiverFilterRuleType.ModifiedBy || 
                        arFilter.RuleType == ArchiverFilterRuleType.ParentListTypeID || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.LastActiveTime
                        || arFilter.RuleType == ArchiverFilterRuleType.ParentFolderName|| arFilter.RuleType == ArchiverFilterRuleType.ParentFolderNameHeirarchically
                        || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabel || arFilter.RuleType == ArchiverFilterRuleType.SensitivityLabelFullName)
                    {
                        checker.HasUnCamlQueryableCondition = true;
                    }
                }
            }

            foreach (var filter in rule.Filters)
            {
                //filter.SequenceNo = filter.SequenceNo + 1;
                if (filter.Rule is ContentTypeRule)
                {
                    filter.RuleType = PolicyRuleType.ContentType;
                }
                if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                {
                    //switch (filter.Condition)
                    //{
                    //    #region old
                    //    // [REC-738] remove timepoint ref FromTo/Before
                    //    //case PolicyCondition.FromTo:
                    //    //    var fromDt = ConvertUtcDateTime(filter.Value.Value1);
                    //    //    var toDt = ConvertUtcDateTime(filter.Value.Value2);
                    //    //    if (toDt > timePoint)
                    //    //    {
                    //    //        filter.Value.Value2 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                    //    //    }
                    //    //    break;
                    //    //case PolicyCondition.Before:
                    //    //    var ltDt = ConvertUtcDateTime(filter.Value.Value1);
                    //    //    if (ltDt >= timePoint)
                    //    //    {
                    //    //        filter.Value.Value1 = timePoint.ToString(AveDateTimeUtility.DATETYPEForAPI003);
                    //    //    }
                    //    //    break;
                    //    #endregion
                    //    case PolicyCondition.OlderThan:
                    //        int num;
                    //        DateTime tempDt = DateTime.UtcNow;
                    //        if (int.TryParse(filter.Value.Value1, out num))
                    //        {
                    //            if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                    //            {
                    //                tempDt = timePoint.AddDays(-num);
                    //            }
                    //            else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                    //            {
                    //                tempDt = timePoint.AddDays(-num * 7);
                    //            }
                    //            else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                    //            {
                    //                tempDt = timePoint.AddMonths(-num);
                    //            }
                    //            else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    //            {
                    //                tempDt = timePoint.AddYears(-num);
                    //            }
                    //            filter.Value.Value1 = tempDt.ToString(DateTimeUtil.DATETYPEForAPI003);
                    //            filter.Condition = PolicyCondition.Before;
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }
            //AddCreateTimeRuleChecker(rule, timePoint);
            if (rule.MoveToRecordCenterAndDelareSetting != null && rule.MoveToRecordCenterAndDelareSetting.DestinationLocation != null && !string.IsNullOrEmpty(rule.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url))
            {
                AddMoveToFilter(rule);
            }
            else if (rule.spMoveOption != null && rule.spMoveOption.MoveDestination != null)
            {
                AddMoveToFilter(rule);
            }
            return checker;
        }
        #region add logic for move to...
        public void AddCreateTimeRuleChecker(Rule ruleObj, DateTime timePoint)
        {
            var arFilter = new FilterPolicy()
            {
                Condition = PolicyCondition.Before,
                Level = ruleObj.PolicyLevel,
                Rule = new CreatedRule() { Value1 = "Created Time" },
                RuleType = PolicyRuleType.CreatedTime,
                Value = new PolicyValue(timePoint.ToString(DateTimeUtil.DATETYPEForAPI003)),
            };
            int ruleCount = ruleObj.Filters.Count;
            arFilter.SequenceNo = ruleCount + 1;
            //SOFilterPolicy filterDto = CloneSameFilterObject(arFilter.Dto);
            //ruleObj.SOFilters.Add(filterDto);
            ruleObj.Filters.Add(arFilter);
            AddAndOrExpressionForMove(ruleObj, arFilter);
            //have a bug here should change order Created Time to last
            #region old logic
            //var tempStrs = rule.AndOrExpression[rule.PolicyLevel].Split(new char[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            //string andOrExpression = "(1 And";
            //foreach (var str in tempStrs)
            //{
            //    int sequenceNo = 0;
            //    if (int.TryParse(str, out sequenceNo))
            //    {
            //        sequenceNo++;
            //        andOrExpression = string.Format("{0} {1}", andOrExpression, sequenceNo.ToString());
            //    }
            //    else
            //    {
            //        andOrExpression = string.Format("{0} {1}", andOrExpression, str);
            //    }
            //}
            //andOrExpression += ")";
            //rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            //{
            //    { rule.PolicyLevel, andOrExpression }
            //};
            #endregion
        }
        /// <summary>
        /// add destination checking in Last cretia.
        /// </summary>
        /// <param name="ruleObj"></param>
        public void AddMoveToFilter(Rule ruleObj)
        {
            var arFilter = new FilterPolicy();
            //arFilter.CombineMode = ArchiverFilterCombineMode.And;
            arFilter.Condition = PolicyCondition.DoesNotContains;
            arFilter.RuleType = PolicyRuleType.Url;
            var Rule = new UrlRule() { Value1 = "URL" };
            arFilter.Rule = Rule;
            if (ruleObj.MoveToRecordCenterAndDelareSetting != null)
            {
                arFilter.Value = new PolicyValue(ruleObj.MoveToRecordCenterAndDelareSetting.DestinationLocation.Url);
            }
            else
            {
                // TO DO(fpwang)
                if (ruleObj.spMoveOption != null && ruleObj.spMoveOption.MoveDestination.DestMode == DestMode.UrlMode)
                {
                    arFilter.Value = new PolicyValue(ruleObj.spMoveOption.MoveDestination.SPUrl);
                }
                else if (ruleObj.spMoveOption != null && ruleObj.spMoveOption.MoveDestination.DestMode == DestMode.TreeMode && ruleObj.spMoveOption.MoveDestination.SPTreeNode != null)
                {
                    arFilter.Value = new PolicyValue(ruleObj.spMoveOption.MoveDestination.SPTreeNode.FullPath);
                }
                else
                {
                    logger.Info("Return no need to add filter");
                    return;
                }
            }
            if (arFilter.Value.Value1.Contains("#/"))//add logic to trim url for Onpremise 2013,2016
            {
                var siteUrl = arFilter.Value.Value1.Substring(0, arFilter.Value.Value1.IndexOf("_layouts", StringComparison.OrdinalIgnoreCase));
                var listUrl = arFilter.Value.Value1.Substring(arFilter.Value.Value1.IndexOf("#/", StringComparison.OrdinalIgnoreCase) + 2).Split('/')[0];
                arFilter.Value.Value1 = siteUrl + listUrl;
            }
            else if (arFilter.Value.Value1.Contains("/Forms/")) //Add logic to trim URL for Office365
            {
                var libUrl = arFilter.Value.Value1.Substring(0, arFilter.Value.Value1.IndexOf("/Forms/"));
                arFilter.Value.Value1 = libUrl;
            }
            if (arFilter.Value.Value1.Contains("%"))
            {
                arFilter.Value.Value1 = HttpUtility.UrlDecode(arFilter.Value.Value1);
            }
            arFilter.Level = ruleObj.PolicyLevel;
            int ruleCount = ruleObj.Filters.Count;
            arFilter.SequenceNo = ruleCount + 1;
            //SOFilterPolicy filterDto = CloneSameFilterObject(arFilter.Dto);
            //ruleObj.SOFilters.Add(filterDto);
            ruleObj.Filters.Add(arFilter);
            AddAndOrExpressionForMove(ruleObj, arFilter);
        }
        private void AddAndOrExpressionForMove(Rule rule, FilterPolicy filterDto)
        {
            string AndOrExpression = rule.AndOrExpression[rule.PolicyLevel];
            //AndOrExpression += string.Format(" {0} {1}", "Or", filterDto.SequenceNo);
            AndOrExpression = AndOrExpression.Insert(AndOrExpression.Length - 1, string.Format(" {0} {1}", "And", filterDto.SequenceNo));
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>()
            {
                { rule.PolicyLevel, AndOrExpression }
            };
        }
        #endregion
       /* private RMContentDisposalAction GetOperationType(Rule rule)
        {
            int keepDataOption = rule.KeepDataOption;
            if (keepDataOption == (int)KeepDataStatus.LinkToDocument)
            {
                return RMContentDisposalAction.LeaveStub;
            }
            else if (keepDataOption != (int)KeepDataStatus.Delete && keepDataOption != (int)KeepDataStatus.Remove && keepDataOption != (int)KeepDataStatus.Vault)
            {
                return RMContentDisposalAction.KeepData;
            }
            else
            {
                return RMContentDisposalAction.Remove;
            }
        }*/

        public List<PolicyLevel> GetRuleLevels(Dictionary<Guid, RMRuleItemCollection> mTermAndRulesMapping)
        {
            List<PolicyLevel> levels = new List<PolicyLevel>();
            foreach (var ruleItemCollection in mTermAndRulesMapping.Values)
            {
                RuleCollection commonRules = ruleItemCollection.CommonRules;
                foreach (Rule rule in commonRules.Rules.Values)
                {
                    if (!levels.Contains(rule.PolicyLevel))
                    {
                        levels.Add(rule.PolicyLevel);
                    }
                }
            }
            return levels;
        }
        public bool CheckHasLowLevelRule(List<PolicyLevel> levels, PolicyLevel curLevel)
        {
            bool isHasLowLevelRule = false;
            List<PolicyLevel> lowLevels = levels.Where(l => (int)l > (int)curLevel).ToList();
            if (lowLevels.Count > 0)
            {
                isHasLowLevelRule = true;
            }
            return isHasLowLevelRule;
        }
        public async Task<List<TermTreeNode>> GetRATermTreeNodesAsync()
        {
            List<TermTreeNode> groupNodes = new List<TermTreeNode>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup(false);
            foreach (var group in termGroups)
            {
                TermTreeNode groupNode = new TermTreeNode()
                {
                    ID = group.UniqueId,
                    Children = new Dictionary<Guid, TermTreeNode>()
                };
                List<RMTermSet> allRMTermSet = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in allRMTermSet)
                {
                    TermTreeNode termSetNode = TermDao.GetRATermSetTree(termSet.UniqueId);
                    if (termSetNode != null)
                    {
                        termSetNode.ParentID = group.UniqueId;
                        groupNode.Children.Add(termSetNode.ID, termSetNode);
                    }
                }
                groupNodes.Add(groupNode);
            }

            return groupNodes;
        }
        /// <summary>
        /// 获取RA Term Tree 包含移除的Term
        /// </summary>
        /// <returns></returns>
        public async Task<List<TermTreeNode>> GetRATermTreeNodeOfOrphanedTermAsync()
        {
            List<TermTreeNode> groupNodes = new List<TermTreeNode>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup();
            foreach (var group in termGroups)
            {
                TermTreeNode groupNode = new TermTreeNode()
                {
                    ID = group.UniqueId,
                    Children = new Dictionary<Guid, TermTreeNode>()
                };
                List<RMTermSet> allRMTermSet = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in allRMTermSet)
                {
                    TermTreeNode termSetNode = TermDao.GetRATermSetTreeOfOrphanedTerm(termSet.UniqueId);
                    if (termSetNode != null)
                    {
                        termSetNode.ParentID = group.UniqueId;
                        groupNode.Children.Add(termSetNode.ID, termSetNode);
                    }
                }
                groupNodes.Add(groupNode);
            }

            return groupNodes;
        }
        public string GetMetaDataColumnName(Guid webAppId)
        {
            return SPSettingDao.GetMedataColumn(webAppId);
        }
        /// <summary>
        /// 获取RM中的OrphanedTerms
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<Guid, RMTermIdentity>> GetOrphanedTermsOfRMAsync()
        {
            Dictionary<Guid, RMTermIdentity> termIdEntity = new Dictionary<Guid, RMTermIdentity>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup();
            foreach (var group in termGroups)
            {
                List<RMTermSet> newTermSets = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in newTermSets)
                {
                    List<RMTerm> orphanedTerms = TermDao.GetOrphanedTerms(termSet.Id);
                    foreach (var term in orphanedTerms)
                    {
                        var identity = new RMTermIdentity()
                        {
                            UniqueId = term.UniqueId,
                            Name = term.Name,
                            FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId),
                            Status = GetOrphanedAndRetiredTermStatus(term)
                        };
                        termIdEntity.Add(term.UniqueId, identity);
                    }
                }
            }
            return termIdEntity;
        }
        /// <summary>
        /// 获取OrphanedTerm的状态 remove和Deprecated都显示成Retired
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private RMTermStatus GetOrphanedAndRetiredTermStatus(RMTerm term)
        {
            RMTermStatus status = RMTermStatus.Retired;
            if (term.IsRemoved)
            {
                status = RMTermStatus.Removed;
            }
            else if (term.IsDeprecated)
            {
                status = RMTermStatus.Retired;
            }
            else
            {
                RMTerm returnTerm = TermDao.GetTermTimeSettings(term.Id);
                long utcNow = DateTime.UtcNow.Ticks;
                if (returnTerm.TermExpirationFrom > 0 && utcNow < returnTerm.TermExpirationFrom)
                {
                    status = RMTermStatus.Retired;
                }
                if (returnTerm.TermExpirationTo > 0 && utcNow > returnTerm.TermExpirationTo)
                {
                    status = RMTermStatus.Retired;
                }
            }
            return status;
        }
        public async Task<Dictionary<Guid, RMTermIdentity>> GetRetiredTermsOfRMAsync()
        {
            Dictionary<Guid, RMTermIdentity> termIdEntity = new Dictionary<Guid, RMTermIdentity>();
            List<RMTermGroup> termGroups = TermGroupDao.LoadTermGroup();
            foreach (var group in termGroups)
            {
                List<RMTermSet> newTermSets = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                foreach (RMTermSet termSet in newTermSets)
                {
                    List<RMTerm> retiredTerms = TermDao.GetretiredTerms(termSet.Id);
                    foreach (var term in retiredTerms)
                    {
                        var identity = new RMTermIdentity()
                        {
                            UniqueId = term.UniqueId,
                            Name = term.Name,
                            FullPath = TermDao.GetTermNamesPathByTermId(term.UniqueId),
                            Status = GetOrphanedAndRetiredTermStatus(term)
                        };
                        termIdEntity.Add(term.UniqueId, identity);
                    }
                }
            }
            return termIdEntity;
        }
        #region Discover term for report
        public async Task<Dictionary<Guid, RMTermIdentity>> GetTermIDsFromBCSTermTreeAsync(string ext1)
        {
            Dictionary<Guid, RMTermIdentity> termIdEntity = new Dictionary<Guid, RMTermIdentity>();
            try
            {
                Dictionary<int, RMTermDto> termDic = JsonConvert.DeserializeObject<Dictionary<int, RMTermDto>>(ext1);
                List<Guid> needDelTermIds = new List<Guid>();
                logger.Info("Begin build RMTermSet Tree for BCS Term Usage Report.");
                List<RMTermGroup> termGroup = TermGroupDao.LoadTermGroup(false);
                foreach (var group in termGroup)
                {
                    List<RMTermSet> newTermSets = await TermSetDAO.LoadTermSetAsync(TermSetType.Business, group.UniqueId);
                    if (newTermSets.Count == 0)
                    {
                        logger.Warn("There is no RMTermSet in RMDB. group name:{0} ", group.Name);
                        continue;
                        //throw new Exception("There is no RMTermSet in RMDB.");
                    }
                    //assembly TermSet with term
                    foreach (RMTermSet termSet in newTermSets)
                    {
                        var termSetPath = group.Name + "/" + termSet.Name;
                        List<RMTerm> allTerm = TermDao.GetTermFromTermSetWithoutDeletedTerm(termSet.Id);
                        List<RMTermDto> terms = new List<RMTermDto>();
                        RMTermDto termSetDto = null;
                        //只会有一个TermSet所以取第一个
                        if (termDic.ContainsKey(-termSet.Id))
                        {
                            termSetDto = termDic[-termSet.Id];
                            if (termDic.Count == 1 && termSetDto.IsChecked)
                            {
                                //只勾选TermSet
                                DiscoverAllTerm(termSetPath, RMTermStatus.Avaliable, allTerm, termDic, ref termIdEntity);
                            }
                            else
                            {
                                DiscoverTerm(termSetPath, termSetDto, RMTermStatus.Avaliable, allTerm, termDic, ref termIdEntity);
                            }
                        }
                        //else
                        //{
                        //    throw new Exception("no term cache.");
                        //}
                        //cache need remove orphan term ids
                        List<RMTerm> orphanedTerms = TermDao.GetOrphanedTerms(termSet.Id);
                        if (orphanedTerms != null && orphanedTerms.Count > 0)
                        {
                            foreach (var term in termIdEntity.Values)
                            {
                                if (orphanedTerms.Where(t => t.UniqueId.Equals(term.UniqueId)).FirstOrDefault() != null)
                                {
                                    needDelTermIds.Add(term.UniqueId);
                                }
                            }
                        }

                        List<RMTerm> retiredTerms = TermDao.GetretiredTerms(termSet.Id);
                        if (retiredTerms != null && retiredTerms.Count > 0)
                        {
                            foreach (var term in termIdEntity.Values)
                            {
                                if (retiredTerms.Where(t => t.UniqueId.Equals(term.UniqueId)).FirstOrDefault() != null)
                                {
                                    needDelTermIds.Add(term.UniqueId);
                                }
                            }
                        }
                    }
                }
                logger.Info("build RMTermSet Tree for BCS Term Usage Report Complete.");

                if (termIdEntity == null || termIdEntity.Count == 0)
                {
                    throw new Exception("no term cache.");
                }
                //remove orphan term
                foreach (var id in needDelTermIds)
                {
                    if (termIdEntity.ContainsKey(id))
                    {
                        termIdEntity.Remove(id);
                    }
                }
                return termIdEntity;
            }
            catch (Exception e)
            {
                logger.Error("There are some error in build RMTermSet Tree for BCS Term Usage Report,ERROR: {0}", e.ToString());
                throw;
            }

        }

        private RMTermStatus GetTermStatus(RMTerm term, RMTermStatus parentStatus)
        {
            RMTermStatus status = RMTermStatus.Avaliable;
            if (term.IsDeprecated)
            {
                status = RMTermStatus.Retired;
            }
            else if (term.IsRemoved)
            {
                status = RMTermStatus.Removed;
            }
            else if (term.TermExpirationFrom > 0 || term.TermExpirationTo > 0)
            {
                long utcNow = DateTime.UtcNow.Ticks;
                if (term.TermExpirationFrom > 0 && utcNow < term.TermExpirationFrom)
                {
                    status = RMTermStatus.Retired;
                }
                if (term.TermExpirationTo > 0 && utcNow > term.TermExpirationTo)
                {
                    status = RMTermStatus.Retired;
                }
            }
            else if (term.BreakInheritFromParent && !(term.TermExpirationFrom > 0 || term.TermExpirationTo > 0))
            {
                status = RMTermStatus.Avaliable;
            }
            else if (!parentStatus.Equals(RMTermStatus.Retired))
            {
                status = parentStatus;
            }

            return status;
        }

        private void DiscoverTerm(string parentTermPath, RMTermDto parentDto, RMTermStatus parentStatus, List<RMTerm> subTerms, Dictionary<int, RMTermDto> termDic, ref Dictionary<Guid, RMTermIdentity> termIdEntity)
        {
            bool selectAll = parentDto.IsChecked && parentDto.IsLeafNode;

            foreach (RMTerm subTerm in subTerms)
            {
                string termFullPath = parentTermPath + "/" + subTerm.Name;
                List<RMTerm> allSubTerm = TermDao.GetTermFromParentTermWithoutDeletedTerm(subTerm.Id);
                RMTermDto subTermDto;

                if (selectAll)
                {
                    var indentity = new RMTermIdentity()
                    {
                        UniqueId = subTerm.UniqueId,
                        Name = subTerm.Name,
                        FullPath = termFullPath,
                        Status = GetTermStatus(subTerm, parentStatus)
                    };
                    termIdEntity.Add(subTerm.UniqueId, indentity);
                    DiscoverAllTerm(termFullPath, indentity.Status, allSubTerm, termDic, ref termIdEntity);
                }
                else if (termDic.TryGetValue(subTerm.Id, out subTermDto))
                {
                    if (subTermDto.IsChecked)
                    {
                        var indentity = new RMTermIdentity()
                        {
                            UniqueId = subTerm.UniqueId,
                            Name = subTerm.Name,
                            FullPath = termFullPath,
                            Status = GetTermStatus(subTerm, parentStatus)
                        };
                        termIdEntity.Add(subTerm.UniqueId, indentity);
                        DiscoverTerm(termFullPath, subTermDto, indentity.Status, allSubTerm, termDic, ref termIdEntity);
                    }
                    else if (!subTermDto.IsLeafNode)//没有勾选该节点,但节点已load过,需要check子节点勾选情况
                    {
                        DiscoverTerm(termFullPath, subTermDto, GetTermStatus(subTerm, parentStatus), allSubTerm, termDic, ref termIdEntity);
                    }
                }
            }
        }

        private void DiscoverAllTerm(string parentTermPath, RMTermStatus parentStatus, List<RMTerm> subTerms, Dictionary<int, RMTermDto> termDic, ref Dictionary<Guid, RMTermIdentity> termIdEntity)
        {
            foreach (RMTerm subTerm in subTerms)
            {
                string termFullPath = parentTermPath + "/" + subTerm.Name;
                var identity = new RMTermIdentity()
                {
                    UniqueId = subTerm.UniqueId,
                    Name = subTerm.Name,
                    FullPath = termFullPath,
                    Status = GetTermStatus(subTerm, parentStatus)
                };
                termIdEntity.Add(subTerm.UniqueId, identity);
                List<RMTerm> allSubTerm = TermDao.GetTermFromParentTermWithoutDeletedTerm(subTerm.Id);
                DiscoverAllTerm(termFullPath, identity.Status, allSubTerm, termDic, ref termIdEntity);
            }

        }


        #endregion

    }
}
