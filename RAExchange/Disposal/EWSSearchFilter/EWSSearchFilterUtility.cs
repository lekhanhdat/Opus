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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rule = AvePoint.GCommon.Contract.StorageOptimization.Object.Rule;
using RuleCollection = AvePoint.GCommon.Contract.StorageOptimization.Object.RuleCollection;

namespace AvePoint.RA.RAExchange.Disposal
{
    public class EWSSearchFilterUtility
    {
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RuleCollection mRuleCollection;
        private List<Rule> exoRules = null;
        private bool isNullTermClassification = false;
        private bool isSupportGraphApi = false;
        private static Guid WellKnowTermColumnGuid = new Guid("AA44DC13-6491-40C8-8C4C-5FE81370EFF3");
        private static int WellKnowTermColumnId = 0xF666;
        private static string EmailRegexString = @"^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.(com|cn|net)$";
        public bool HasUnSupportCriteria { get; }
        public SearchFilter SearchFilter { get; }

        public EWSSearchFilterUtility(RuleCollection sheduleRuleCollection, bool isNullTermClassification = false, bool isSupportGraphApi = false)
        {

            mRuleCollection = sheduleRuleCollection;
            this.isNullTermClassification = isNullTermClassification;
            this.isSupportGraphApi = isSupportGraphApi;
            //获取当前所有EXO Rule
            exoRules = mRuleCollection.Rules.Select(rulet => rulet.Value).Where(rulet => rulet.PolicyLevel == PolicyLevel.ExchangeOnlineItem).ToList();
            HasUnSupportCriteria = !isSupportGraphApi ? CheckHasUnSupportCriteria() : CheckHasUnSupportCriteriaForGraphApi();
            if (CheckExistCompositeOperatorAndOr())
            {
                mLog.Info("Current job does not has unsupport AndOrExpression");
                HasUnSupportCriteria = true;
            }
            if (!HasUnSupportCriteria)
            {
                mLog.Info("Current job does not has unsupport rule and ConvertAllRulesToSearchFilter.exoRules count:{0}.", exoRules.Count);
                SearchFilter = ConvertAllRulesToSearchFilter();
            }
            else
            {
                mLog.Info("Current job has unsupport rule and use full query to discover objects.exoRules count:{0}.", exoRules.Count);
            }
        }

        private bool CheckExistCompositeOperatorAndOr()
        {
            bool hasUnSupportCriteria = false;
            foreach (Rule rule in exoRules)
            {
                bool useAndExpression = rule.EXORule.AndOrExpression.FirstOrDefault().Value.Contains("And");
                bool useOrExpression = rule.EXORule.AndOrExpression.FirstOrDefault().Value.Contains("Or");
                if (useAndExpression && useOrExpression)
                {
                    mLog.Info($"Current rule has unsupport AndOrExpression.RuleName:{rule.Name}, AndOrExpression: {rule.EXORule.AndOrExpression.FirstOrDefault().Value}");
                    hasUnSupportCriteria = true;
                }
            }
            return hasUnSupportCriteria;
        }

