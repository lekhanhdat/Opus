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
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Services;
using System.Collections.Concurrent;

namespace RAGoogle.Util
{
    public class RuleManager
    {
        private static readonly IRALogger _logger = RALogger.GetInstance(typeof(RuleManager));
        private ITermRuleAssociationDao AssociationDao => PlatformWindsorManager.GetService<ITermRuleAssociationDao>();
        private ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private IRuleManagerService RuleManagerService => PlatformWindsorManager.GetService<IRuleManagerService>();

        private readonly Dictionary<int, List<Rule>> _termRuleAssociatedCache;

        private ConcurrentDictionary<Guid, List<Rule>> _labelUniqueIdRuleAssociatedCache;

        private Dictionary<Guid, RMRuleInfos> _ruleInfoCache;
        private Dictionary<int, Rule> ruleCollection;
        private readonly object _locker;

        public RuleManager()
        {
            _termRuleAssociatedCache = new();
            _labelUniqueIdRuleAssociatedCache = new();
            _ruleInfoCache = new();
            _locker = new();
        }

        public RuleManager(Dictionary<int, Rule> ruleCollection)
        {
            this.ruleCollection = ruleCollection;
        }

        public async Task InitRulesInfoAsync()
        {
            using (var performance = new PerformanceScope("Report.GetRules"))
            {
                var dbRules = await RuleManagerService.GetSimpleRecordsRulesFromDBAsync();
                if (dbRules.Count > 0)
                {
                    _ruleInfoCache = dbRules.ToDictionary(key => new Guid(key.RuleId), value => value);
                }
            }
        }

        public bool TryGetRuleInfo(Guid id, out RMRuleInfos ruleInfo)
        {
            return _ruleInfoCache.TryGetValue(id, out ruleInfo);
        }

        public ConcurrentDictionary<string, Rule> LoadRules()
        {
            ConcurrentDictionary<string, Rule> ruleDics = [];
            using (var performance = new PerformanceScope("Report.LoadRules"))
            {
                var dbRules = RuleManagerService.GetRulesFromRecords();
                if (dbRules.Count > 0)
                {
                    _logger.Info("Begin to load rules.");
                    ruleDics = new ConcurrentDictionary<string, Rule>(dbRules.Where(r => r.GoogleDriveRule != null && r.GoogleDriveRule.SOFilters.Count > 0).ToDictionary(r => r.Id));
                    _logger.Info("Loaded {0} rules.", ruleDics.Count);
                }
                return ruleDics;
            }
        }

