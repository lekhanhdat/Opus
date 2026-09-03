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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.Explorer.Model;
using RABox.Converters;

namespace RABox.RuleManagement
{
    public class BoxRuleManagement
    {
        private static RALogger _logger = RALogger.GetInstance(typeof(BoxRuleManagement));
        private readonly List<Rule> _rules;

        public BoxRuleManagement(List<Rule> rules)
        {
            _rules = rules;
        }

        public Tuple<Rule, TimeSpan> MatchPotentialRule(ObjectInfoBase obj, bool checkActionDueDate = false)
        {
            if (_rules == null)
            {
                return null;
            }

            var rule = CheckCriteria(obj);
            if (rule != null)
            {
                return new Tuple<Rule, TimeSpan>(rule, default(TimeSpan));
            }
            else if (checkActionDueDate)
            {
                var potentialRules = _rules.Where(t => t.BoxRule.Filters != null && t.BoxRule.Filters.Any(f => f.Condition == PolicyCondition.OlderThan)).ToList();

                foreach (var pr in potentialRules)
                {
                    try
                    {
                        Dictionary<string, TimeSpan> offsets = ComputeCheckRuleOffsets(obj, pr);
                        var tObj = ObjectConverter.CloneFilterObject(obj, offsets);
                        var engine = new FilterEngine(pr.BoxRule.Filters, pr.BoxRule.AndOrExpression, true);
                        if (engine.IsQualified(tObj))
                        {
                            return new Tuple<Rule, TimeSpan>(pr, ComputeActionDueDateOffsets(obj, pr));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is PropertyNotAssignedException)
                        {
                            _logger.Error("A property was not assigned while checking a Box rule. Exception:{0}", ex.ToString());
                        }
                        throw new Exception(string.Format("Checked expression failed.{0}", rule?.Compression), ex);
                    }
                }
                return null;
            }
            return null;
        }

        private Rule CheckCriteria(ObjectInfoBase info)
        {
            foreach (var rule in _rules)
            {
                try
                {
                    if (rule.BoxRule.Filters == null || rule.BoxRule.AndOrExpression == null)
                    {
                        continue;
                    }

                    var engine = new FilterEngine(rule.BoxRule.Filters, rule.BoxRule.AndOrExpression, true);
                    if (engine.IsQualified(info))
                    {
                        return rule;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is PropertyNotAssignedException)
                    {
                        _logger.Error("A property was not assigned while checking a Box rule. Exception:{0}", ex.ToString());
                    }
                    var thEX = new Exception(string.Format("Checked expression failed.{0}", rule.Compression), ex);
                    thEX.Data.Add("ruleName", rule.Name);
                    throw thEX;
                }
            }
            return null;
        }

        public void ApplyRuleInfo(BoxItemInfo itemInfo, Record record)
        {
            var oldRule = record.RuleId;

            record.RuleId = Guid.Empty;
            record.RuleLevel = (int)PolicyLevel.None;
            record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
            record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;

            var matchedRule = MatchPotentialRule(itemInfo, true);

            if (matchedRule == null)
            {
                _logger.Warn($"The item [{record.Id} - {record.ExternalId}] is not match any rule.");
                return;
            }

            var ruleInfo = matchedRule.Item1;
            var dueDate = matchedRule.Item2;

            record.RuleId = string.IsNullOrEmpty(ruleInfo.Id) ? record.RuleId : new Guid(ruleInfo.Id);
            record.RuleLevel = (int)ruleInfo.PolicyLevel;
            record.DisposalDueDate = record.PreviosDisposalDueDate = dueDate == default ? AvePoint.RA.Contract.Common.DueDateUtil.NextJob : DateTime.UtcNow.Add(dueDate).Ticks;

            if (oldRule != Guid.Empty && record.RuleId != oldRule)
            {
                _logger.Info($"The item [{record.Id} - {record.ExternalId}] changed rule [{oldRule}] to [{record.RuleId}] ");
                record.RemoveManualProperties();
            }

            if (record.HoldStatus)
            {
                if (record.DisposalDueDate == AvePoint.RA.Contract.Common.DueDateUtil.NextJob)
                {
                    record.DisposalDueDate = record.HoldReleaseTime;
                    record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.NextJob;
                }
                if (record.DisposalDueDate < record.HoldReleaseTime)
                {
                    record.DisposalDueDate = record.HoldReleaseTime;
                    record.PreviosDisposalDueDate = record.HoldReleaseTime;
                }
            }
        }

