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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.Service.Services.AzureFileShare.RuleManagement;
using AvePoint.RA.Service.Services.CustomizeConnector.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CustomizeConnector.RuleManagement
{
    public class ConnectorTermRuleInfoManagement
    {
        private readonly RALogger Logger = RALogger.GetInstance(typeof(ConnectorTermRuleInfoManagement));

        private ITermRuleAssociationDao TermRuleAssociationDao =>
    PlatformWindsorManager.GetService<ITermRuleAssociationDao>();

        private IRuleManagerService RuleManagerService =>
    PlatformWindsorManager.GetService<IRuleManagerService>();

        private readonly Dictionary<string, List<Rule>> RuleCache =
            new();

        private readonly object Locker = new object();

        private bool TryGetTermRelatedRule(string termId, out List<Rule> rules)
        {
            if (!RuleCache.TryGetValue(termId, out rules))
            {
                lock (Locker)
                {
                    if (!RuleCache.TryGetValue(termId, out rules))
                    {
                        Logger.Info($"Can't find term [{termId}] related rule from cache.");
                        var termRelatedRuleInfoes = TermRuleAssociationDao.GetTermRuleInfoByTermUniqueId(new Guid(termId));
                        if (termRelatedRuleInfoes.Count == 0)
                        {
                            Logger.Warn($"Current term [{termId}] not found related rule infoes.");
                            RuleCache[termId] = new List<Rule>();
                            return false;
                        }

                        var ruleIds = termRelatedRuleInfoes.Select(item => item.RuleId).ToList();
                        Logger.Info($"Term [{termId}] related rules [{string.Join(", ", ruleIds)}].");
                        rules = RuleManagerService.GetRulesByIds(ruleIds);
                        rules = rules.Where(item => item.ConnectorRule != null).OrderBy(item => termRelatedRuleInfoes.First(i => i.RuleId.ToString() == item.Id).RuleOrder).ToList();
                        if (rules.Count == 0)
                        {
                            Logger.Warn($"Current term related rules not found in record.");
                            RuleCache[termId] = new List<Rule>();
                            return false;
                        }

                        RuleCache[termId] = rules;
                    }
                }
            }

            return rules.Count != 0;
        }

        public void ApplyRule(Record record, Dictionary<string, object> rulePolicyValues)
        {
            record.RuleId = Guid.Empty;
            record.RuleLevel = (int)PolicyLevel.None;
            record.DisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
            record.PreviosDisposalDueDate = AvePoint.RA.Contract.Common.DueDateUtil.None;
            record.IsManualSynced = false;
            record.DisposalStatus = (int)SOApproveDBStatus.None;
            record.ExportToRECO = false;
            record.ManualReviewer = Array.Empty<int>();

            if (record.TermId == Guid.Empty)
            {
                return;
            }

            if (!TryGetTermRelatedRule(record.TermId.ToString(), out var termRelatedRules))
            {
                Logger.Warn($"The term [{record.TermId}] is not related rules.");
                return;
            }

            var matchedRule = new ConnectorRuleManagement(termRelatedRules).MatchPotentialRule(ConnectorRecordCoverter.ConvertRecord2DocumentInfo(record, rulePolicyValues), true);
            if (matchedRule == null)
            {
                Logger.Warn($"The item [{record.Id}] is not match any rule.");
                return;
            }

            var ruleInfo = matchedRule.Item1;
            var dueDate = matchedRule.Item2;
            if(ruleInfo.ConnectorRule.IsManualApproval)
            {
                record.ExportToRECO = false;
                record.DisposalStatus = dueDate == default ? (int)SOApproveDBStatus.WaitingApprove : (int)SOApproveDBStatus.None;
            }
            record.RuleId = string.IsNullOrEmpty(ruleInfo.Id) ? Guid.Empty : new Guid(ruleInfo.Id);
            record.RuleLevel = (int)ruleInfo.PolicyLevel;
            var temp = DateTime.UtcNow.Ticks;
            record.DisposalDueDate = record.PreviosDisposalDueDate = dueDate == default ? AvePoint.RA.Contract.Common.DueDateUtil.NextJob : DateTime.UtcNow.Add(dueDate).Ticks;
            if (record.HoldStatus)
            {
                if (record.DisposalDueDate == AvePoint.RA.Contract.Common.DueDateUtil.NextJob || record.DisposalDueDate < record.HoldReleaseTime)
                {
                    temp = record.DisposalDueDate;
                    record.DisposalDueDate = record.HoldReleaseTime;
                    record.PreviosDisposalDueDate = temp;
                }
            }
        }
    }
}