        public Dictionary<int, List<Rule>>? GetAssociatedRuleAsync(string labelId, string? tenantId, bool isDisposalRule = false)
        {
            Dictionary<int, List<Rule>>? associatedRules = new();

            if (string.IsNullOrEmpty(labelId))
            {
                _logger.Error($"LabelId is null or empty.");
                return associatedRules;
            }

            using (var performance = new PerformanceScope("GoogleRuleManager:TryGetLabelRuleAssociated", "", true))
            {
                var rmTerms = new List<RMTerm>();
                if (tenantId.IsNotNullOrEmpty())
                {
                    var term = TermDao.GetRMTermByLabelId(labelId, tenantId);
                    if (term != null)
                    {
                        rmTerms.Add(term);
                    }
                }
                else
                {
                    rmTerms = TermDao.GetRMTermsByLabelId(labelId);
                }

                if (rmTerms.IsNullOrEmpty())
                {
                    _logger.Error($"Label {labelId} applied on item has not been synced.");
                    return associatedRules;
                }
                foreach (var rmTerm in rmTerms)
                {
                    int termId = rmTerm.Id;

                    lock (_locker)
                    {
                        if (_termRuleAssociatedCache.TryGetValue(termId, out List<Rule>? rules))
                        {
                            associatedRules.Add(termId, rules);
                            continue;
                        }

                        _logger.Info($"Can't find term [{rmTerm.Name}] associated rules from cache.");

                        var associatedTermRuleInfos = AssociationDao.GetTermRuleInfoByTermid(rmTerm.Id);
                        if (associatedTermRuleInfos.Count == 0)
                        {
                            _logger.Warn($"Current term [{rmTerm.Name}] not found associated rule infoes.");
                            _termRuleAssociatedCache[termId] = new List<Rule>();
                            continue;
                        }

                        var ruleIds = associatedTermRuleInfos.Select(r => r.RuleId).ToList();
                        rules = RuleManagerService.GetRulesByIds(ruleIds);
                        rules = rules.Where(r => r.GoogleDriveRule != null)
                            .OrderBy(r => associatedTermRuleInfos.First(i => i.RuleId.ToString() == r.Id).RuleOrder)
                            .ToList();
                        if (rules.Count == 0)
                        {
                            _logger.Warn($"Current term related rules not found in record.");
                            _termRuleAssociatedCache[termId] = new List<Rule>();
                            continue;
                        }
                        if (isDisposalRule)
                        {
                            RebuildRuleAsync(rules);
                        }
                        _termRuleAssociatedCache[termId] = rules;
                        associatedRules.Add(termId, rules);
                    }
                }
                return associatedRules;
            }
        }
        public List<Rule>? GetRelatedRulesByLabelUniqueId(Guid labelUniqueId)
        {
            List<Rule>? rules = null;
            if (_labelUniqueIdRuleAssociatedCache.TryGetValue(labelUniqueId, out rules))
            {
                return rules;
            }
            using (var performance = new PerformanceScope("GoogleRuleManager:TryGetLabelRuleAssociated", "", true))
            {

                _logger.Info($"Can't find label [{labelUniqueId}] associated rules from cache.");

                var associatedLabelRuleInfos = AssociationDao.GetTermRuleInfoByTermUniqueId(labelUniqueId);
                if (associatedLabelRuleInfos.Count == 0)
                {
                    _logger.Warn($"Current label [{labelUniqueId}] not found associated rule infoes.");
                    _labelUniqueIdRuleAssociatedCache[labelUniqueId] = new List<Rule>();
                    return rules;
                }

                var ruleIds = associatedLabelRuleInfos.Select(r => r.RuleId).ToList();
                rules = RuleManagerService.GetRulesByIds(ruleIds);
                rules = rules.Where(r => r.GoogleDriveRule != null)
                    .OrderBy(r => associatedLabelRuleInfos.First(i => i.RuleId.ToString() == r.Id).RuleOrder)
                    .ToList();
                if (rules.Count == 0)
                {
                    _logger.Warn($"Current term related rules not found in record.");
                    _labelUniqueIdRuleAssociatedCache[labelUniqueId] = new List<Rule>();
                    return rules;
                }

                _labelUniqueIdRuleAssociatedCache[labelUniqueId] = rules;
            }
            return rules;
        }
        public async Task<bool> ApplyRuleInfo(Record record)
        {
            record.RuleId = Guid.Empty;
            record.RuleLevel = (int)PolicyLevel.None;
            record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
            record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;

            var rules = GetRelatedRulesByLabelUniqueId(record.TermId);
            if (rules == null)
            {
                _logger.Warn($"The item [{record.Id}-{record.TermId}] is not found any rule.");
                return false;
            }
            var (ruleInfo, dueDate) = MatchedPotentialRule(record.ConvertToGoogleItemInfo(), rules, true);
            if (ruleInfo == null)
            {
                _logger.Warn($"The item [{record.Id} - {record.TermId}] is not match any rule.");
                return false;
            }
            record.RuleId = Guid.Parse(ruleInfo.Id);
            record.RuleLevel = (int)ruleInfo.PolicyLevel;
            record.DisposalDueDate = record.PreviosDisposalDueDate = dueDate == default ? AvePoint.RA.Contract.Common.DueDateUtil.NextJob : DateTime.UtcNow.Add(dueDate).Ticks;
            return true;
        }
        public Tuple<Rule, TimeSpan> MatchedPotentialRule(ObjectInfoBase obj, List<Rule> rules, bool checkActionDueDate = false)
        {
            if (rules == null || obj == null)
            {
                return new Tuple<Rule, TimeSpan>(null, default(TimeSpan));
            }

            var rule = CheckCriteria(obj, rules);
            if (rule != null)
            {
                return new Tuple<Rule, TimeSpan>(rule, default(TimeSpan));
            }
            else if (checkActionDueDate)
            {
                var potentialRules = rules.Where(t => t.GoogleDriveRule.Filters != null && t.GoogleDriveRule.Filters.Any(f => f.Condition == PolicyCondition.OlderThan)).ToList();

                foreach (var pr in potentialRules)
                {
                    try
                    {
                        Dictionary<string, TimeSpan> offsets = ComputeCheckRuleOffsets(obj, pr);
                        var tObj = ObjectConverter.CloneFilterObject(obj, offsets);
                        var engine = new FilterEngine(pr.GoogleDriveRule.Filters, pr.GoogleDriveRule.AndOrExpression, true);
                        if (tObj != null && engine.IsQualified(tObj))
                        {
                            return new Tuple<Rule, TimeSpan>(pr, ComputeActionDueDateOffsets(obj, pr));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is PropertyNotAssignedException)
                        {
                            _logger.Error("A property was not assigned while checking a Google Drive rule. Exception:{0}", ex.ToString());
                        }
                        throw new Exception(string.Format("Checked expression failed.{0}", rule?.Compression), ex);
                    }
                }
            }
            return new Tuple<Rule, TimeSpan>(rule, default(TimeSpan));
        }