        /// <summary>
        /// 1.Subject的Match/DoesNotMatch不支持
        /// 2.Retention Label不支持
        /// 4.Send From只支持EmailAddress，如果填写的是非EmailAddress格式，需要走Full Query.
        /// 5.Send To只支持Display Name，如果填写的是EmailAddress格式，需要走Full Query.
        /// 6.Sensitivity Label不支持
        /// </summary>
        /// <returns></returns>
        private bool CheckHasUnSupportCriteria()
        {
            bool hasUnSupportCriteria = false;
            foreach (Rule rule in exoRules)
            {
                if (rule.EXORule != null)
                {
                    /// 1.Subject的Match/DoesNotMatch不支持
                    int retentionLabelRuleCount = rule.EXORule.SOFilters.Where(x => x.Rule is RetentionLabelRule).Count();
                    if (retentionLabelRuleCount > 0)
                    {
                        hasUnSupportCriteria = true;
                        mLog.Info("Current rule has unsupport rule:RetentionLabelRule.RuleName:{0}.", rule.Name);
                        break;
                    }
                    /// 2.Retention Label不支持
                    int subjectUnSupportCondition = rule.EXORule.SOFilters.Where(x => (x.Condition == PolicyCondition.DoesNotMatch || x.Condition == PolicyCondition.Match)).Count();
                    if (subjectUnSupportCondition > 0)
                    {
                        hasUnSupportCriteria = true;
                        mLog.Info("Current rule has unsupport condition:Match or DoesNotMatch.RuleName:{0}.", rule.Name);
                        break;
                    }

                    if (rule.EXORule.SOFilters.Any(x => x.Rule is AttachmentRule && x.Condition == PolicyCondition.LessOrEqualThan))
                    {
                        hasUnSupportCriteria = true;
                        mLog.Info("Current rule has unsupport rule:AttachmentRule LessOrEqualThan.RuleName:{0}.", rule.Name);
                        break;
                    }

                    /// 4.Send From只支持EmailAddress，如果填写的是非EmailAddress格式，需要走Full Query.
                    var sendFromRuleResult = rule.EXORule.SOFilters.Where(x => x.Rule is SendFromRule).ToList();
                    foreach (var sendFromRule in sendFromRuleResult)
                    {
                        if (!new Regex(EmailRegexString).IsMatch(sendFromRule.Value.Value1))
                        {
                            hasUnSupportCriteria = true;
                            mLog.Info("Current rule has unsupport condition:SendFromRule does not fit EmailRegexString.RuleName:{0}.", rule.Name);
                            break;
                        }
                    }
                    /// 5.Send To只支持Display Name，如果填写的是EmailAddress格式，需要走Full Query.
                    var sendToRuleResult = rule.EXORule.SOFilters.Where(x => x.Rule is SendToRule).ToList();
                    foreach (var sendToRule in sendToRuleResult)
                    {
                        if (new Regex(EmailRegexString).IsMatch(sendToRule.Value.Value1))
                        {
                            hasUnSupportCriteria = true;
                            mLog.Info("Current rule has unsupport condition:SendToRule contains EmailRegexString.RuleName:{0}.", rule.Name);
                            break;
                        }
                    }
                    /// 6.SensitivityLabelRule不支持
                    if (rule.EXORule.SOFilters.Any(x => x.Rule is SensitivityLabelRule))
                    {
                        hasUnSupportCriteria = true;
                        mLog.Info("Current rule has unsupport rule:SensitivityLabelRule.RuleName:{0}.", rule.Name);
                        break;
                    }
                }
                else
                {
                    mLog.Info("Current rule EXORule is null.RuleName:{0}.", rule.Name);
                }

            }
            return hasUnSupportCriteria;
        }

        private bool CheckHasUnSupportCriteriaForGraphApi()
        {
            bool hasUnSupportCriteria = false;
            foreach (Rule rule in exoRules)
            {
                if (rule.EXORule != null)
                {
                    // Subject's Match/DoesNotMatch does not support
                    int subjectUnSupportCondition = rule.EXORule.SOFilters.Where(x => (x.Condition == PolicyCondition.DoesNotMatch || x.Condition == PolicyCondition.Match)).Count();
                    if (subjectUnSupportCondition > 0)
                    {
                        hasUnSupportCriteria = true;
                        mLog.Info("Current rule has unsupport condition:Match or DoesNotMatch.RuleName:{0}.", rule.Name);
                        break;
                    }
                    if (rule.EXORule.SOFilters.Any(x => x.Rule is SendToRule))
                    {
                        hasUnSupportCriteria = true;
                        mLog.Info("Current rule has unsupport rule:SendToRule.RuleName:{0}.", rule.Name);
                        break;
                    }
                }    
                else
                {
                    mLog.Info("Current rule EXORule is null.RuleName:{0}.", rule.Name);
                }
            }
            return hasUnSupportCriteria;
        }

