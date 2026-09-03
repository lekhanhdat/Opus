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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using RAManualApprovalCommon.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApprovalCommon
{
    public class ManualApprovalRuleInfoManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalRuleInfoManager));

        private static readonly IRuleManagerService RuleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();

        private static readonly ConcurrentDictionary<string, Dictionary<SourceFlag, ManualApprovalRuleModel>> RuleInfos = 
            new ConcurrentDictionary<string, Dictionary<SourceFlag, ManualApprovalRuleModel>>();

        private static readonly Dictionary<SourceFlag, Func<SourceFlag, RMRuleInfos, ManualApprovalRuleModel>> SourceGetRuleMethods =
            new Dictionary<SourceFlag, Func<SourceFlag, RMRuleInfos, ManualApprovalRuleModel>>
            {
                { SourceFlag.SharePoint, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo)},
                { SourceFlag.SharePointOnPrem, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.SPLocalRule)},
                { SourceFlag.Exchange, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.EXORule)},
                { SourceFlag.FileSystem, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.FSRule)},
                { SourceFlag.OneDrive, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.OneDriveRule)},
                { SourceFlag.Physical, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.PhysicalRule)},
                { SourceFlag.AzureFileShare, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.AzureFileRule)},
                { SourceFlag.LifecycleRetention, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo)},
                { SourceFlag.Connector, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.ConnectorRule)},
                { SourceFlag.Box, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.BoxRule)},
                { SourceFlag.Google, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.GoogleDriveRule)},
                { SourceFlag.Teams, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo)},
            };

        public static bool TryGet(SourceFlag flag, string ruleId, out ManualApprovalRuleModel ruleInfo)
        {
            ruleInfo = null;

            try
            {
                if(string.IsNullOrEmpty(ruleId))
                {
                    throw new ArgumentNullException("[ruleId]");
                }

                if(!RuleInfos.TryGetValue(ruleId, out var sourceRules))
                {
                    sourceRules = LoadRuleInfo(ruleId);
                    if (!RuleInfos.TryAdd(ruleId, sourceRules))
                    {
                        Logger.Warn($"The rule: [{ruleId}] add to memory cache failed.");
                    }
                }

                ruleInfo = sourceRules[flag].DeepCopy();

                return true;                
            }
            catch(Exception e)
            {
                Logger.Error($"An error occurred while get rule info by id: [{ruleId}]. Error: {e}");
                return false;
            }
        }

        private static Dictionary<SourceFlag, ManualApprovalRuleModel> LoadRuleInfo(string ruleId)
        {
            using(new PerformanceScope($"Load rule: [{ruleId}]"))
            {
                var result = new Dictionary<SourceFlag, ManualApprovalRuleModel>();

                var rule = RuleManagerService.LoadRuleAsync(ruleId).GetAwaiter().GetResult();
                if (rule == null)
                {
                    throw new Exception($"Load rule by [{ruleId}] is empty.");
                }

                foreach (var sourceGetRuleMethod in SourceGetRuleMethods)
                {
                    result.Add(sourceGetRuleMethod.Key, sourceGetRuleMethod.Value(sourceGetRuleMethod.Key, rule));
                }

                return result;
            }
        }

        private static ManualApprovalRuleModel AssemblyRuleModel(SourceFlag flag, string ruleId, string ruleName, string disposalClass, RMRuleInfos ruleInfo)
        {
            if (ruleInfo == null)
            {
                Logger.Warn($"Can't find [{flag}] rule by id: [{ruleId}].");
                return new ManualApprovalRuleModel
                {
                    Flag = flag
                };
            }

            AvePoint.GCommon.Contract.StorageOptimization.Object.RetentionInfo retentionInfo = null;
            if (ruleInfo.RetentionInfo != null)
            {
                retentionInfo = new AvePoint.GCommon.Contract.StorageOptimization.Object.RetentionInfo();
                retentionInfo.IsManualApproval = ruleInfo.RetentionInfo.IsManualApproval;
                retentionInfo.IsSendEamilToOwner = ruleInfo.RetentionInfo.IsSendEamilToOwner;
                retentionInfo.ReviewType = ruleInfo.RetentionInfo.ReviewType;
                retentionInfo.WorkflowId = ruleInfo.RetentionInfo.WorkflowId;
                retentionInfo.UserInfos = ruleInfo.RetentionInfo.UserInfos;
            }

            return new ManualApprovalRuleModel
            {
                IsHasRule = true,
                Flag = flag,
                RuleId = ruleId,
                RuleName = ruleName,
                RuleCriterias = string.Join(" ", ruleInfo.RuleCretias),
                RuleDisposalClass = disposalClass,
                WorkflowId = ruleInfo.WorkflowId,
                IsSendEmailToOwner = ruleInfo.IsSendEmailToOwner,
                Owners = RuleManagerService.Convert2RecordOwnerInfos(ruleInfo.Users),
                RetentionInfo = retentionInfo,
                RelatedRecordOption = ruleInfo.RelatedRecordOption,
                IsGControlWorkflow = ruleInfo.IsGControlManualApproval
            };
        }
    }
}
