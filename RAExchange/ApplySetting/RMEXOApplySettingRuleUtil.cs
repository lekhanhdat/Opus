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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.ApplySetting
{
    public class RMEXOApplySettingRuleUtil
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RMEXOApplySettingRuleUtil));
        public static RuleCollection GetRuleCollection(List<ClassificationRule> autoRules, ref Dictionary<string, Guid> termRuleMapping)
        {
            List<Rule> rules = new List<Rule>();
            List<SOFilterPolicy> soFilters;
            foreach (var autoRule in autoRules)
            {
                //目前只有一个message 的category，所以只有一个default rule，以后会存在多个，需要写兼容逻辑
                if (autoRule.IsDefaultRule)
                {
                    if (autoRule.NoDefaultTerm)
                    {
                        termRuleMapping.Add(Guid.Empty.ToString(), Guid.Empty);
                    }
                    else
                    {
                        termRuleMapping.Add(Guid.Empty.ToString(), new Guid(autoRule.TermId));
                    }
                }
                else
                {
                    soFilters = new List<SOFilterPolicy>();
                    int sequenceNo = 0;
                    ConvertToSOFilters(autoRule.FilterGroups, ref sequenceNo, ref soFilters);
                    List<FilterPolicy> filerPolicies = ConvertSOFiletrPolicyToFilterPolicy(soFilters);
                    string andOrExpressionStr = GetGroupsAndOrExpression(autoRule.FilterGroups, ArchiverFilterCombineMode.And);
                    logger.Info("AndOr Expression:{0}", andOrExpressionStr);
                    Rule soRule = ConvertToSORule(autoRule, soFilters, filerPolicies, andOrExpressionStr);
                    rules.Add(soRule);

                    termRuleMapping.Add(soRule.Id, new Guid(autoRule.TermId));
                }
            }

            RuleCollection ruleCol = new RuleCollection() { Rules = new Dictionary<int, Rule>() };
            for (int i = 0; i < rules.Count; i++)
            {
                ruleCol.Rules.Add(i, rules[i]);
            }
            return ruleCol;
        }

        public static string GetGroupAndOrExpression(FilterGroup filterGroup)
        {
            string groupAndOrExpression = string.Empty;

            string filtersExpression = GetFiltersAndOrExpression(filterGroup.Filters);
            groupAndOrExpression = filtersExpression;

            if (filterGroup.FilterGroups != null && filterGroup.FilterGroups.Count > 0)
            {
                string groupsResult = GetGroupsAndOrExpression(filterGroup.FilterGroups, filterGroup.CombineMode);
                groupAndOrExpression += " " + filterGroup.CombineMode.ToString() + " " + groupsResult;
            }

            if (filterGroup.Filters.Count == 1 && filterGroup.FilterGroups.Count == 0)
            {
                //do nothing
            }
            else
            {
                groupAndOrExpression = "(" + groupAndOrExpression + ")";
            }
            return groupAndOrExpression;
        }

        public static string GetGroupsAndOrExpression(List<FilterGroup> filterGroups, ArchiverFilterCombineMode combineMode)
        {
            string result = string.Empty;
            for (int i = 0; i < filterGroups.Count; i++)
            {
                string groupResult = GetGroupAndOrExpression(filterGroups[i]);
                if (i == 0)
                {
                    result = groupResult;
                }
                else
                {
                    result += " " + combineMode.ToString() + " " + groupResult;
                }
            }
            return result;
        }

        public static List<FilterPolicy> ConvertSOFiletrPolicyToFilterPolicy(List<SOFilterPolicy> soFilters)
        {
            List<FilterPolicy> filerPolicies = new List<FilterPolicy>();
            foreach (var filter in soFilters)
            {
                FilterPolicy filterPolicy = new FilterPolicy();
                if (filter.Condition == PolicyCondition.Exactly || filter.Condition == PolicyCondition.Equals)
                {
                    filterPolicy.Condition = PolicyCondition.Equals;
                }
                else
                {
                    filterPolicy.Condition = filter.Condition;
                }
                filterPolicy.Level = filter.Level;
                filterPolicy.Rule = filter.Rule;
                filterPolicy.RuleType = filter.RuleType;
                filterPolicy.SequenceNo = filter.SequenceNo;
                filterPolicy.Value = filter.Value;

                filerPolicies.Add(filterPolicy);
            }
            return filerPolicies;
        }

        public static Rule ConvertToSORule(ClassificationRule autoRule, List<SOFilterPolicy> soFilters, List<FilterPolicy> filerPolicies, string andOrStr)
        {
            Rule rule = new Rule();
            rule.Id = Guid.NewGuid().ToString();
            rule.SOFilters = soFilters;
            rule.Filters = filerPolicies;
            rule.PolicyLevel = (PolicyLevel)autoRule.RuleLevel;
            rule.Order = autoRule.RuleOrder;
            rule.ProfileType = AvePoint.GCommon.Contract.Server.Common.Profile.Object.ProfileType.ArchiverRule;
            rule.IncludeNew = "1";
            //rule.AndOrExpression = GetAndOrExpression(soFilters, autoRule.RuleLevel);
            rule.AndOrExpression = new Dictionary<PolicyLevel, string>() { { (PolicyLevel)filerPolicies.FirstOrDefault()?.Level, andOrStr } };
            return rule;
        }
        public static string GetFiltersAndOrExpression(List<RuleFilter> filters)
        {
            //string AndOrExpression = "(";
            string AndOrExpression = string.Empty;
            for (int i = 0; i < filters.Count; i++)
            {
                RuleFilter filter = filters[i];
                if (i == filters.Count - 1)
                {
                    AndOrExpression += string.Format("{0}", filter.SequenceNo);
                }
                else
                {
                    AndOrExpression += string.Format("{0} {1} ", filter.SequenceNo, filter.CombineMode == ArchiverFilterCombineMode.And ? "And" : "Or");
                }
            }
            //AndOrExpression += ")";
            return AndOrExpression;
        }

        public static void ConvertToSOFilters(List<FilterGroup> filterGroups, ref int sequenceNo, ref List<SOFilterPolicy> soFilters)
        {
            foreach (var filterGroup in filterGroups)
            {
                foreach (var raFilter in filterGroup.Filters)
                {
                    sequenceNo++;
                    SOFilterPolicy soFilter = BuildSOFilter(raFilter, sequenceNo);
                    soFilters.Add(soFilter);
                }
                ConvertToSOFilters(filterGroup.FilterGroups, ref sequenceNo, ref soFilters);
            }
        }

        public static SOFilterPolicy BuildSOFilter(RuleFilter filter, int sequenceNo)
        {
            ArchiverRuleFilter arFilter = new ArchiverRuleFilter();
            arFilter.CombineMode = filter.CombineMode;
            //arFilter.SequenceNo = filter.SequenceNo;
            arFilter.SequenceNo = sequenceNo;
            arFilter.Level = (PolicyLevel)filter.Level;
            arFilter.Condition = filter.Condition;
            arFilter.RuleType = filter.RuleType;
            if (!string.IsNullOrEmpty(filter.filterName))
            {
                arFilter.RuleName = filter.filterName;
            }
            //arFilter.Dto.Rule = arFilter.RuleBase;
            if (arFilter.RuleType == ArchiverFilterRuleType.ModifiedTime || arFilter.RuleType == ArchiverFilterRuleType.CreatedTime
         || arFilter.RuleType == ArchiverFilterRuleType.LastAccessedTime || arFilter.RuleType == ArchiverFilterRuleType.DateTimeColumn || arFilter.RuleType == ArchiverFilterRuleType.DateTimeCustomProperty
         || arFilter.RuleType == ArchiverFilterRuleType.SendDateUTC)
            {
                string startDayLightSaving = filter.StartTimeInfo == null ? "true" : filter.StartTimeInfo.IsDayLightSaving.ToString();
                string endDayLightSaving = filter.EndTimeInfo == null ? "true" : filter.EndTimeInfo.IsDayLightSaving.ToString();
                if (arFilter.Condition == ArchiverFilterCondition.FromTo)
                {

                    DateTime startUtcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    DateTime endUtcTime = arFilter.SetDateTime(filter.Value2, filter.EndTimeInfo.TimeZoneId, endDayLightSaving, true);
                    if (DateTime.Parse(filter.Value1) >= DateTime.Parse(filter.Value2))
                    {
                        //throw new InvalidArgumentException(Messages.Get("start_date_after_end_date"));
                        throw new Exception("");
                    }
                    arFilter.Value1 = startUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                    arFilter.Value2 = endUtcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.Before)
                {
                    // ValidateValueCount(value, 3);
                    DateTime utcTime = arFilter.SetDateTime(filter.Value1, filter.StartTimeInfo.TimeZoneId, startDayLightSaving, false);
                    arFilter.Value1 = utcTime.ToString(APIDateTimeFormat.DATETYPEForAPI003);
                }
                else if (arFilter.Condition == ArchiverFilterCondition.OlderThan)
                {
                    //ValidateValueCount(value, 1);
                    //SetValueForOlderThan(value[0]);
                    arFilter.Value1 = filter.Value1;
                    arFilter.Value1Unit = filter.Value1Unit;
                }
            }
            else
            {
                arFilter.Value1 = filter.Value1;
                if (filter.RuleType == ArchiverFilterRuleType.DocumentSize || filter.RuleType == ArchiverFilterRuleType.SiteCollectionSizeTrigger
                    || filter.RuleType == ArchiverFilterRuleType.Size)
                {
                    arFilter.Value1Unit = filter.Value1Unit;
                    arFilter.Value2Unit = filter.Value2Unit;
                }
                arFilter.Value2 = filter.Value2;
            }
            return arFilter.Dto;
        }
    }
}
