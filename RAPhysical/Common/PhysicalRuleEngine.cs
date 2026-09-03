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
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.TemplateManagement;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RAPhysical.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.Common
{
    public class PhysicalRuleEngine
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private const string HOLDSTATUS = "HoldStatus";
        private const string DESTURL = "DestinationUrl";
        private static string delimiter = ((Char)0x12).ToString();
        public static string HoldStatusKey => $"{HOLDSTATUS}{delimiter}";
        public static string MoveToDestUrlKey =>$"{DESTURL}{delimiter}";
        private readonly List<Rule> _rules;
        public List<FilterPolicy> FilterPolicyCollection { get; private set; }
        private int RuleLevelNumber { get; set; }
        public bool HasPhysicalBoxCondition { get; private set; }
        public bool HasPhysicalFileCondition { get; private set; }

        public PhysicalRuleEngine(List<Rule> mRules)
        {
            var rules =  CloneRules(mRules);
            var physicalRules = rules.Where(r => r.PhysicalRule != null).ToList();
            physicalRules.ForEach(rule =>
            {
                rule.PhysicalRule.Filters = ConvertSOFilterPolicysToFilterPolicys(rule.PhysicalRule.SOFilters);
                int sequenceNo = rule.PhysicalRule.Filters.Count;
                int sequenceNoBeforeAppendFilter = sequenceNo;
                int appendFilterCount = 0;
                if (rule.PhysicalRule.spMoveOption != null && rule.PhysicalRule.spMoveOption.MoveDestination != null)
                {
                    sequenceNo++;
                    appendFilterCount++;
                    var boxId = rule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.BoxId;
                    var destId = string.IsNullOrEmpty(boxId) ? rule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.LocationId : boxId;
                    logger.Info($"destId : {destId}, boxid:{boxId}, location id : {rule.PhysicalRule.spMoveOption.MoveDestination.PhysicalTree.LocationId}.");
                    rule.PhysicalRule.Filters.Add(new FilterPolicy()
                    {
                        Condition = PolicyCondition.IsExactlyNot,
                        Level = rule.PhysicalRule.PolicyLevel,
                        Value = new PolicyValue(destId),
                        //Rule = new UrlRule() { Value1 = "URL" },
                        Rule = new ColumnTextRule() { Value1 = MoveToDestUrlKey },
                        SequenceNo = sequenceNo,
                    });
                }
                //else
                //{
                //    sequenceNo++;
                //    appendFilterCount++;
                //    rule.PhysicalRule.Filters.Add(new FilterPolicy
                //    {
                //        Condition = PolicyCondition.Exactly,
                //        Level = rule.PhysicalRule.PolicyLevel,
                //        Value = new PolicyValue("no"),
                //        Rule = new ColumnBooleanRule() { Value1 = HoldStatusKey },
                //        SequenceNo = sequenceNo,
                //    });
                //}
                var andOrExpress = GetAndOrExpress(rule.PhysicalRule.AndOrExpression, rule.PhysicalRule.PolicyLevel, sequenceNoBeforeAppendFilter, appendFilterCount);
                rule.PhysicalRule.AndOrExpression = andOrExpress;
            });
            _rules = physicalRules;

            if (_rules != null)
            {
                foreach (var rule in _rules)
                {
                    if (!HasPhysicalBoxCondition)
                    {
                        HasPhysicalBoxCondition |= rule.PhysicalRule.PolicyLevel == PolicyLevel.PhysicalBox;
                    }
                    if (!HasPhysicalFileCondition)
                    {
                        HasPhysicalFileCondition |= rule.PhysicalRule.PolicyLevel == PolicyLevel.PhysicalFile;
                    }
                }
            }
            //此处顺序不可以调整，应该为从低到高判断，来决定最低级别Rule 的level
            if (HasPhysicalFileCondition)
            {
                RuleLevelNumber = (int)PolicyLevel.PhysicalFile;
                MergeFilterPolicy();
            }
            if (HasPhysicalBoxCondition)
            {
                RuleLevelNumber = (int)PolicyLevel.PhysicalBox;
                MergeFilterPolicy();
            }
        }

        public bool HasLowerLevelRule(int cacheNodeType)
        {
            return cacheNodeType < RuleLevelNumber;
        }

        public Rule CheckRule(ObjectInfoBase obj)
        {
            Rule result = null;
            if (_rules == null)
            {
                return null;
            }

            result = CheckCriteria(obj);
            return result;
        }
        public Rule GetRuleFromRuleCollectionByRuleId(string ruleId)
        {
            return _rules.Where(a => a.Id == ruleId)?.FirstOrDefault();
        }
        public Rule CheckRule(Record record, Dictionary<Guid, TemplateColumnDto> columnCollection)
        {
            if (_rules == null) { return null; }
            Rule result = null;
            ObjectInfoBase baseInfo = null;
            if (record.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                baseInfo = PhysicalObjectConvertor.ConvertPhysicalBoxFilterObject(FilterPolicyCollection, record, columnCollection);
            }
            else if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
            {
                baseInfo = PhysicalObjectConvertor.ConvertPhysicalFileFilterObject(FilterPolicyCollection, record, columnCollection);
            }
            result = CheckCriteria(baseInfo);
            return result;
        }

        public Rule CheckDueDisposalRule(Record record, Dictionary<Guid, TemplateColumnDto> columnCollection, ref string dueDisposalTime)
        {
            if (_rules == null) { return null; }
            DateTime resutlTime = DateTime.MinValue;
            Rule result = null;
            PolicyLevel policyLevel = PolicyLevel.None;
            ObjectInfoBase baseInfo = null;
            if (record.NodeType == (int)RMNodeLevel.PhysicalBox)
            {
                policyLevel = PolicyLevel.PhysicalBox;
                baseInfo = PhysicalObjectConvertor.ConvertPhysicalBoxFilterObject(FilterPolicyCollection, record, columnCollection);
            }
            else if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
            {
                policyLevel = PolicyLevel.PhysicalFile;
                baseInfo = PhysicalObjectConvertor.ConvertPhysicalFileFilterObject(FilterPolicyCollection, record, columnCollection);
            }
            var curRules = _rules.Where(r => r.PhysicalRule.PolicyLevel == policyLevel && r.PhysicalRule.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));

            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;
                //Record 里面只有all 和any， 所以有一个or 表示是any
                //进入这个判断表示只有一个older than 的条件，所以早晚会符合rule，只是时间问题
                if (rule.PhysicalRule.AndOrExpression[rule.PhysicalRule.PolicyLevel].Contains("Or") || rule.PhysicalRule.Filters.Count == 1)
                {
                    foreach (var filter in rule.PhysicalRule.Filters)
                    {
                        var timeValue = GetDueDate(filter, record, baseInfo);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckDueDateCriteria(baseInfo, rule);
                    if (r != null)
                    {
                        foreach (var filter in rule.PhysicalRule.Filters)
                        {
                            var timeValue = GetDueDate(filter, record, baseInfo);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        result = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return result;
        }

        public Rule CheckDueDisposalRule(IPhysicalBox box, ObjectInfoBase baseInfo, ref string dueDisposalTime)
        {
            if (_rules == null) { return null; }
            DateTime resutlTime = DateTime.MinValue;
            Rule result = null;
            PolicyLevel policyLevel = PolicyLevel.PhysicalBox;
            var curRules = _rules.Where(r => r.PhysicalRule.PolicyLevel == policyLevel && r.PhysicalRule.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));

            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;
                //Record 里面只有all 和any， 所以有一个or 表示是any
                //进入这个判断表示只有一个older than 的条件，所以早晚会符合rule，只是时间问题
                if (rule.PhysicalRule.AndOrExpression[rule.PhysicalRule.PolicyLevel].Contains("Or") || rule.PhysicalRule.Filters.Count == 1)
                {
                    foreach (var filter in rule.PhysicalRule.Filters)
                    {
                        var timeValue = GetDueDate(filter, box, baseInfo);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckDueDateCriteria(baseInfo, rule);
                    if (r != null)
                    {
                        foreach (var filter in rule.PhysicalRule.Filters)
                        {
                            var timeValue = GetDueDate(filter, box, baseInfo);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        result = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return result;
        }

        public Rule CheckDueDisposalRule(IPhysicalFile physicalFile, ObjectInfoBase baseInfo, ref string dueDisposalTime)
        {
            if (_rules == null) { return null; }
            DateTime resutlTime = DateTime.MinValue;
            Rule result = null;
            PolicyLevel policyLevel = PolicyLevel.PhysicalFile;
            var curRules = _rules.Where(r => r.PhysicalRule.PolicyLevel == policyLevel && r.PhysicalRule.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));

            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;
                //Record 里面只有all 和any， 所以有一个or 表示是any
                //进入这个判断表示只有一个older than 的条件，所以早晚会符合rule，只是时间问题
                if (rule.PhysicalRule.AndOrExpression[rule.PhysicalRule.PolicyLevel].Contains("Or") || rule.PhysicalRule.Filters.Count == 1)
                {
                    foreach (var filter in rule.PhysicalRule.Filters)
                    {
                        var timeValue = GetDueDate(filter, physicalFile, baseInfo);
                        if (timeValue != DateTime.MinValue)
                        {
                            if (disposalTime == DateTime.MinValue || disposalTime > timeValue)// or关系取due date最小值
                            {
                                disposalTime = timeValue;
                                tempRule = rule;
                            }
                        }
                    }
                }
                else
                {
                    var r = CheckDueDateCriteria(baseInfo, rule);
                    if (r != null)
                    {
                        foreach (var filter in rule.PhysicalRule.Filters)
                        {
                            var timeValue = GetDueDate(filter, physicalFile, baseInfo);
                            if (timeValue != DateTime.MinValue)
                            {
                                if (disposalTime == DateTime.MinValue || disposalTime < timeValue)// and关系取due date最大值
                                {
                                    disposalTime = timeValue;
                                    tempRule = rule;
                                }
                            }
                        }
                    }
                }
                //多个rule取due date最小值
                if (disposalTime != DateTime.MinValue)
                {
                    if (resutlTime == DateTime.MinValue || resutlTime > disposalTime)
                    {
                        resutlTime = disposalTime;
                        result = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return result;
        }

        private Rule CheckDueDateCriteria(ObjectInfoBase obj, Rule rule)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is PhysicalBoxInfo)
            {
                var tempObj = obj as PhysicalBoxInfo;
                tempObj.Created = DateTime.MinValue;
                tempObj.Modified = DateTime.MinValue;
                tempObj.LastestFolderDisposalDueDate = DateTime.MinValue;
                rule.PhysicalRule.Filters.Where(f => f.Rule is ColumnDateTimeRule && f.Condition == PolicyCondition.OlderThan)
                    .ToList()
                    .ForEach(f =>
                    {
                        var columnName = f.Value.Value1;
                        if (tempObj.ColumnInfos.ContainsKey(columnName))
                        {
                            tempObj.ColumnInfos[columnName] = DateTime.MinValue;
                        }
                    });
            }
            else if (obj is PhysicalFileInfo)
            {
                var tempObj = obj as PhysicalFileInfo;
                tempObj.Created = DateTime.MinValue;
                tempObj.Modified = DateTime.MinValue;
                rule.PhysicalRule.Filters.Where(f => f.Rule is ColumnDateTimeRule && f.Condition == PolicyCondition.OlderThan)
                   .ToList()
                   .ForEach(f =>
                   {
                       var columnName = f.Value.Value1;
                       if (tempObj.ColumnInfos.ContainsKey(columnName))
                       {
                           tempObj.ColumnInfos[columnName] = DateTime.MinValue;
                       }
                   });
            }
            else
            {
                return null;
            }
            return this.CheckCurrentCriteria(obj, rule, skipCheckDateTimeMinValue: true);
        }

        private Rule CheckCurrentCriteria(ObjectInfoBase info, Rule rulet, int thresholdRuleOrder = -1, bool skipCheckDateTimeMinValue = false)
        {
            if (-1 == thresholdRuleOrder || rulet.PhysicalRule.Order < thresholdRuleOrder)
            {
                try
                {
                    //我们需要filter out模式
                    var engine = new FilterEngine(rulet.PhysicalRule.Filters, rulet.PhysicalRule.AndOrExpression, true, skipCheckDateTimeMinValue);
                    if (engine.IsQualified(info))
                    {
                        return rulet;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is PropertyNotAssignedException)
                    {
                        logger.Error("A property was not assigned while checking a physical rule. Exception:{0}", ex.ToString());
                    }
                    throw new Exception(ex.ToString());
                }
            }
            else if (rulet.Order == thresholdRuleOrder)
            {
                return rulet;
            }
            return null;
        }

        private DateTime GetDueDate(FilterPolicy filter, Record record, ObjectInfoBase baseInfo)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }

            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule || filter.Rule is ColumnDateTimeRule || filter.Rule is LastestFolderDisposalDueDateRule)
            {
                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = new DateTime(record.TimeModified);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = new DateTime(record.TimeCreated);
                }
                else if (filter.Rule is ColumnDateTimeRule)
                {
                    try
                    {
                        if (record.NodeType == (int)RMNodeLevel.PhysicalBox)
                        {
                            PhysicalBoxInfo boxFilterInfo = baseInfo as PhysicalBoxInfo;
                            if (boxFilterInfo.ColumnInfos.ContainsKey(filter.Rule.Value1))
                            {
                                var value = boxFilterInfo.ColumnInfos[filter.Rule.Value1].ToString();
                                if (!DateTime.TryParse(value, out timeValue))
                                {
                                    logger.Warn($"Cannot convert value {value}. to date time format.");
                                    //return DateTime.MinValue;
                                }
                            }
                        }
                        else if (record.NodeType == (int)RMNodeLevel.PhysicalFile)
                        {
                            PhysicalFileInfo folderFilterInfo = baseInfo as PhysicalFileInfo;
                            if (folderFilterInfo.ColumnInfos.ContainsKey(filter.Rule.Value1))
                            {
                                var value = folderFilterInfo.ColumnInfos[filter.Rule.Value1].ToString();
                                if (!DateTime.TryParse(value, out timeValue))
                                {
                                    logger.Warn($"Cannot convert value {value}. to date time format.");
                                    //return DateTime.MinValue;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Cannot get column : {filter.Value.Value1} from record : {record.LeafName}, {record.Id}.  error:{ex.Message}");
                    }
                }
                else if (filter.Rule is LastestFolderDisposalDueDateRule)
                {
                    timeValue = new DateTime(PhysicalObjectConvertor.GetLastestFolderDisposalDueDateRuleUnderBox(new PhysicalBox(record)));
                }
                #endregion
                //the forecase only work for older than condition, timeValue == DateTime.MinValue means rule is wrong or column is wrong
                if (timeValue != DateTime.MinValue && filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }
            }
            return timeValue;
        }

        private DateTime GetDueDate(FilterPolicy filter, IPhysicalBox physicalBox, ObjectInfoBase baseInfo)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }

            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule || filter.Rule is ColumnDateTimeRule || filter.Rule is LastestFolderDisposalDueDateRule)
            {
                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = new DateTime(physicalBox.ModifiedTimeTicks);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = new DateTime(physicalBox.CreateTimeTicks);
                }
                else if (filter.Rule is ColumnDateTimeRule)
                {
                    try
                    {
                        var boxFilterInfo = baseInfo as PhysicalBoxInfo;
                        if (boxFilterInfo.ColumnInfos.ContainsKey(filter.Rule.Value1))
                        {
                            var value = boxFilterInfo.ColumnInfos[filter.Rule.Value1].ToString();
                            if (!DateTime.TryParse(value, out timeValue))
                            {
                                logger.Warn($"Cannot convert value {value}. to date time format.");
                                return DateTime.MinValue;
                            }
                        }
                        else
                        {
                            return DateTime.MinValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Cannot get column : {filter.Rule.Value1} from record : {physicalBox.Name}. reason : {ex.ToString()}");
                    }
                }
                else if (filter.Rule is LastestFolderDisposalDueDateRule)
                {
                    var boxFilterInfo = baseInfo as PhysicalBoxInfo;
                    if (boxFilterInfo.LastestFolderDisposalDueDate != DateTime.MinValue)
                    {
                        timeValue = boxFilterInfo.LastestFolderDisposalDueDate;
                    }
                    else
                    {
                        return DateTime.MinValue;
                    }
                }
                #endregion
                //the forecase only work for older than condition
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }
            }
            return timeValue;
        }

        private DateTime GetDueDate(FilterPolicy filter, IPhysicalFile physicalFileInfo, ObjectInfoBase baseInfo)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }

            if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule || filter.Rule is ColumnDateTimeRule)
            {
                #region get time value
                if (filter.Rule is ModifiedRule)
                {
                    timeValue = new DateTime(physicalFileInfo.ModifiedTimeTicks);
                }
                else if (filter.Rule is CreatedRule)
                {
                    timeValue = new DateTime(physicalFileInfo.CreateTimeTicks);
                }
                else if (filter.Rule is ColumnDateTimeRule)
                {
                    try
                    {
                        var FileFilterInfo = baseInfo as PhysicalFileInfo;
                        if (FileFilterInfo.ColumnInfos.ContainsKey(filter.Rule.Value1))
                        {
                            var value = FileFilterInfo.ColumnInfos[filter.Rule.Value1].ToString();
                            if (!DateTime.TryParse(value, out timeValue))
                            {
                                logger.Warn($"Cannot convert value {value}. to date time format.");
                                return DateTime.MinValue;
                            }
                        }
                        else
                        {
                            return DateTime.MinValue;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Cannot get column : {filter.Rule.Value1} from record : {physicalFileInfo.Name}. reason : {ex.ToString()}");
                    }
                }
                #endregion
                //the forecase only work for older than condition
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    int num;
                    #region calculate time
                    if (int.TryParse(filter.Value.Value1, out num))
                    {
                        if (filter.Value.Value1Unit == PolicyValueUnit.Days)
                        {
                            timeValue = timeValue.AddDays(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                        {
                            timeValue = timeValue.AddDays(num * 7);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                        {
                            timeValue = timeValue.AddMonths(num);
                        }
                        else if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                        {
                            timeValue = timeValue.AddYears(num);
                        }
                    }
                    #endregion
                }
            }
            return timeValue;
        }

        private Rule CheckCriteria(ObjectInfoBase info)
        {
            //already sorted when querying the mappings from the db
            //var rules = _rules.OrderBy(t => t.Order);
            foreach (var rule in _rules)
            {
                try
                {
                    if (rule.PhysicalRule == null || rule.PhysicalRule.Filters == null || rule.PhysicalRule.AndOrExpression == null)
                    {
                        continue;
                    }

                    var engine = new FilterEngine( rule.PhysicalRule.Filters, rule.PhysicalRule.AndOrExpression, true );
                    if (engine.IsQualified(info))
                    {
                        return rule;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Checked expression failed. Expression:{0} ,Exception:{1}", rule.PhysicalRule.Compression, ex.ToString());
                    throw new Exception(string.Format("Checked expression failed.{0}", rule.PhysicalRule.Compression), ex);
                }
            }
            return null;
        }

        private void MergeFilterPolicy()
        {
            FilterPolicyCollection = new List<FilterPolicy>();
            foreach (var filterPolicy in _rules.SelectMany(rule => rule.PhysicalRule.Filters))
            {
                FilterPolicyCollection.Add(filterPolicy);
            }
        }

        private List<Rule> CloneRules(List<Rule> rules)
        {
            string xml = SerializerHelper.SerializeByDataContractSerializer(rules);
            List<Rule> result = SerializerHelper.DeserializeByDataContractSerializer<List<Rule>>(xml);
            return result;
        }

        public List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> ConvertSOFilterPolicysToFilterPolicys(List<SOFilterPolicy> soFilterPolicys)
        {
            List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy> result = new List<AvePoint.GCommon.Contract.CommonFilter.FilterPolicy>();
            if (soFilterPolicys != null && soFilterPolicys.Count > 0)
            {
                foreach (SOFilterPolicy soFilterPolicy in soFilterPolicys)
                {
                    result.Add(ConvertSOFilterPolicyToFilterPolicy(soFilterPolicy));
                }
            }
            return result;
        }

        public AvePoint.GCommon.Contract.CommonFilter.FilterPolicy ConvertSOFilterPolicyToFilterPolicy(SOFilterPolicy soFilterPolicy)
        {
            if (soFilterPolicy == null)
            {
                return null;
            }
            AvePoint.GCommon.Contract.CommonFilter.FilterPolicy result = new AvePoint.GCommon.Contract.CommonFilter.FilterPolicy();
            result.Rule = soFilterPolicy.Rule;
            result.Condition = soFilterPolicy.Condition;
            result.Value = soFilterPolicy.Value;
            result.Level = soFilterPolicy.Level;
            result.SequenceNo = soFilterPolicy.SequenceNo;
            return result;
        }

        /// <summary>
        /// 此方法提供拼接AndOrExpress 功能,对于有拼装Filter 的case，需要借助此方法重新拼接And or 关系
        /// </summary>
        /// <param name="andOrExpress">原始AndOrExpress， 支持原始为空的情况，即原始Filter 为空的情况进行拼接</param>
        /// <param name="policyLevel">Rule 的Policy Level</param>
        /// <param name="sequenceNoBeforeAppendFilter">拼接之前的SequenceNumber， 也就是拼接之前Filter 的Count</param>
        /// <param name="appendCount">额外拼接Filter的个数</param>
        /// <returns></returns>
        private Dictionary<PolicyLevel, string> GetAndOrExpress(Dictionary<PolicyLevel, string> andOrExpress, PolicyLevel policyLevel, int sequenceNoBeforeAppendFilter, int appendCount)
        {
            Dictionary<PolicyLevel, string> andOrValue = andOrExpress;
            if (andOrValue == null)
            {
                andOrValue = new Dictionary<PolicyLevel, string>();
                andOrValue.Add(policyLevel, "(1)");
                sequenceNoBeforeAppendFilter += 1;
                for (int i = 0; i < appendCount - 1; i++)
                {
                    sequenceNoBeforeAppendFilter++;
                    andOrValue[policyLevel] = "(" + andOrValue[policyLevel] + "and" + sequenceNoBeforeAppendFilter.ToString() + ")";
                }
            }
            else
            {
                for (int i = 0; i < appendCount; i++)
                {
                    sequenceNoBeforeAppendFilter++;
                    andOrValue[policyLevel] = "(" + andOrValue[policyLevel] + "and" + sequenceNoBeforeAppendFilter.ToString() + ")";
                }
            }
            return andOrValue;
        }

        #region  add for the requirement for Feature to Prescan Rule.
        //public Tuple<Rule, TimeSpan> MatchPotentialRule(ObjectInfoBase obj)
        //{

        //    var rule = CheckCriteria(obj);
        //    if (rule != null)
        //    {
        //        //directly matched
        //        return new Tuple<Rule, TimeSpan>(rule, default(TimeSpan));
        //    }
        //    else
        //    {
        //        var potentialRules = _rules.Where(t => t.PhysicalRule != null && t.PhysicalRule.Filters.Any(f => f.Condition == AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.OlderThan)).OrderBy(t => t.Order).ToList();
        //        List<TimeSpan> offsets = ComputeOffsets(obj, potentialRules);
        //        foreach (var offset in offsets)
        //        {
        //            var tObj = ObjectConverter.CloneFilterObject(obj, offset);
        //            foreach (var pr in potentialRules)
        //            {
        //                try
        //                {
        //                    var engine = new FilterEngine(pr.PhysicalRule.Filters, pr.PhysicalRule.AndOrExpression, true);
        //                    if (engine.IsQualified(tObj))
        //                    {
        //                        return new Tuple<Rule, TimeSpan>(pr, offset);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    logger.Error("Checked expression failed. Expression:{0} ,Exception:{1}", rule.Compression, ex.ToString());
        //                    throw new Exception(string.Format("Checked expression failed.{0}", rule.Compression), ex);
        //                }
        //            }
        //        }
        //        return null;
        //    }
        //}

        //private List<TimeSpan> ComputeOffsets(ObjectInfoBase obj, List<Rule> potentialRules)
        //{
        //    var now = DateTime.UtcNow;
        //    List<TimeSpan> offsets = new List<TimeSpan>();
        //    List<DateTime> leftMargins = new List<DateTime>();
        //    foreach (var r in potentialRules)
        //    {
        //        foreach (var filter in r.PhysicalRule.Filters)
        //        {
        //            if (filter.Condition == PolicyCondition.OlderThan)
        //            {
        //                var value = int.Parse(filter.Value.Value1);
        //                if (filter.Value.Value1Unit == PolicyValueUnit.Years)
        //                {
        //                    leftMargins.Add(now.AddYears(0 - value));
        //                }
        //                else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
        //                {
        //                    leftMargins.Add(now.AddMonths(0 - value));
        //                }
        //                else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
        //                {
        //                    leftMargins.Add(now.AddDays(0 - value * 7));
        //                }
        //                else
        //                {
        //                    leftMargins.Add(now.AddDays(0 - value));
        //                }
        //            }
        //        }
        //    }


        //    foreach (var leftMargin in leftMargins.Distinct())
        //    {
        //        if (obj is IPhysicalBox)
        //        {
        //            var box = obj as IPhysicalBox;
        //            if (box.Created >= leftMargin)
        //            {
        //                offsets.Add(box.Created - leftMargin);
        //            }
        //            if (box.Modified >= leftMargin)
        //            {
        //                offsets.Add(box.Modified - leftMargin);
        //            }
        //        }
        //        else if (obj is IPhysicalFile)
        //        {
        //            var file = obj as IPhysicalFile;

        //            if (file.Created >= leftMargin)
        //            {
        //                offsets.Add(file.Created - leftMargin);
        //            }
        //            if (file.Modified >= leftMargin)
        //            {
        //                offsets.Add(file.Modified - leftMargin);
        //            }
        //        }
        //    }
        //    offsets = offsets.Distinct().ToList();
        //    offsets.Sort();
        //    logger.Debug("offsets:{0}", string.Concat<TimeSpan>(offsets));
        //    return offsets;
        //}
        #endregion
    }
}
