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
using Storage;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Common.FileSystem
{
    public class DisposalRuleEngine
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(DisposalRuleEngine));
        private readonly List<Rule> _rules;

        public DisposalRuleEngine(List<Rule> rules)
        {
            _rules = rules;
        }
        public Tuple<Rule, TimeSpan> MatchPotentialRule(ObjectInfoBase obj, bool checkActionDueDate = false)
        {
            if (_rules == null || obj is IXSystem)
            {
                return null;
            }

            var rule = CheckCriteria(obj);
            if (rule != null)
            {
                //directly matched
                return new Tuple<Rule, TimeSpan>(rule, default(TimeSpan));
            }
            else if (checkActionDueDate)
            {
                //取出来所有带Older Than条件的Rule(包括Criteria有其它条件的Rule)
                var potentialRules = _rules.Where(t => t.FSRule.Filters != null && t.FSRule.Filters.Any(f => f.Condition == AvePoint.GCommon.Contract.CommonFilter.PolicyCondition.OlderThan)).ToList();

                foreach (var pr in potentialRules)
                {
                    try
                    {
                        Dictionary<string, TimeSpan> offsets = ComputeCheckRuleOffsets(obj, pr);
                        var tObj = ObjectConverter.CloneFilterObject(obj, offsets);
                        var engine = new FilterEngine(pr.FSRule.Filters, pr.FSRule.AndOrExpression, true);
                        if (engine.IsQualified(tObj))
                        {
                            return new Tuple<Rule, TimeSpan>(pr, ComputeActionDueDateOffsets(obj, pr));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is PropertyNotAssignedException)
                        {
                            logger.Error("A property was not assigned while checking a file-system rule. Exception:{0}", ex.ToString());
                        }
                        ArgumentCheck.NotNull(rule, nameof(rule));
                        //logger.Error("Checked expression failed. Expression:{0} ,Exception:{1}", rule.Compression, ex.ToString());
                        throw new Exception(string.Format("Checked expression failed.{0}", rule?.Compression), ex);
                    }
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// 进入此判断，说明当前OlderThan条件还不符合Rule，即Property Time(ModifiedTime/AccessTime/CreatedTime) > CurrentTime-OlderThanTime。
        /// 此时我们需要计算多少天后文件符合rule并且计算文件符合哪个rule.我们的做法是用Property Time(ModifiedTime/AccessTime/CreatedTime)- OlderThanTime进行ReCheck Rule，
        ///此时时间条件一定符合rule，因为文件的Property Time(ModifiedTime/AccessTime/CreatedTime)一定小于Current Time，只需要检测其它类型Criteria即可。
        ///
        ///当前方法的目的是返回对应的时间差让后续的时间条件check rule时全部符合rule，然后让其check其它Criteria
        /// </summary>
        private Dictionary<string, TimeSpan> ComputeCheckRuleOffsets(ObjectInfoBase obj, Rule potentialRule)
        {
            bool isAndExpression = potentialRule.FSRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
            var now = DateTime.UtcNow;
            Dictionary<string, TimeSpan> finalOffsets = new Dictionary<string, TimeSpan>();
            Dictionary<string, List<TimeSpan>> offsets = new Dictionary<string, List<TimeSpan>>();
            Dictionary<string, List<DateTime>> leftMargins = new Dictionary<string, List<DateTime>>();
            foreach (var filter in potentialRule.FSRule.Filters)
            {
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    if (!leftMargins.ContainsKey(filter.Rule.ToString()))
                    {
                        leftMargins[filter.Rule.Value1] = new List<DateTime>();
                    }
                    var value = int.Parse(filter.Value.Value1);
                    if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    {
                        leftMargins[filter.Rule.Value1].Add(now.AddYears(0 - value));
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                    {

                        leftMargins[filter.Rule.Value1].Add(now.AddMonths(0 - value));
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                    {
                        leftMargins[filter.Rule.Value1].Add(now.AddDays(0 - value * 7));
                    }
                    else
                    {
                        leftMargins[filter.Rule.Value1].Add(now.AddDays(0 - value));
                    }
                }
            }

            foreach (var leftMargin in leftMargins)
            {
                if (!offsets.ContainsKey(leftMargin.Key))
                {
                    offsets[leftMargin.Key] = new List<TimeSpan>();
                }
                if (obj is FSFileInfo)
                {
                    var file = obj as FSFileInfo;
                    foreach (var time in leftMargin.Value)
                    {
                        if (leftMargin.Key== "Last Accessed Time" && file.AccessTime >= time)
                        {
                            offsets[leftMargin.Key].Add(file.AccessTime - time);
                        }
                        if (leftMargin.Key == "Created Time" && file.Created >= time)
                        {
                            offsets[leftMargin.Key].Add(file.Created - time);
                        }
                        if (leftMargin.Key == "Modified Time" && file.Modified >= time)
                        {
                            offsets[leftMargin.Key].Add(file.Modified - time);
                        }
                    }
                }
                else if (obj is FSFolderInfo)
                {
                    var folder = obj as FSFolderInfo;
                    foreach (var time in leftMargin.Value)
                    {
                        if (leftMargin.Key == "Last Accessed Time" && folder.AccessTime >= time)
                        {
                            offsets[leftMargin.Key].Add(folder.AccessTime - time);
                        }
                        if (leftMargin.Key == "Created Time" && folder.Created >= time)
                        {
                            offsets[leftMargin.Key].Add(folder.Created - time);
                        }
                        if (leftMargin.Key == "Modified Time" && folder.Modified >= time)
                        {
                            offsets[leftMargin.Key].Add(folder.Modified - time);
                        }
                    }
                }
            }

            foreach (var offset in offsets)
            {
                offset.Value.Sort();
                if (isAndExpression)
                {
                    finalOffsets[offset.Key] = offset.Value.LastOrDefault();
                }
                else
                {
                    finalOffsets[offset.Key] = offset.Value.FirstOrDefault();
                }
            }
            return finalOffsets;
        }

        /// <summary>
        /// 真正计算Due Date的方法
        /// </summary>
        private TimeSpan ComputeActionDueDateOffsets(ObjectInfoBase obj, Rule potentialRule)
        {
            bool isAndExpression = potentialRule.FSRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
            var now = DateTime.UtcNow;
            List<TimeSpan> offsets = new List<TimeSpan>();
            List<DateTime> leftMargins = new List<DateTime>();
            foreach (var filter in potentialRule.FSRule.Filters)
            {
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    var value = int.Parse(filter.Value.Value1);
                    if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    {
                        if (obj is FSFileInfo)
                        {
                            var file = obj as FSFileInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddYears(value) > now)
                            {
                                offsets.Add(file.AccessTime.AddYears(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddYears(value) > now)
                            {
                                offsets.Add(file.Created.AddYears(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddYears(value) > now)
                            {
                                offsets.Add(file.Modified.AddYears(value) - now);
                            }
                        }
                        else if (obj is FSFolderInfo)
                        {
                            var folder = obj as FSFolderInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && folder.AccessTime.AddYears(value) > now)
                            {
                                offsets.Add(folder.AccessTime.AddYears(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && folder.Created.AddYears(value) > now)
                            {
                                offsets.Add(folder.Created.AddYears(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && folder.Modified.AddYears(value) > now)
                            {
                                offsets.Add(folder.Modified.AddYears(value) - now);
                            }
                        }
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                    {
                        if (obj is FSFileInfo)
                        {
                            var file = obj as FSFileInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddMonths(value) > now)
                            {
                                offsets.Add(file.AccessTime.AddMonths(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddMonths(value) > now)
                            {
                                offsets.Add(file.Created.AddMonths(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddMonths(value) > now)
                            {
                                offsets.Add(file.Modified.AddMonths(value) - now);
                            }
                        }
                        else if (obj is FSFolderInfo)
                        {
                            var folder = obj as FSFolderInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && folder.AccessTime.AddMonths(value) > now)
                            {
                                offsets.Add(folder.AccessTime.AddMonths(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && folder.Created.AddMonths(value) > now)
                            {
                                offsets.Add(folder.Created.AddMonths(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && folder.Modified.AddMonths(value) > now)
                            {
                                offsets.Add(folder.Modified.AddMonths(value) - now);
                            }
                        }
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                    {
                        if (obj is FSFileInfo)
                        {
                            var file = obj as FSFileInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddDays(value * 7) > now)
                            {
                                offsets.Add(file.AccessTime.AddDays(value * 7) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddDays(value * 7) > now)
                            {
                                offsets.Add(file.Created.AddDays(value * 7) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddDays(value * 7) > now)
                            {
                                offsets.Add(file.Modified.AddDays(value * 7) - now);
                            }
                        }
                        else if (obj is FSFolderInfo)
                        {
                            var folder = obj as FSFolderInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && folder.AccessTime.AddDays(value * 7) > now)
                            {
                                offsets.Add(folder.AccessTime.AddDays(value * 7) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && folder.Created.AddDays(value * 7) > now)
                            {
                                offsets.Add(folder.Created.AddDays(value * 7) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && folder.Modified.AddDays(value * 7) > now)
                            {
                                offsets.Add(folder.Modified.AddDays(value * 7) - now);
                            }
                        }
                    }
                    else
                    {
                        if (obj is FSFileInfo)
                        {
                            var file = obj as FSFileInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddDays(value) > now)
                            {
                                offsets.Add(file.AccessTime.AddDays(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddDays(value) > now)
                            {
                                offsets.Add(file.Created.AddDays(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddDays(value) > now)
                            {
                                offsets.Add(file.Modified.AddDays(value) - now);
                            }
                        }
                        else if (obj is FSFolderInfo)
                        {
                            var folder = obj as FSFolderInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && folder.AccessTime.AddDays(value) > now)
                            {
                                offsets.Add(folder.AccessTime.AddDays(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && folder.Created.AddDays(value) > now)
                            {
                                offsets.Add(folder.Created.AddDays(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && folder.Modified.AddDays(value) > now)
                            {
                                offsets.Add(folder.Modified.AddDays(value) - now);
                            }
                        }
                    }
                }
            }
            offsets = offsets.Distinct().ToList();
            offsets.Sort();
            logger.Debug("ComputeActionDueDateOffsets:{0}", string.Concat<TimeSpan>(offsets));
            if (isAndExpression)
            {
                return offsets.LastOrDefault();
            }
            else
            {
                return offsets.FirstOrDefault();
            }
        }

        public Rule MatchRule(ObjectInfoBase obj)
        {
            Rule result = null;
            if (_rules == null || obj is IXSystem)
            {
                return null;
            }

            result = CheckCriteria(obj);
            return result;
        }
        private Rule CheckCriteria(ObjectInfoBase info)
        {
            //already sorted when querying the mappings from the db
            //var rules = _rules.OrderBy(t => t.Order);
            foreach (var rule in _rules)
            {
                try
                {
                    if (rule.FSRule.Filters == null || rule.FSRule.AndOrExpression == null)
                    {
                        continue;
                    }

                    var engine = new FilterEngine(rule.FSRule.Filters, rule.FSRule.AndOrExpression, true);
                    if (engine.IsQualified(info))
                    {
                        return rule;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is PropertyNotAssignedException)
                    {
                        logger.Error("A property was not assigned while checking a file-system rule. Exception:{0}", ex.ToString());
                    }
                    //logger.Error("Checked expression failed. Expression:{0} ,Exception:{1}", rule.Compression, ex.ToString());
                    var thEX = new Exception(string.Format("Checked expression failed.{0}", rule.Compression), ex);
                    thEX.Data.Add("ruleName", rule.Name);
                    throw thEX;
                }
            }
            return null;
        }
    }
}