        /// <summary>
        /// 1.Convert所有Rule的Criteria为Search Filter
        /// 2.Rule和Rule之间使用LogicalOperator.Or
        /// 3.Criteria和Criteria之间根据AndOrExpression决定
        /// </summary>
        /// <returns></returns>
        public SearchFilter ConvertAllRulesToSearchFilter()
        {
            SearchFilter combinedAllRulesSearchFilter = null;
            List<SearchFilter> combinedAllRulesSearchFilterCollection = new List<SearchFilter>();
            foreach (Rule rule in exoRules)
            {
                SearchFilter currentRuleSearchFilter = null;
                List<SearchFilter> currentRuleSearchFilterCollection = new List<SearchFilter>();
                foreach (SOFilterPolicy soFilter in rule.EXORule.SOFilters)
                {
                    SearchFilter currentSOFilterPolicySearchFilter = ConvertSOFilterPolicyToSearchFilter(soFilter, rule.Name);
                    currentRuleSearchFilterCollection.Add(currentSOFilterPolicySearchFilter);
                }
                currentRuleSearchFilter = CombinedCurrentRuleSearchFilter(rule, currentRuleSearchFilterCollection);
                combinedAllRulesSearchFilterCollection.Add(currentRuleSearchFilter);
            }
            if (ArchiverCommonStaticMethod.IsNestleCustomizeSearchFilter && ArchiverCommonStaticMethod.NestleCustomizeSearchFilterDays > 0)
            {
                mLog.Info("this is IsNestleCustomizeSearchFilter and NestleCustomizeSearchFilterDays>0,will use this filter");
                combinedAllRulesSearchFilter = GetNestleCustomizeSearchFilter(ArchiverCommonStaticMethod.NestleCustomizeSearchFilterDays);
            }
            else if (combinedAllRulesSearchFilterCollection.Count == 1)
            {
                combinedAllRulesSearchFilter = combinedAllRulesSearchFilterCollection.FirstOrDefault();
            }
            else
            {
                combinedAllRulesSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, combinedAllRulesSearchFilterCollection.ToArray());
            }

