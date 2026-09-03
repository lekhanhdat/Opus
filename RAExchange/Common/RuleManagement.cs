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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using ExchangeBackupUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IExchangeItem = ExchangeBackupUtility.Graph.IExchangeItem;

namespace AvePoint.RA.RAExchange.Common
{
    public class RuleManagement
    {
        #region private member
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(RuleManagement));
        private readonly RuleCollection mRuleCollection;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3);

        #endregion

        #region property

        public bool HasExchangeItemCondition { get; private set; }
        public bool HasExchangeFolderCondition { get; private set; }
        public bool HasMailboxCondition { get; private set; }//exchange mailbox rule
        private int RuleLevelNumber { get; set; }
        private List<FilterPolicy> FilterPolicyCollection { get; set; }

        #endregion property

        #region public method

        public RuleManagement(RuleCollection sheduleRuleCollection)
        {
            mRuleCollection = sheduleRuleCollection;

            #region find all conditions type.

            if (mRuleCollection != null)
                foreach (var rule in mRuleCollection.Rules.Select(rulet => rulet.Value))
                {
                    if (!HasExchangeItemCondition)
                    {
                        HasExchangeItemCondition |= rule.PolicyLevel == PolicyLevel.ExchangeOnlineItem;
                    }
                    if (!HasExchangeFolderCondition)
                    {
                        HasExchangeFolderCondition |= rule.PolicyLevel == PolicyLevel.ExchangeOnlineFolder;
                    }
                    if (!HasMailboxCondition)
                    {
                        HasMailboxCondition |= rule.PolicyLevel == PolicyLevel.ExchangeOnlineMailbox;
                    }
                }
            if (HasExchangeItemCondition)
            {
                RuleLevelNumber = (int)ExchangeCacheNodeType.Item;
                MergeFilterPolicy();
                return;
            }
            if (HasExchangeFolderCondition)
            {
                RuleLevelNumber = (int)ExchangeCacheNodeType.Folder;
                MergeFilterPolicy();
                return;
            }
            if (HasMailboxCondition) //支持exchange mailbox level rule
            {
                RuleLevelNumber = (int)ExchangeCacheNodeType.Mailbox;
                MergeFilterPolicy();
                return;
            }
            #endregion

        }

        public Rule CheckItemCriteria(ExchangeItem entity)
        {
            using (var performance = new PerformanceScope("EXO.RuleManagement.CheckItemCriteria", addToStatistics: true))
            {
                Rule rule = null;
                if (entity == null)
                {
                    return null;
                }
                ObjectInfoBase baseInfo = null;
                PolicyLevel currentEntityPolicyLevel = ConvertMailTypeToPolicyLevel(entity.ItemType);
                //除了ExchangeItem_Message 其他类型的Item暂不支持，如果需要支持，可以参考DAO 代码实现相关方法
                switch (currentEntityPolicyLevel)
                {
                    case PolicyLevel.ExchangeOnlineItem_Message:
                        baseInfo = GetEMessageFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Task:
                        //baseInfo = GetETaskFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Post:
                        //baseInfo = GetEPostFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Event:
                        //baseInfo = GetEEventFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Journal:
                        //baseInfo = GetEJournalFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Note:
                        //baseInfo = GetENoteFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Contact:
                        //baseInfo = GetEContactFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    case PolicyLevel.ExchangeOnlineItem_Document:
                        //baseInfo = GetEDocumentFilterObjectInfo(FilterPolicyCollection, entity);
                        break;
                    default:
                        break;
                }
                if (entity.IsDraft)
                {
                    logger.Info($"CheckItemCriteria.MailName:{entity.ItemName ?? string.Empty}.IsDraft and skip draft.");
                    return null;
                }
                rule = CheckCriteria(baseInfo, ConvertMailTypeToPolicyLevel(entity.ItemType));
                if (SharePoint.ArchiverCommon.ArchiverCommonStaticMethod.IsNestleCustomize)
                {
                    OutputObjectInfoBase(baseInfo, entity, rule);
                }
                return rule;
            }
        }
        
        public Rule CheckItemCriteria(IExchangeItem entity, Dictionary<Guid, string> retentionLabelsDic = null)
        {
            using var performance = new PerformanceScope("EXO.RuleManagement.CheckItemCriteria", addToStatistics: true);
            Rule rule = null;
            if (entity == null)
            {
                return null;
            }
            ObjectInfoBase baseInfo = null;
            PolicyLevel currentEntityPolicyLevel = ConvertMailTypeToPolicyLevel(entity.ItemType);
            //除了ExchangeItem_Message 其他类型的Item暂不支持，如果需要支持，可以参考DAO 代码实现相关方法
            switch (currentEntityPolicyLevel)
            {
                case PolicyLevel.ExchangeOnlineItem_Message:
                    baseInfo = GetEMessageFilterObjectInfo(FilterPolicyCollection, entity, retentionLabelsDic);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Task:
                    //baseInfo = GetETaskFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Post:
                    //baseInfo = GetEPostFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Event:
                    //baseInfo = GetEEventFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Journal:
                    //baseInfo = GetEJournalFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Note:
                    //baseInfo = GetENoteFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Contact:
                    //baseInfo = GetEContactFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Document:
                    //baseInfo = GetEDocumentFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                default:
                    break;
            }
            if (entity.IsDraft)
            {
                logger.Info($"CheckItemCriteria.MailName:{entity.ItemName ?? string.Empty}.IsDraft and skip draft.");
                return null;
            }
            rule = CheckCriteria(baseInfo, ConvertMailTypeToPolicyLevel(entity.ItemType));
            if (SharePoint.ArchiverCommon.ArchiverCommonStaticMethod.IsNestleCustomize)
            {
                OutputObjectInfoBase(baseInfo, entity, rule);
            }
            return rule;
        }


        private void OutputObjectInfoBase(ObjectInfoBase baseInfo, IExchangeItem entity, Rule rule)
        {
            try
            {
                logger.Info($"CheckItemCriteria.MailName:{entity.ItemName ?? string.Empty}.Mail Path:{entity.ItemPath}.IsMeetRule:{rule != null}.BaseInfo:{SerializerHelper.SerializeByJsonSerializer(baseInfo as ExchangeMessageInfo)}.");
            }
            catch (Exception ex)
            {
                logger.Warn($"Failed OutputObjectInfoBase.Message:{ex}.");
            }
        }
        public Rule GetRuleFromRuleCollectionByRuleId(string ruleId)
        {
            return mRuleCollection.Rules.Values.Where(a => a.Id == ruleId)?.FirstOrDefault();
        }
        public bool HasLowerLevelRule(int cacheNodeType)
        {
            return cacheNodeType < RuleLevelNumber;
        }

        #endregion

        public Rule GetDueDisposalRule(ExchangeItem entity, ref string dueDisposalTime)
        {
            DateTime resutlTime = DateTime.MinValue;
            Rule resultRule = null;
            PolicyLevel policyLevel = PolicyLevel.ExchangeOnlineItem;

            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == policyLevel && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));
            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;
                //Record 里面只有all 和any， 所以有一个or 表示是any
                //进入这个判断表示只有一个older than 的条件，所以早晚会符合rule，只是时间问题
                if (rule.AndOrExpression[PolicyLevel.ExchangeOnlineItem_Message].Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, entity);
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
                    var r = CheckItemDueDateCriteria(entity, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, entity);
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
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }

        public Rule GetDueDisposalRule(IExchangeItem entity, ref string dueDisposalTime)
        {
            DateTime resutlTime = DateTime.MinValue;
            Rule resultRule = null;
            PolicyLevel policyLevel = PolicyLevel.ExchangeOnlineItem;

            var curRules = mRuleCollection.Rules.Select(rulet => rulet.Value).AsQueryable().Where(r => r.PolicyLevel == policyLevel && r.Filters.Any(fi => fi.Condition == PolicyCondition.OlderThan));
            foreach (var rule in curRules)
            {
                DateTime disposalTime = DateTime.MinValue;
                Rule tempRule = null;
                //Record 里面只有all 和any， 所以有一个or 表示是any
                //进入这个判断表示只有一个older than 的条件，所以早晚会符合rule，只是时间问题
                if (rule.AndOrExpression[PolicyLevel.ExchangeOnlineItem_Message].Contains("Or") || rule.Filters.Count == 1)
                {
                    foreach (var filter in rule.Filters)
                    {
                        var timeValue = GetDueDate(filter, entity);
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
                    var r = CheckItemDueDateCriteria(entity, rule);

                    if (r != null)
                    {
                        foreach (var filter in rule.Filters)
                        {
                            var timeValue = GetDueDate(filter, entity);
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
                        resultRule = tempRule;
                    }
                }
            }
            dueDisposalTime = resutlTime == DateTime.MinValue ? string.Empty : resutlTime.Ticks.ToString();
            return resultRule;
        }

        private DateTime GetDueDate(FilterPolicy filter, ExchangeItem item)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is SendDateUTCRule || filter.Rule is SendDateRule)
            {
                timeValue = item.SendDateUTC;
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

        private DateTime GetDueDate(FilterPolicy filter, IExchangeItem item)
        {
            DateTime timeValue = DateTime.MinValue;
            if (filter.Condition != PolicyCondition.OlderThan)
            {
                return timeValue;
            }
            if (filter.Rule is SendDateUTCRule || filter.Rule is SendDateRule)
            {
                timeValue = item.SendDateUTC;
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

        //此方法检查item 未来一段时间可能符合的Rule，并且在check 过程把时间赋成最小值，忽略时间条件的影响。
        //最理想方案是剔除older than。然后肯定不应该符合before，并且From to 的value 2 必须比当前时间小,由于逻辑复杂，暂时采用SP 的方案
        public Rule CheckItemDueDateCriteria(ExchangeItem entity, Rule rule)
        {
            if (entity == null)
            {
                return null;
            }
            ObjectInfoBase baseInfo = null;

            PolicyLevel currentEntityPolicyLevel = ConvertMailTypeToPolicyLevel(entity.ItemType);
            //除了ExchangeItem_Message 其他类型的Item暂不支持，如果需要支持，可以参考DAO 代码实现相关方法
            switch (currentEntityPolicyLevel)
            {
                case PolicyLevel.ExchangeOnlineItem_Message:
                    baseInfo = GetEMessageFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Task:
                    //baseInfo = GetETaskFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Post:
                    //baseInfo = GetEPostFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Event:
                    //baseInfo = GetEEventFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Journal:
                    //baseInfo = GetEJournalFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Note:
                    //baseInfo = GetENoteFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Contact:
                    //baseInfo = GetEContactFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Document:
                    //baseInfo = GetEDocumentFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                default:
                    break;
            }
            //此处将时间都设置成DateTime.MinValue，保证肯定能符合older than rule，但是对于一定不符合before的时间条件，和可能符合的From to 条件，都存在Know issue。  
            //比如当前rule中有一个before 条件，item肯定不满足，那么这个文件永远不会符合这个rule，但是由于改成了datatime.minvalue，就会符合了这个rule
            if (baseInfo is ExchangeMessageInfo)
            {
                ExchangeMessageInfo docInfo = baseInfo as ExchangeMessageInfo;
                docInfo.Modified = DateTime.MinValue;
                docInfo.Created = DateTime.MinValue;
                docInfo.SendDateUTC = DateTime.MinValue;
            }

            return CheckCurrentCriteria(baseInfo, rule);

        }

        public Rule CheckItemDueDateCriteria(IExchangeItem entity, Rule rule)
        {
            if (entity == null)
            {
                return null;
            }
            ObjectInfoBase baseInfo = null;

            PolicyLevel currentEntityPolicyLevel = ConvertMailTypeToPolicyLevel(entity.ItemType);
            //除了ExchangeItem_Message 其他类型的Item暂不支持，如果需要支持，可以参考DAO 代码实现相关方法
            switch (currentEntityPolicyLevel)
            {
                case PolicyLevel.ExchangeOnlineItem_Message:
                    baseInfo = GetEMessageFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Task:
                    //baseInfo = GetETaskFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Post:
                    //baseInfo = GetEPostFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Event:
                    //baseInfo = GetEEventFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Journal:
                    //baseInfo = GetEJournalFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Note:
                    //baseInfo = GetENoteFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Contact:
                    //baseInfo = GetEContactFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                case PolicyLevel.ExchangeOnlineItem_Document:
                    //baseInfo = GetEDocumentFilterObjectInfo(FilterPolicyCollection, entity);
                    break;
                default:
                    break;
            }
            //此处将时间都设置成DateTime.MinValue，保证肯定能符合older than rule，但是对于一定不符合before的时间条件，和可能符合的From to 条件，都存在Know issue。  
            //比如当前rule中有一个before 条件，item肯定不满足，那么这个文件永远不会符合这个rule，但是由于改成了datatime.minvalue，就会符合了这个rule
            if (baseInfo is ExchangeMessageInfo)
            {
                ExchangeMessageInfo docInfo = baseInfo as ExchangeMessageInfo;
                docInfo.Modified = DateTime.MinValue;
                docInfo.Created = DateTime.MinValue;
                docInfo.SendDateUTC = DateTime.MinValue;
            }

            return CheckCurrentCriteria(baseInfo, rule);
        }

        private Rule CheckCurrentCriteria(ObjectInfoBase info, Rule rulet, int thresholdRuleOrder = -1)
        {
            if (-1 == thresholdRuleOrder || rulet.Order < thresholdRuleOrder)
            {
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
                    if (ex is PropertyNotAssignedException)
                    {
                        logger.Error("A property was not assigned while checking an Exchange rule. Exception:{0}", ex.ToString());
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

        //private ObjectInfoBase GetMailboxFilterObjectInfo(List<FilterPolicy> policies, RuleEntity entity)
        //{
        //    ExchangeMailboxInfo result = new ExchangeMailboxInfo();
        //    policies = CreateDistinctFiltersCopy(policies, PolicyLevel.ExchangeOnlineMailbox);
        //    //mailboxAddress rule，其他rule有需求再添加
        //    foreach (FilterPolicy policy in policies)
        //    {
        //        string ruleName = policy.Rule.GetType().Name;
        //        ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
        //        switch (ruleName)
        //        {
        //            case "MailboxAddressRule":
        //                result.MailboxAddress = entity.MailboxAddress;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return result;
        //}
        //private ObjectInfoBase GetEFolderFilterObjectInfo(List<FilterPolicy> policies, RuleEntity entity)
        //{
        //    ExchangeFolderInfo result = new ExchangeFolderInfo();
        //    policies = CreateDistinctFiltersCopy(policies, PolicyLevel.ExchangeOnlineFolder);
        //    //mailboxAddress rule，其他rule有需求再添加
        //    foreach (FilterPolicy policy in policies)
        //    {
        //        string ruleName = policy.Rule.GetType().Name;
        //        ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
        //        switch (ruleName)
        //        {
        //            case "NameRule":
        //                result.FolderName = entity.FolderName;
        //                break;
        //            case "SubFolderCountRule":
        //                result.ChildFolderCount = entity.ChildFolderCount;
        //                break;
        //            case "ItemCountRule":
        //                result.ItemsCount = entity.ItemsCount;
        //                break;
        //            case "FolderTypeRule":
        //                result.FolderType = entity.FolderType;
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    return result;
        //}
        private ObjectInfoBase GetEMessageFilterObjectInfo(List<FilterPolicy> policies, ExchangeItem entity)
        {
            using (var performance = new PerformanceScope("EXO.RuleManagement.GetEItemFilterObjectInfo"))
            {
                ExchangeMessageInfo result = new ExchangeMessageInfo();
                policies = CreateDistinctFiltersCopy(policies, ConvertMailTypeToPolicyLevel(entity.ItemType));
                //暂时支持title和mailboxAddress rule，其他rule有需求再添加
                foreach (FilterPolicy policy in policies)
                {
                    string ruleName = policy.Rule.GetType().Name;
                    ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                    switch (ruleName)
                    {
                        case "NameRule":
                            result.ItemName = entity.ItemName ?? string.Empty;
                            break;
                        case "TypeRule":
                            result.ItemType = entity.ItemType;
                            break;
                        case "ModifiedRule":
                            result.Modified = entity.Modified;
                            break;
                        case "CreatedRule":
                            result.Created = entity.Created;
                            break;
                        case "ModifiedByRule":
                            result.ModifiedBy = entity.ModifiedBy;
                            break;
                        case "SizeRule":
                            result.ItemSize = entity.ItemSize;
                            break;
                        case "SendToRule":
                            result.SendToDisplayWithAddress = entity.DisplayTo;
                            result.SendToDisplayName = entity.DisplayTo;
                            result.SendToEmailAddress = entity.DisplayTo;
                            break;
                        case "SendFromRule":
                            result.SendFromDisplayName = entity.SenderDisplayName;
                            result.SendFromEmailAddress = entity.SenderEmailAddress;
                            result.SendFromDisplayWithAddress = entity.Sender;
                            break;
                        case "SubjectRule":
                            result.Subject = entity.ItemName ?? string.Empty;
                            break;
                        case "SendDateRule":
                        case "SendDateUTCRule":
                            result.SendDateUTC = entity.SendDateUTC;
                            break;
                        case "AttachmentRule":
                            result.AttachmentCount = entity.AttachmentCount;
                            break;
                        case "RetentionLabelRule":
                            result.RetentionLabel = entity.RetentionLabel;
                            break;
                        case "SensitivityLabelRule":
                            result.SensitivityLabel = entity.SensitivityLabel;
                            break;
                        default:
                            break;
                    }
                }
                return result;
            }
        }

        private ObjectInfoBase GetEMessageFilterObjectInfo(List<FilterPolicy> policies, IExchangeItem entity, Dictionary<Guid, string> retentionLabelsDic = null)
        {
            using (var performance = new PerformanceScope("EXO.RuleManagement.GetEItemFilterObjectInfo"))
            {
                try
                {
                    _semaphore.WaitAsync().ExecuteAsyncTask();
                    ExchangeMessageInfo result = new ExchangeMessageInfo();
                    policies = CreateDistinctFiltersCopy(policies, ConvertMailTypeToPolicyLevel(entity.ItemType));
                    //暂时支持title和mailboxAddress rule，其他rule有需求再添加
                    foreach (FilterPolicy policy in policies)
                    {
                        string ruleName = policy.Rule.GetType().Name;
                        ruleName = ruleName.Substring(ruleName.LastIndexOf('.') + 1);
                        switch (ruleName)
                        {
                            case "NameRule":
                                result.ItemName = entity.ItemName ?? string.Empty;
                                break;
                            case "TypeRule":
                                result.ItemType = entity.ItemType;
                                break;
                            case "ModifiedRule":
                                result.Modified = entity.Modified;
                                break;
                            case "CreatedRule":
                                result.Created = entity.Created;
                                break;
                            case "ModifiedByRule":
                                result.ModifiedBy = entity.ModifiedBy;
                                break;
                            case "SizeRule":
                                result.ItemSize = entity.ItemSize;
                                break;
                            case "SendToRule":
                                result.SendToDisplayWithAddress = entity.DisplayTo;
                                result.SendToDisplayName = entity.DisplayTo;
                                result.SendToEmailAddress = entity.DisplayTo;
                                break;
                            case "SendFromRule":
                                result.SendFromDisplayName = entity.SenderDisplayName;
                                result.SendFromEmailAddress = entity.SenderEmailAddress;
                                result.SendFromDisplayWithAddress = entity.Sender;
                                break;
                            case "SubjectRule":
                                result.Subject = entity.ItemName ?? string.Empty;
                                break;
                            case "SendDateRule":
                            case "SendDateUTCRule":
                                result.SendDateUTC = entity.SendDateUTC;
                                break;
                            case "AttachmentRule":
                                result.AttachmentCount = entity.AttachmentCount;
                                break;
                            case "RetentionLabelRule":
                                if (!string.IsNullOrEmpty(entity.RetentionLabel))
                                {
                                    result.RetentionLabel = entity.RetentionLabel;
                                }
                                else if (retentionLabelsDic != null)
                                {
                                    Guid retentionId = entity.PolicyTag?.RetentionId ?? Guid.Empty;

                                    retentionLabelsDic.TryGetValue(retentionId, out string labelName);

                                    result.RetentionLabel = labelName ?? string.Empty;
                                }
                                else
                                {
                                    result.RetentionLabel = string.Empty;
                                }

                                logger.Info($"Retention label name : {result.RetentionLabel} for item: {entity.ItemName}"); break;

                            case "SensitivityLabelRule":
                                result.SensitivityLabel = entity.SensitivityLabel;
                                break;
                            default:
                                break;
                        }
                    }

                    return result;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }


        public PolicyLevel ConvertMailTypeToPolicyLevel(string itemType)
        {
            switch (itemType)
            {
                case "IPM.Note":
                    return PolicyLevel.ExchangeOnlineItem_Message;
                case "IPM.Task":
                    return PolicyLevel.ExchangeOnlineItem_Task;
                case "IPM.Post":
                    return PolicyLevel.ExchangeOnlineItem_Post;
                case "IPM.Appointment":
                    return PolicyLevel.ExchangeOnlineItem_Event;
                case "IPM.Activity":
                    return PolicyLevel.ExchangeOnlineItem_Journal;
                case "IPM.StickyNote":
                    return PolicyLevel.ExchangeOnlineItem_Note;
                case "IPM.Contact":
                    return PolicyLevel.ExchangeOnlineItem_Contact;
                case "IPM.Document":
                    return PolicyLevel.ExchangeOnlineItem_Document;
                default:
                    return PolicyLevel.ExchangeOnlineItem_Message;
            }
        }

        private Rule CheckCriteria(ObjectInfoBase info, PolicyLevel policyLevel, int thresholdRuleOrder = -1)
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
                        FilterEngine engine = null;
                        Dictionary<PolicyLevel, string> filterConditionExpressionLists = new Dictionary<PolicyLevel, string>();
                        if (rulet.AndOrExpression.ContainsKey(policyLevel))
                        {
                            filterConditionExpressionLists.Add(policyLevel, rulet.AndOrExpression[policyLevel]);
                            engine = new FilterEngine(rulet.Filters.Where(filter => filter.Level == policyLevel).ToList(), filterConditionExpressionLists, true);
                        }
                        else
                        {
                            //目前EXO Records只支持Mail Level，其它Level例如Event之类的不支持
                            logger.Info("Current object PolicyLevel doesn't support at current rule.RuleName:{0}.PolicyLevel:{1}.", rulet.Name, policyLevel.ToString());
                            engine = new FilterEngine(rulet.Filters, rulet.AndOrExpression, true);
                        }
                        if (info == null)
                        {
                            logger.Info("Current object info is null.PolicyLevel:{0}.", policyLevel.ToString());
                            return null;
                        }
                        if (engine.IsQualified(info))
                        {
                            return rulet;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Error in check criteria, message : {ex.ToString()}.");
                        throw new Exception($"Error in check criteria : {rulet.Compression}.");
                    }
                }
                else if (rulet.Order == thresholdRuleOrder)
                {
                    return rulet;
                }
            }
            return null;
        }

        /// <summary>
        /// 获得该Level每种Filter的不重复的Rule
        /// </summary>
        private List<FilterPolicy> CreateDistinctFiltersCopy(List<FilterPolicy> filters, PolicyLevel level)
        {
            if (filters != null)
            {
                return filters.Where(filter => filter.Level == level).Distinct(FilterRuleTypeEqualityComparer.GetInstance()).ToList();
            }
            return new List<FilterPolicy>();
        }
        private void MergeFilterPolicy()
        {
            FilterPolicyCollection = new List<FilterPolicy>();
            var filterPolicyType = new List<Type>();
            foreach (var filterPolicy in mRuleCollection.Rules.Values.SelectMany(rule => rule.Filters))
            {
                FilterPolicyCollection.Add(filterPolicy);
                filterPolicyType.Add(filterPolicy.Rule.GetType());
            }
        }
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

    internal enum ExchangeCacheNodeType
    {
        Group = 0,
        Mailbox = 3,
        Folder = 5,
        Item = 7,
    }
}