        /// <summary>
        /// Computes the remaining time based on rule conditions and box time attributes (Last Accessed Time, Created Time, Modified Time).
        /// The remaining time is calculated based on the "OlderThan" conditions of the rule, compared to the box time attributes.
        /// 
        /// The result is returned as a dictionary, where the key is the name of the box time attributes and the value is the corresponding remaining time.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="potentialRule"></param>
        /// <returns></returns>
        private Dictionary<string, TimeSpan> ComputeCheckRuleOffsets(ObjectInfoBase obj, Rule potentialRule)
        {
            bool isAndExpression = potentialRule.BoxRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
            var now = DateTime.UtcNow;
            Dictionary<string, TimeSpan> finalOffsets = new Dictionary<string, TimeSpan>();
            Dictionary<string, List<TimeSpan>> ruleOffsets = new Dictionary<string, List<TimeSpan>>();
            Dictionary<string, List<DateTime>> originalTimes = new Dictionary<string, List<DateTime>>();
            foreach (var filter in potentialRule.BoxRule.Filters)
            {
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    if (!originalTimes.ContainsKey(filter.Rule.ToString()))
                    {
                        originalTimes[filter.Rule.Value1] = new List<DateTime>();
                    }
                    var value = int.Parse(filter.Value.Value1);
                    if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    {
                        originalTimes[filter.Rule.Value1].Add(now.AddYears(0 - value));
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                    {

                        originalTimes[filter.Rule.Value1].Add(now.AddMonths(0 - value));
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                    {
                        originalTimes[filter.Rule.Value1].Add(now.AddDays(0 - value * 7));
                    }
                    else
                    {
                        originalTimes[filter.Rule.Value1].Add(now.AddDays(0 - value));
                    }
                }
            }

            foreach (var originalTime in originalTimes)
            {
                if (!ruleOffsets.ContainsKey(originalTime.Key))
                {
                    ruleOffsets[originalTime.Key] = new List<TimeSpan>();
                }
                if (obj is BoxItemInfo)
                {
                    var file = obj as BoxItemInfo;
                    foreach (var time in originalTime.Value)
                    {
                        if (originalTime.Key == "Last Accessed Time" && file.AccessTime >= time)
                        {
                            ruleOffsets[originalTime.Key].Add(file.AccessTime - time);
                        }
                        if (originalTime.Key == "Created Time" && file.Created >= time)
                        {
                            ruleOffsets[originalTime.Key].Add(file.Created - time);
                        }
                        if (originalTime.Key == "Modified Time" && file.Modified >= time)
                        {
                            ruleOffsets[originalTime.Key].Add(file.Modified - time);
                        }
                    }
                }
            }

            foreach (var offset in ruleOffsets)
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
        /// Computes the action due date based on rule conditions and box time attributes (Last Accessed Time, Created Time, Modified Time).
        /// The remaining time is calculated based on the "OlderThan" conditions of the rule, compared to the box time attributes.
        /// 
        /// The result is returned as the remaining time for the action due date, based on the applied rule.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="potentialRule"></param>
        /// <returns></returns>
        private TimeSpan ComputeActionDueDateOffsets(ObjectInfoBase obj, Rule potentialRule)
        {
            bool isAndExpression = potentialRule.BoxRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
            var now = DateTime.UtcNow;
            List<TimeSpan> ruleOffsets = new List<TimeSpan>();
            foreach (var filter in potentialRule.BoxRule.Filters)
            {
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    var value = int.Parse(filter.Value.Value1);
                    if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    {
                        if (obj is BoxItemInfo)
                        {
                            var file = obj as BoxItemInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddYears(value) > now)
                            {
                                ruleOffsets.Add(file.AccessTime.AddYears(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddYears(value) > now)
                            {
                                ruleOffsets.Add(file.Created.AddYears(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddYears(value) > now)
                            {
                                ruleOffsets.Add(file.Modified.AddYears(value) - now);
                            }
                        }
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Months)
                    {
                        if (obj is BoxItemInfo)
                        {
                            var file = obj as BoxItemInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddMonths(value) > now)
                            {
                                ruleOffsets.Add(file.AccessTime.AddMonths(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddMonths(value) > now)
                            {
                                ruleOffsets.Add(file.Created.AddMonths(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddMonths(value) > now)
                            {
                                ruleOffsets.Add(file.Modified.AddMonths(value) - now);
                            }
                        }
                    }
                    else if (filter.Value.Value1Unit == PolicyValueUnit.Weeks)
                    {
                        if (obj is BoxItemInfo)
                        {
                            var file = obj as BoxItemInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddDays(value * 7) > now)
                            {
                                ruleOffsets.Add(file.AccessTime.AddDays(value * 7) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddDays(value * 7) > now)
                            {
                                ruleOffsets.Add(file.Created.AddDays(value * 7) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddDays(value * 7) > now)
                            {
                                ruleOffsets.Add(file.Modified.AddDays(value * 7) - now);
                            }
                        }
                    }
                    else
                    {
                        if (obj is BoxItemInfo)
                        {
                            var file = obj as BoxItemInfo;
                            if (filter.Rule.Value1 == "Last Accessed Time" && file.AccessTime.AddDays(value) > now)
                            {
                                ruleOffsets.Add(file.AccessTime.AddDays(value) - now);
                            }
                            if (filter.Rule.Value1 == "Created Time" && file.Created.AddDays(value) > now)
                            {
                                ruleOffsets.Add(file.Created.AddDays(value) - now);
                            }
                            if (filter.Rule.Value1 == "Modified Time" && file.Modified.AddDays(value) > now)
                            {
                                ruleOffsets.Add(file.Modified.AddDays(value) - now);
                            }
                        }
                    }
                }
            }
            ruleOffsets = ruleOffsets.Distinct().ToList();
            ruleOffsets.Sort();
            _logger.Debug("ComputeActionDueDateOffsets:{0}", string.Concat<TimeSpan>(ruleOffsets));
            if (isAndExpression)
            {
                return ruleOffsets.LastOrDefault();
            }
            else
            {
                return ruleOffsets.FirstOrDefault();
            }
        }
    }
}