            return combinedAllRulesSearchFilter;
        }
        private SearchFilter GetNestleCustomizeSearchFilter(int olderThanMonth)
        {
            SearchFilter currentSOFilterPolicySearchFilter = null;
            //DateTime olderThanMonths = DateTime.UtcNow.AddMonths(0 - olderThanMonth);
            DateTime olderThanMonths = DateTime.UtcNow.AddDays(0 - olderThanMonth);
            //mLog.Info($"GetNestleCustomizeSearchFilter.olderThanMonths:{olderThanMonths}.");
            currentSOFilterPolicySearchFilter = new SearchFilter.IsLessThan(ItemSchema.DateTimeSent, olderThanMonths);
            return currentSOFilterPolicySearchFilter;
        }
        /// <summary>
        /// 拼接当前Rule的所有Search Filter(包括Null Term Classification的两种情况)
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="currentRuleSearchFilterCollection"></param>
        /// <returns></returns>
        private SearchFilter CombinedCurrentRuleSearchFilter(Rule rule, List<SearchFilter> currentRuleSearchFilterCollection)
        {
            SearchFilter currentRuleSearchFilter = null;
            bool useAndExpression = rule.EXORule.AndOrExpression.FirstOrDefault().Value.Contains("And");
            bool useOrExpression = rule.EXORule.AndOrExpression.FirstOrDefault().Value.Contains("Or");
            if (isNullTermClassification)
            {
                if (useAndExpression)
                {
                    currentRuleSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, currentRuleSearchFilterCollection.ToArray());
                }
                else if (useOrExpression)
                {
                    currentRuleSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, currentRuleSearchFilterCollection.ToArray());
                }
                else
                {
                    //没有and和or，证明只有一个Criteria
                    currentRuleSearchFilter = currentRuleSearchFilterCollection.FirstOrDefault();
                }
            }
            else
            {
                //非Null Term Classification需要把Term property存在条件加进去
                ExtendedPropertyDefinition extendedPropertyDefinition = new ExtendedPropertyDefinition(WellKnowTermColumnGuid, WellKnowTermColumnId, MapiPropertyType.String);
                SearchFilter termSearchFilter = new SearchFilter.Exists(extendedPropertyDefinition);
                if (useAndExpression)
                {
                    currentRuleSearchFilterCollection.Add(termSearchFilter);
                    currentRuleSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, currentRuleSearchFilterCollection.ToArray());
                }
                else if (useOrExpression)
                {
                    var tempFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.Or, currentRuleSearchFilterCollection.ToArray());
                    var tempFiterList = new List<SearchFilter>();
                    tempFiterList.Add(tempFilter);
                    tempFiterList.Add(termSearchFilter);
                    currentRuleSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, tempFiterList.ToArray());
                }
                else
                {
                    //没有and和or，证明只有一个Criteria
                    currentRuleSearchFilterCollection.Add(termSearchFilter);
                    currentRuleSearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, currentRuleSearchFilterCollection.ToArray());
                }
            }

            return currentRuleSearchFilter;
        }

        private SearchFilter ConvertSOFilterPolicyToSearchFilter(SOFilterPolicy soFilter, string ruleName)
        {
            SearchFilter currentSOFilterPolicySearchFilter = null;
            if (soFilter.Rule is SubjectRule)
            {
                switch (soFilter.Condition)
                {
                    case PolicyCondition.Contains:
                        currentSOFilterPolicySearchFilter = new SearchFilter.ContainsSubstring(ItemSchema.Subject, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.Equals:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsEqualTo(ItemSchema.Subject, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.DoesNotContains:
                        SearchFilter searchFilter = new SearchFilter.ContainsSubstring(ItemSchema.Subject, soFilter.Value.Value1);
                        currentSOFilterPolicySearchFilter = new SearchFilter.Not(searchFilter);
                        break;
                    case PolicyCondition.IsExactlyNot:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsNotEqualTo(ItemSchema.Subject, soFilter.Value.Value1);
                        break;
                    default:
                        break;
                }
            }
            else if (soFilter.Rule is AttachmentRule)
            {
                switch (soFilter.Condition)
                {
                    case PolicyCondition.GreaterOrEqualThan:
                    case PolicyCondition.LessOrEqualThan:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsEqualTo(ItemSchema.HasAttachments, true);
                        break;
                    default:
                        break;
                }
            }
            else if (soFilter.Rule is SendToRule)
            {
                var propDefinition = isSupportGraphApi
                    ? EmailMessageSchema.ReceivedBy
                    : ItemSchema.DisplayTo;
                switch (soFilter.Condition)
                {
                    case PolicyCondition.Contains:
                        currentSOFilterPolicySearchFilter = new SearchFilter.ContainsSubstring(propDefinition, soFilter.Value.Value1);
                        break;
                    default:
                        break;
                }
            }
            else if (soFilter.Rule is SendFromRule)
            {
                switch (soFilter.Condition)
                {
                    case PolicyCondition.Contains:
                        currentSOFilterPolicySearchFilter = new SearchFilter.ContainsSubstring(EmailMessageSchema.From, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.Equals:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsEqualTo(EmailMessageSchema.From, soFilter.Value.Value1);
                        break;
                    default:
                        break;
                }
            }
            else if (soFilter.Rule is SizeRule)
            {
                long criteria = long.Parse(soFilter.Value.Value1);
                switch (soFilter.Value.Value1Unit)
                {
                    case PolicyValueUnit.KB:
                        criteria *= 1024;
                        break;
                    case PolicyValueUnit.MB:
                        criteria *= 1024 * 1024;
                        break;
                    case PolicyValueUnit.GB:
                        criteria *= 1024 * 1024 * 1024;
                        break;
                    default:
                        break;
                }
                switch (soFilter.Condition)
                {
                    case PolicyCondition.GreaterOrEqualThan:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.Size, criteria);
                        break;
                    case PolicyCondition.LessOrEqualThan:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsLessThanOrEqualTo(ItemSchema.Size, criteria);
                        break;
                    default:
                        break;
                }
            }
            else if (soFilter.Rule is SendDateUTCRule)
            {
                DateTime policyDateTimeValue1;
                DateTime policyDateTimeValue2;
                int dayWeekMonthYear;
                switch (soFilter.Condition)
                {
                    case PolicyCondition.FromTo:
                        policyDateTimeValue1 = DateTime.Parse(soFilter.Value.Value1);
                        policyDateTimeValue1 = DateTime.SpecifyKind(policyDateTimeValue1, DateTimeKind.Utc);
                        policyDateTimeValue2 = DateTime.Parse(soFilter.Value.Value2);
                        policyDateTimeValue2 = DateTime.SpecifyKind(policyDateTimeValue2, DateTimeKind.Utc);
                        // Add a search filter that searches on the DateTimeSent.
                        List<SearchFilter> searchFilterCollection = new List<SearchFilter>();
                        searchFilterCollection.Add(new SearchFilter.IsGreaterThanOrEqualTo(ItemSchema.DateTimeSent, policyDateTimeValue1));
                        searchFilterCollection.Add(new SearchFilter.IsLessThanOrEqualTo(ItemSchema.DateTimeSent, policyDateTimeValue2));

                        // Create the search filter.
                        currentSOFilterPolicySearchFilter = new SearchFilter.SearchFilterCollection(LogicalOperator.And, searchFilterCollection.ToArray());
                        break;
                    case PolicyCondition.Before:
                        policyDateTimeValue1 = DateTime.Parse(soFilter.Value.Value1);
                        policyDateTimeValue1 = DateTime.SpecifyKind(policyDateTimeValue1, DateTimeKind.Utc);
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsLessThan(ItemSchema.DateTimeSent, policyDateTimeValue1);
                        break;
                    case PolicyCondition.OlderThan:
                        DateTime olderThanDays = DateTime.UtcNow;
                        switch (soFilter.Value.Value1Unit)
                        {
                            case PolicyValueUnit.Days:
                                dayWeekMonthYear = int.Parse(soFilter.Value.Value1);
                                olderThanDays = DateTime.UtcNow.AddDays(0 - dayWeekMonthYear);
                                break;
                            case PolicyValueUnit.Weeks:
                                dayWeekMonthYear = int.Parse(soFilter.Value.Value1);
                                olderThanDays = DateTime.UtcNow.AddDays(0 - dayWeekMonthYear * 7);
                                break;
                            case PolicyValueUnit.Months:
                                dayWeekMonthYear = int.Parse(soFilter.Value.Value1);
                                olderThanDays = DateTime.UtcNow.AddMonths(0 - dayWeekMonthYear);
                                break;
                            case PolicyValueUnit.Years:
                                dayWeekMonthYear = int.Parse(soFilter.Value.Value1);
                                olderThanDays = DateTime.UtcNow.AddYears(0 - dayWeekMonthYear);
                                break;
                            default:
                                break;

                        }
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsLessThan(ItemSchema.DateTimeSent, olderThanDays);
                        break;
                    default:
                        break;
                }
            }
            else if (isSupportGraphApi && soFilter.Rule is RetentionLabelRule)
            {
                switch (soFilter.Condition)
                {
                    case PolicyCondition.Equals:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsEqualTo(ItemSchema.PolicyTag, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.IsExactlyNot:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsNotEqualTo(ItemSchema.PolicyTag, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.IsEmpty:
                        var searchFilter = new SearchFilter.Exists(ItemSchema.PolicyTag);
                        currentSOFilterPolicySearchFilter = new SearchFilter.Not(searchFilter);
                        break;
                    default:
                        break;
                }
            }
            else if (isSupportGraphApi && soFilter.Rule is SensitivityLabelRule)
            {
                switch (soFilter.Condition)
                {
                    case PolicyCondition.Equals:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsEqualTo(ItemSchema.Sensitivity, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.IsExactlyNot:
                        currentSOFilterPolicySearchFilter = new SearchFilter.IsNotEqualTo(ItemSchema.Sensitivity, soFilter.Value.Value1);
                        break;
                    case PolicyCondition.IsEmpty:
                        var searchFilter = new SearchFilter.Exists(ItemSchema.Sensitivity);
                        currentSOFilterPolicySearchFilter = new SearchFilter.Not(searchFilter);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //Term rule or Retention rule.
                mLog.Info("Current Criteria is does not support.RuleName:{0}.Criteria:{1}.", ruleName, soFilter.Rule.ToString());
            }
            return currentSOFilterPolicySearchFilter;
        }
    }
}