        private Rule CheckCriteria(ObjectInfoBase info, List<Rule> rules)
        {
            foreach (var rule in rules)
            {
                try
                {
                    _logger.Info($"rule name:{rule.Id}");
                    if (rule.GoogleDriveRule.Filters == null || rule.GoogleDriveRule.AndOrExpression == null)
                    {
                        _logger.Info($"continue rule name:{rule.Id}");
                        continue;
                    }

                    var engine = new FilterEngine(rule.GoogleDriveRule.Filters, rule.GoogleDriveRule.AndOrExpression, true);
                    if (engine.IsQualified(info))
                    {
                        _logger.Info($"match rule:{rule.Id}");
                        return rule;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is PropertyNotAssignedException)
                    {
                        _logger.Error("A property was not assigned while checking a Google Drive rule. Exception:{0}", ex.ToString());
                    }
                    var thEX = new Exception(string.Format("Checked expression failed.{0}", rule.Compression), ex);
                    thEX.Data.Add("ruleName", rule.Name);
                    throw thEX;
                }
            }
            return null;
        }

        /// <summary>
        /// Computes the remaining time based on rule conditions and google time attributes (Last Accessed Time, Created Time, Modified Time).
        /// The remaining time is calculated based on the "OlderThan" conditions of the rule, compared to the google time attributes.
        /// 
        /// The result is returned as a dictionary, where the key is the name of the google time attributes and the value is the corresponding remaining time.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="potentialRule"></param>
        /// <returns></returns>
        private Dictionary<string, TimeSpan> ComputeCheckRuleOffsets(ObjectInfoBase obj, Rule potentialRule)
        {
            bool isAndExpression = potentialRule.GoogleDriveRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
            var now = DateTime.UtcNow;
            Dictionary<string, TimeSpan> finalOffsets = new Dictionary<string, TimeSpan>();
            Dictionary<string, List<TimeSpan>> ruleOffsets = new Dictionary<string, List<TimeSpan>>();
            Dictionary<string, List<DateTime>> originalTimes = new Dictionary<string, List<DateTime>>();
            foreach (var filter in potentialRule.GoogleDriveRule.Filters)
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
                if (obj is GoogleItemInfo)
                {
                    var file = obj as GoogleItemInfo;
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
        /// Computes the action due date based on rule conditions and google time attributes (Last Accessed Time, Created Time, Modified Time).
        /// The remaining time is calculated based on the "OlderThan" conditions of the rule, compared to the google time attributes.
        /// 
        /// The result is returned as the remaining time for the action due date, based on the applied rule.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="potentialRule"></param>
        /// <returns></returns>
        private TimeSpan ComputeActionDueDateOffsets(ObjectInfoBase obj, Rule potentialRule)
        {
            bool isAndExpression = potentialRule.GoogleDriveRule.AndOrExpression.FirstOrDefault().Value.IndexOf("And") != -1;
            var now = DateTime.UtcNow;
            List<TimeSpan> ruleOffsets = new List<TimeSpan>();
            foreach (var filter in potentialRule.GoogleDriveRule.Filters)
            {
                if (filter.Condition == PolicyCondition.OlderThan)
                {
                    var value = int.Parse(filter.Value.Value1);
                    if (filter.Value.Value1Unit == PolicyValueUnit.Years)
                    {
                        if (obj is GoogleItemInfo)
                        {
                            var file = obj as GoogleItemInfo;
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
                        if (obj is GoogleItemInfo)
                        {
                            var file = obj as GoogleItemInfo;
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
                        if (obj is GoogleItemInfo)
                        {
                            var file = obj as GoogleItemInfo;
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
                        if (obj is GoogleItemInfo)
                        {
                            var file = obj as GoogleItemInfo;
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

        private void RebuildRuleAsync(List<Rule> rules)
        {
            if (rules.IsNotNullOrEmpty())
            {
                foreach (var rule in rules)
                {
                    try
                    {
                        RebuildRecordsMoveSetting(rule);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Rebuild disposal rules '{0}' error. Inner exception: {1}", rule.Name, ex.ToString());
                    }
                }
            }
        }

        public void RebuildRecordsMoveSetting(Rule rule)
        {
            if (rule.GoogleDriveRule.spMoveOption is { MoveDestination: not null } && !string.IsNullOrEmpty(rule.GoogleDriveRule.spMoveOption.MoveDestination.DestinationId))
            {
                rule.GoogleDriveRule.MoveToRecordCenterAndDelareSetting = new MoveToRecordCenterAndDelareSetting
                {
                    DestinationLocation = new DestinationLocationInfo
                    {
                        DestinationId = rule.GoogleDriveRule.spMoveOption.MoveDestination.DestinationId,
                        GoogleTreeNode = rule.GoogleDriveRule.spMoveOption.MoveDestination.GoogleTreeNode
                    }
                };
            }
        }

        public (Rule? rule, RMTerm? term) CalculateMatchedPotentialRule(RMAosGoogleAppProfile appProfile, GoogleItemData item, GoogleDriveTreeNodeDto selectedNode, RMGoogleSetting setting)
        {
            try
            {
                using (CheckJobStopScope jScope = new())
                {
                    using (GoogleDriveService service = new(appProfile, item.MemberEmail))
                    {
                        var itemInfo = item.ConvertToInfo();
                        Tuple<Rule, TimeSpan>? matchedRule = null;
                        int matchedTermId = -1;
                        List<int> aveLabelIds = [];
                        Dictionary<int, List<Rule>>? associatedRules = null;

                        foreach (var label in item.MetaInfo.Labels)
                        {
                            associatedRules = GetAssociatedRuleAsync(label.Id, selectedNode.TenantId, true);
                            if (associatedRules.IsNullOrEmpty())
                            {
                                continue;
                            }
                            matchedTermId = associatedRules.FirstOrDefault().Key;
                            foreach (var associatedRule in associatedRules)
                            {
                                matchedRule = MatchedPotentialRule(itemInfo, associatedRule.Value);
                                if (matchedRule.Item1 != null)
                                {
                                    matchedTermId = associatedRule.Key;
                                    break;
                                }
                            }
                            if (matchedRule?.Item1 != null && matchedTermId > 0)
                            {
                                break;
                            }
                        }

                        RMTerm? rmTerm = null;
                        if (matchedTermId > 0)
                        {
                            rmTerm = TermDao.GetRMTermByTermId(matchedTermId);
                        }

                        return (matchedRule?.Item1, rmTerm);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn($"An error occurred while calculate matched rule [{item.Name}]. Error: {ex}");
                throw;
            }
        }
    }
}
