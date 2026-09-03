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
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;

using AvePoint.GCommon;
using System.Reflection;
using AvePoint.Wrapper.Common;
using System.Collections;
using System.Text;
using System.Globalization;
using LOGRESOURCE = Merged18NResources.Archive.Archive;
using LOGRESOURCEnew = Merged18NResources.Common;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract;
using AvePoint.Wrapper.Discovery;
using AvePoint.RA.CommonUtil;

namespace AvePoint.RA.SharePoint.ArchiverCommon
{
    /// <summary>
    /// backup check rule, only has IAveObject, doen't have IAveDiscoverObject. So copy Common Wrapper(records method) to Archiver.
    /// All条件下：
    ///1. 查看Rule条件里是否有时间条件，并且是Older than，如果是走2，否则走3
    ///2. 将当前时间改为无穷大（或者create time/Modified time改成无穷小）然后check rule，如果符合Rule，则根据数值计算Due date；如果不符合Rule，那么Due Date为空。
    ///3. 直接check rule，符合条件的话，next job，不符合条件的话，空。
    ///Any条件下：
    ///a.看是否有时间条件并且older than
    ///b.Check Rule，
    ///如果b为true，则next job；如果b为false，a为true，则计算Due date；如果b为fasle，a也false，则Due date为空。
    ///对于多个Rule的check 逻辑，通用的check逻辑是，如果是含有时间相关并且是older than的，需要修改按时间让Older than符合rule。在有多个Rule符合条件的情况下，则采用发生时间最短的。
    ///对于Archiver之后，重新Check Rule的逻辑为：仍然从上到下Check，如果含有时间并且older than的rule，和上面一样，通过修改时间让其满足Rule。一直Check到当前Archiver执行的Rule
    ///。如果当前的Rule仍然符合，则不继续向下Check，并且，从比这个Rule的order小的Rule里面找时间最短的，认为是Next action rule。如果这个Rule的order已经是1（最小），那么Due Date就是NULL。
    ///如果当前的Rule不符合，则继续Check比这个Rule的Order大的Rule，然后所有符合的Rule里，找时间最短的。
    /// </summary>
    public class RecordsCalculateDisposalDueDate
    {
        #region private member

        private static RALogger mLog = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly RuleCollection mRuleCollection;
        private ObjectInfoBase baseInfo = null;

        #endregion

        #region property

        public bool HasDocumentCondition { get; private set; }
        public bool HasAttachmentCondition { get; private set; }
        public bool HasDocVersionCondition { get; private set; }
        public bool HasItemVersionCondition { get; private set; }
        public bool HasItemCondition { get; private set; }

        public bool HasFolderCondition { get; private set; }
        public bool HasListCondition { get; private set; }
        public bool HasListFilterCondition { get; private set; }
        public bool HasSiteCondition { get; private set; }
        public bool HasSiteFilterCondition { get; private set; }
        public bool HasSiteCollectionCondition { get; private set; }
        public bool HasSiteCollectionFilterCondition { get; private set; }
        public int RuleLevelNumber { get; private set; }
        private List<FilterPolicy> FilterPolicyCollection { get; set; }

        #endregion property

        public RecordsCalculateDisposalDueDate(RuleCollection sheduleRuleCollection, string jobId = "")
        {
            mRuleCollection = sheduleRuleCollection;
            //Set WrapperConfiguration.UseStubAccessTimeRule Value false ADO-117596
            WrapperConfiguration.UseStubAccessTimeRule = false;

            #region find all conditions type.

            if (mRuleCollection != null)
                foreach (var rule in mRuleCollection.Rules.Select(rulet => rulet.Value))
                {
                    if (!HasAttachmentCondition)
                    {
                        HasAttachmentCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Attachment) != null);
                    }
                    if (!HasDocumentCondition)
                    {
                        HasDocumentCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Document) != null);
                    }
                    if (!HasDocVersionCondition)
                    {
                        HasDocVersionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.DocumentVersion) != null);
                        try
                        {
                            mLog.Info("HasDocVersionCondition:{0}", HasDocVersionCondition);
                        }
                        catch (Exception e)
                        {
                            mLog.Info(e.ToString());
                        }
                    }
                    if (!HasItemVersionCondition)
                    {
                        HasItemVersionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.ItemVersion) != null);
                    }
                    if (!HasItemCondition)
                    {
                        HasItemCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Item) != null);
                    }
                    if (!HasItemCondition)
                    {
                        HasItemCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Newsfeed) != null);
                    }
                    if (!HasFolderCondition)
                    {
                        HasFolderCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Folder) != null);
                    }
                    if (!HasListCondition)
                    {
                        HasListCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.List) != null);
                    }
                    if (!HasSiteCondition)
                    {
                        HasSiteCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.Site) != null);
                    }
                    if (!HasSiteCollectionCondition)
                    {
                        HasSiteCollectionCondition |= (rule.Filters.FirstOrDefault(tmp => tmp.Level == PolicyLevel.SiteCollection) != null);
                    }
                    if (HasDocVersionCondition && HasAttachmentCondition && HasDocumentCondition
                        && HasSiteCollectionCondition && HasSiteCondition && HasListCondition && HasItemCondition && HasItemVersionCondition)
                    {
                        break;
                    }
                }
            if (HasItemCondition || HasAttachmentCondition || HasDocVersionCondition || HasItemVersionCondition || HasDocumentCondition)
            {//为判断是否有低级别rule
                RuleLevelNumber = (int)CacheNodeType.Item;
                MergeFilterPolicy();
                return;
            }
            if (HasFolderCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.Folder;
                MergeFilterPolicy();
                return;
            }
            if (HasListCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.List;
                MergeFilterPolicy();
                return;
            }
            if (HasSiteCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.Web;
                MergeFilterPolicy();
                return;
            }
            if (HasSiteCollectionCondition)
            {
                RuleLevelNumber = (int)CacheNodeType.SiteCollection;
                MergeFilterPolicy();
                return;
            }
            #endregion

        }

        private void MergeFilterPolicy()
        {
            FilterPolicyCollection = new List<FilterPolicy>();
            var filterPolicyType = new List<Type>();
            foreach (var filterPolicy in mRuleCollection.Rules.Values.SelectMany(rule => rule.Filters))
            {
                FilterPolicyCollection.Add(filterPolicy);
                filterPolicyType.Add(filterPolicy.Rule.GetType());
                //if (!filterPolicyType.Contains(filterPolicy.Rule.GetType()))
                //{由于同样的rule不同level会产生错误，暂时先注掉，以后解决此问题

                //}
            }
        }

        private FilterPolicy CloneFilterPolicy(FilterPolicy policy, int count)
        {
            FilterPolicy tempFilter = new FilterPolicy();
            tempFilter.Condition = policy.Condition;
            tempFilter.Level = policy.Level;
            tempFilter.Result = policy.Result;
            tempFilter.Rule = policy.Rule;
            tempFilter.RuleType = policy.RuleType;
            tempFilter.SequenceNo = count + 1;
            tempFilter.Value = policy.Value;
            return tempFilter;
        }

        /// <summary>
        /// 多个rule check逻辑
        /// 对于多个Rule的check 逻辑，通用的check逻辑是，如果是含有时间相关并且是older than的，需要修改按时间让Older than符合rule。在有多个Rule符合条件的情况下，则采用发生时间最短的。
        /// 对于Archiver之后，重新Check Rule的逻辑为：仍然从上到下Check，如果含有时间并且older than的rule，和上面一样，通过修改时间让其满足Rule。一直Check到当前Archiver执行的Rule。如果当前的Rule仍然符合，则不继续向下Check，并且，从比这个Rule的order小的Rule里面找时间最短的，认为是Next action rule。如果这个Rule的order已经是1（最小），那么Due Date就是NULL。如果当前的Rule不符合，则继续Check比这个Rule的Order大的Rule，然后所有符合的Rule里，找时间最短的。
        /// 多个criteria check逻辑
        /// All条件下：
        /// 1. 查看Rule条件里是否有时间条件，并且是Older than，如果是走2，否则走3
        /// 2. 将当前时间改为无穷大（或者create time/Modified time改成无穷小）然后check rule，如果符合Rule，则根据数值计算Due date；如果不符合Rule，那么Due Date为空。
        /// 3. 直接check rule，符合条件的话，next job，不符合条件的话，空。
        /// Any条件下：
        /// a.看是否有时间条件并且older than
        /// b.Check Rule，
        /// 如果b为true，则next job；如果b为false，a为true，则计算Due date；如果b为fasle，a也false，则Due date为空。
        /// case描述：
        ///有多个Rule。
        ///开始符合Rule2，但是在不经过任何改动的情况下（也没有跑Disposal Job），又再次符合Rule1。比如Rule2是Older than 1个月，rule1是Older than2个月。
        ///针对于这种情况，目前暂时不作处理，正常情况下客户应该在符合Rule2之后，符合Rule1之前，应该跑Disposal Job，跑了job之后会重新Check Rule，就不会存在不一致的问题。
        ///以后如果真有这样的客户需求，则可以在跑Collection Job的时候，把过期的数据重新Check一下Rule。过期的意思是说，当前时间大于了Due date。但是鉴于这种case并不常见，并且这种重新Check的动作也比较耗费资源，所以等有需求的时候再处理。
        /// </summary>
        public Rule GetDueDisposalRule(IAveListItem aveItem, ref long dueDisposalTime, int currentRuleOrder = -1)
        {
            Dictionary<Rule, DateTime> duedates = new Dictionary<Rule, DateTime>();
            Rule resultRule = null;
            PolicyLevel policyLevel = PolicyLevel.Item;
            if (aveItem.ParentList.BaseType == AveBaseType.DocumentLibrary)
            {
                policyLevel = PolicyLevel.Document;
            }
            foreach (var rule in mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == policyLevel))
            {
                bool isAndExpression = rule.AndOrExpression.FirstOrDefault().Value.IndexOf(") And (") != -1 && rule.AndOrExpression.FirstOrDefault().Value.Substring(0, rule.AndOrExpression.FirstOrDefault().Value.IndexOf(") And (")).Contains("And");
                DateTime disposalTime = isAndExpression ? DateTime.MinValue : DateTime.MaxValue;
                mLog.Info("Current rule name:{0}.rule AndOrExpression value:{1}.rule filter count:{2}.", rule.Name, rule.AndOrExpression.FirstOrDefault().Value, rule.Filters.Count);
                //if (rule.AndOrExpression.FirstOrDefault().Value.Contains("Or") || rule.Filters.Count == 1)
                {
                    //如果当前的Rule仍然符合，则不继续向下Check，并且，从比这个Rule的order小的Rule里面找时间最短的，认为是Next action rule。
                    //如果这个Rule的order已经是1（最小），那么Due Date就是NULL。如果当前的Rule不符合，则继续Check比这个Rule的Order大的Rule，然后所有符合的Rule里，找时间最短的。
                    if (currentRuleOrder != -1 && rule.Order >= currentRuleOrder)
                    {
                        mLog.Info("Current item meet rule order is:{0} and DueDisposalRule order is:{1} greater than it.", currentRuleOrder, rule.Order);
                        break;
                    }

                    #region Records Rule contains TermRule and Records will send all TermRule for current node,so we need check file meet which TermRule.
                    try
                    {
                        int filterCount = rule.Filters.Where(f => f.Rule is TermRule).ToList().Count;
                        string termRuleAndOrExpression = "( ";
                        for (int i = 1; i <= filterCount; i++)
                        {
                            termRuleAndOrExpression = termRuleAndOrExpression + i + " Or ";
                        }
                        termRuleAndOrExpression = termRuleAndOrExpression.TrimEnd(" Or".ToCharArray()) + " )";
                        mLog.Info("termRuleAndOrExpression is:{0}.", termRuleAndOrExpression);
                        Dictionary<PolicyLevel, string> filterConditionExpressionLists = new Dictionary<PolicyLevel, string>() { };
                        filterConditionExpressionLists.Add(policyLevel, termRuleAndOrExpression);
                        List<FilterPolicy> filterPolicys = new List<FilterPolicy>();
                        for (int i = 0; i < rule.Filters.Where(f => f.Rule is TermRule).ToList().Count; i++)
                        {
                            filterPolicys.Add(CloneFilterPolicy(rule.Filters.Where(f => f.Rule is TermRule).ToList()[i], i));
                        }
                        var engine = new FilterEngine(filterPolicys, filterConditionExpressionLists, true);
                        if (!engine.IsQualified(baseInfo))
                        {
                            mLog.Info("Current item doesn't have term meet current rule");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARCOMCheckRuleManagementCriteria + ex);
                        continue;
                    }
                    #endregion

                    #region Check And Expression NoTimeFilter CheckRule.
                    List<FilterPolicy> noTimeFilterPolicys = new List<FilterPolicy>();
                    noTimeFilterPolicys = rule.Filters.Where(f => !(f.Rule is TermRule) && f.Condition != PolicyCondition.OlderThan).ToList();
                    if (isAndExpression && noTimeFilterPolicys.Count != 0)
                    {
                        try
                        {
                            int filterCount = noTimeFilterPolicys.Count;
                            string noTimeRuleAndOrExpression = "( ";
                            for (int i = 1; i <= filterCount; i++)
                            {
                                noTimeRuleAndOrExpression = noTimeRuleAndOrExpression + i + " And ";
                            }
                            noTimeRuleAndOrExpression = noTimeRuleAndOrExpression.TrimEnd(" And".ToCharArray()) + " )";
                            mLog.Info("noTimeRuleAndOrExpression is:{0}.", noTimeRuleAndOrExpression);
                            Dictionary<PolicyLevel, string> filterConditionExpressionLists = new Dictionary<PolicyLevel, string>() { };
                            filterConditionExpressionLists.Add(policyLevel, noTimeRuleAndOrExpression);
                            List<FilterPolicy> filterPolicys = new List<FilterPolicy>();
                            for (int i = 0; i < noTimeFilterPolicys.Count; i++)
                            {
                                filterPolicys.Add(CloneFilterPolicy(noTimeFilterPolicys[i], i));
                            }
                            var engine = new FilterEngine(filterPolicys, filterConditionExpressionLists, true);
                            if (!engine.IsQualified(baseInfo))
                            {
                                mLog.Info("Current item doesn't have NoTimeFilter meet current rule");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn(LOGRESOURCE.StorageOptimization13_SOARCOMCheckRuleManagementCriteria + ex);
                            continue;
                        }
                    }
                    #endregion

                    foreach (var filter in rule.Filters)
                    {
                        if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                    || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule || filter.Rule is StubLastActiveTimeRule)
                        {
                            DateTime timeValue = DateTime.MinValue;
                            #region get time value
                            if (filter.Rule is ModifiedRule)
                            {
                                timeValue = (DateTime)aveItem["Modified"];//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                                timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                            }
                            else if (filter.Rule is CreatedRule)
                            {
                                timeValue = (DateTime)aveItem["Created"];
                                timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                            }
                            else if (filter.Rule is StubLastAccessTimeRule)
                            {
                                try
                                {
                                    timeValue = aveItem.File.GetLastAccessTime(aveItem.File.UniqueId, aveItem.File.ParentFolder.ServerRelativeUrl, ToUniversalTimeWithTimeZone(aveItem.File.TimeLastModified, aveItem.ParentList.ParentWeb));
                                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info("Failed GetLastAccessTime {0}:{1}:{2}", aveItem.Name, filter.Rule.Value1, ex.ToString());
                                    timeValue = (DateTime)aveItem["Modified"];
                                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                                }
                            }
                            else if (filter.Rule is StubLastActiveTimeRule)
                            {
                                try
                                {
                                    timeValue = aveItem.File.GetLastAccessTime(aveItem.File.UniqueId, aveItem.File.ParentFolder.ServerRelativeUrl, ToUniversalTimeWithTimeZone(aveItem.File.TimeLastModified, aveItem.ParentList.ParentWeb), isCompatibleByModifiedTime: true);
                                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                                }
                                catch (Exception ex)
                                {
                                    mLog.Info("Failed GetLastAccessTime {0}:{1}:{2}", aveItem.Name, filter.Rule.Value1, ex.ToString());
                                    timeValue = (DateTime)aveItem["Modified"];
                                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                                }
                            }
                            else if (filter.Rule is ColumnDateTimeRule)
                            {
                                try
                                {
                                    timeValue = (DateTime)aveItem[filter.Rule.Value1];
                                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveItem.ParentList.ParentWeb);
                                }
                                catch (Exception e)
                                {
                                    mLog.Info("no such column {0}:{1}:{2}", aveItem.Name, filter.Rule.Value1, e.ToString());
                                    //当前Rule是And关系时直接Break当前Rule，当前Rule是Or时 Continue当前Filter.
                                    if (!isAndExpression)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        goto GoToC;
                                    }
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
                                mLog.Info("timeValue is:{0}.", timeValue.ToString());
                                #endregion
                                //And条件取最晚的时间，Or条件取最早的时间
                                if (!isAndExpression)
                                {
                                    if (disposalTime > timeValue)
                                    {
                                        disposalTime = timeValue;//to do next
                                        resultRule = rule;
                                    }
                                }
                                else
                                {
                                    if (disposalTime < timeValue)
                                    {
                                        disposalTime = timeValue;//to do next
                                        resultRule = rule;
                                    }
                                }
                            }
                        }
                    }
                }
                if (resultRule != null && !duedates.ContainsKey(resultRule))
                {
                    duedates.Add(resultRule, disposalTime);
                }
            GoToC:;
            }
            dueDisposalTime = duedates.Count == 0 ? 0 : duedates.OrderBy(x => x.Value).FirstOrDefault().Value.Ticks;
            return duedates.Count == 0 ? null : duedates.OrderBy(x => x.Value).FirstOrDefault().Key;
        }
        public Rule GetDueDisposalRule(IAveFolder aveFolder, ref string dueDisposalTime)
        {
            DateTime disposalTime = DateTime.MinValue;
            Rule resultRule = null;

            foreach (var rule in mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == PolicyLevel.Folder))
            {
                if (rule.AndOrExpression.FirstOrDefault().Value.Contains("or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        if (filter.Rule is CreatedRule || filter.Rule is ModifiedRule
                        || filter.Rule is ColumnDateTimeRule || filter.Rule is StubLastAccessTimeRule)
                        {
                            DateTime timeValue = DateTime.MinValue;
                            #region get time value
                            if (filter.Rule is ModifiedRule)
                            {
                                timeValue = (DateTime)aveFolder.Item["Modified"];//item == null ? file.TimeLastModified : (DateTime)item["Modified"];
                                timeValue = ToUniversalTimeWithTimeZone(timeValue, aveFolder.ParentList.ParentWeb);
                            }
                            else if (filter.Rule is CreatedRule)
                            {
                                timeValue = (DateTime)aveFolder.Item["Created"];
                                timeValue = ToUniversalTimeWithTimeZone(timeValue, aveFolder.ParentList.ParentWeb);
                            }
                            else if (filter.Rule is ColumnDateTimeRule)
                            {
                                try
                                {
                                    timeValue = (DateTime)aveFolder.Item[filter.Rule.Value1];
                                    timeValue = ToUniversalTimeWithTimeZone(timeValue, aveFolder.ParentList.ParentWeb);
                                }
                                catch (Exception e)
                                {
                                    mLog.Info("no such column {0}:{1}:{2}", aveFolder.Name, filter.Rule.Value1, e.ToString());
                                }
                            }
                            #endregion
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
                            if (disposalTime < timeValue)
                            {
                                disposalTime = timeValue;
                                resultRule = rule;
                            }
                        }
                    }
                }
            }
            dueDisposalTime = disposalTime == DateTime.MinValue ? string.Empty : disposalTime.Ticks.ToString();
            return resultRule;
        }

        /// <summary>
        /// 用来检查一个文件是否符合Rule. // modify the discoveritem to IAveListItem  
        /// </summary>
        public Rule CheckItemCriteria(Guid docId, object oItem)
        {
            Rule result = null;
            try
            {
                var item = oItem as IAveListItem;
                if (item == null || FilterPolicyCollection == null)
                {
                    return null;
                }
                if (item.ParentList.BaseType == AveBaseType.DocumentLibrary)
                {
                    List<FilterPolicy> docFilters = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Document)).ToList();
                    var documentInfoRe = new DocumentInfo();
                    baseInfo = FilterAnalyser.SetVersionAlwaysTrue(docFilters, FilterAnalyser.GetDocumentFilterInfo(docFilters, item.File, item));
                }
                else
                {
                    List<FilterPolicy> itemFilters = FilterPolicyCollection.AsQueryable().Where(t => t.Level.Equals(PolicyLevel.Item)).ToList();
                    baseInfo = FilterAnalyser.SetVersionAlwaysTrue(itemFilters, FilterAnalyser.GetItemFilterInfo(itemFilters, item));
                }
                ItemInfo itemInfo = baseInfo as ItemInfo;
                
                if (itemInfo != null)
                {
                    //Office365 APi Discussion Board and Survey List's Item title is null,we need to give it string.Empty
                    itemInfo.Name = itemInfo.Name ?? string.Empty;
                    itemInfo.Title = itemInfo.Title ?? string.Empty;
                }
                result = CheckCriteria(baseInfo);
                if (result == null)
                {
                    mLog.LogToXml(string.Format("ItemInfo:{0}", item.DisplayName), baseInfo);
                }
            }
            catch (Exception ex)
            {
                result = null;
                mLog.Info("An error occur when CheckItemCriteria.Message:{0}.", ex.ToString());
            }
            return result;
        }

        public Rule CheckFolderCriteria(object oFolder, bool IsMicroFeedList = false)
        {
            Rule rule = null;
            var folder = oFolder as IAveFolder;
            if (folder == null || FilterPolicyCollection == null)
            {
                return null;
            }
            baseInfo = FilterAnalyser.GetFolderFilterInfo(FilterPolicyCollection, folder);
            //  var baseInfo = folder.GetFilterObjectInfo(FilterPolicyCollection);
            rule = CheckCriteria(baseInfo);
            if (rule != null && rule.PolicyLevel == PolicyLevel.Folder && IsMicroFeedList)
            {
                mLog.Info("Folder rule doesn't process MicroFeedList. Folder Name:{0}.", folder.Name);
                rule = null;
            }
            if (rule == null)
            {
                mLog.LogToXml(string.Format("FolderInfo:{0}", folder.Name), baseInfo);
            }
            return rule;
        }

        private Rule CheckCriteria(ObjectInfoBase info, int thresholdRuleOrder = -1)
        {
            //在这里遍历Rule时, 应该考虑到Rule的Order, 如果能准确依赖于Manager发的Order的话, 这里可以不考虑Order, 否则
            //应该在遍历的时候考虑到Rule的Order.
            foreach (var rulet in mRuleCollection.Rules.Values)
            {
                if (-1 == thresholdRuleOrder || rulet.Order < thresholdRuleOrder)
                {
                    //如果一个Version的检查过程中， threshold不等于－1表示这个version的当前版本有对应的Rule,
                    //因此，如果一个rulet.Order小于threshold时， 继续检查

                    try
                    {
                        //我们需要filter out模式
                        var engine = new FilterEngine(rulet.Filters, rulet.AndOrExpression, true);
                        if (engine.IsQualified(info))
                        {
                            return rulet;
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Error(LOGRESOURCE.StorageOptimization13_SOARCOMCheckRuleManagementCriteria + ex);
                        throw new Exception(LOGRESOURCE.StorageOptimization13_SOARCOMRuleManagementCheckCriteriaException + rulet.Compression);
                    }

                }
                else if (rulet.Order == thresholdRuleOrder)
                {
                    return rulet;
                }
            }
            return null;
        }

        private static DateTime ToUniversalTimeWithTimeZone(DateTime datetime, IAveWeb web)
        {
            if (datetime.Kind != DateTimeKind.Utc)
            {
                datetime = web.RegionalSettings.TimeZone.LocalTimeToUTC(datetime);
            }
            return datetime;
        }

        internal class FilterRuleTypeEqualityComparer : IEqualityComparer<FilterPolicy>
        {
            private static FilterRuleTypeEqualityComparer instance;

            private FilterRuleTypeEqualityComparer()
            {
            }
            public static FilterRuleTypeEqualityComparer GetInstance()
            {
                if (instance == null)
                {
                    instance = new FilterRuleTypeEqualityComparer();
                }
                return instance;
            }
            public bool Equals(FilterPolicy x, FilterPolicy y)
            {
                return x.Rule.GetType().Equals(y.Rule.GetType());
            }

            public int GetHashCode(FilterPolicy obj)
            {
                return 0;
            }
        }

    }
}
