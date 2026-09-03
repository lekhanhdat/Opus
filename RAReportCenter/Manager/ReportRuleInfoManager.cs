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
using RAReportCenter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.Manager
{
    public class ReportRuleInfoManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ReportRuleInfoManager));

        private static readonly IRuleManagerService RuleManagerService = PlatformWindsorManager.GetService<IRuleManagerService>();

        private static readonly Dictionary<string, Dictionary<SourceFlag, ReportRuleModel>> RuleInfos =
            new Dictionary<string, Dictionary<SourceFlag, ReportRuleModel>>();

        private static readonly Dictionary<SourceFlag, Func<SourceFlag, RMRuleInfos, ReportRuleModel>> SourceGetRuleMethods =
            new Dictionary<SourceFlag, Func<SourceFlag, RMRuleInfos, ReportRuleModel>>
            {
                { SourceFlag.SharePoint, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo)},
                //{ SourceFlag.SharePointOnPrem, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.SPLocalRule)},
                //{ SourceFlag.Exchange, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.EXORule)},
                //{ SourceFlag.FileSystem, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.FSRule)},
                //{ SourceFlag.OneDrive, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.OneDriveRule)},
                //{ SourceFlag.Physical, (flag, ruleInfo) => AssemblyRuleModel(flag, ruleInfo.RuleId, ruleInfo.RuleName, ruleInfo.DisposalClass, ruleInfo.PhysicalRule)},
            };

        private static readonly Dictionary<SourceFlag, Func<RMRuleInfos, int>> SourceGetRuleActionMethods =
            new Dictionary<SourceFlag, Func<RMRuleInfos, int>>
            {
                { SourceFlag.SharePoint,  SharePointOnlineGetRuleAction}
            };

        public static async Task<(bool,ReportRuleModel)> TryGetAsync(SourceFlag flag, string ruleId)
        {
            ReportRuleModel ruleInfo = null;

            try
            {
                if (string.IsNullOrEmpty(ruleId))
                {
                    throw new ArgumentNullException("[ruleId]");
                }

                if (!RuleInfos.TryGetValue(ruleId, out var sourceRules))
                {
                    sourceRules = await LoadRuleInfoAsync(ruleId);
                    RuleInfos.Add(ruleId, sourceRules);
                }

                ruleInfo = sourceRules[flag];

                return (true, ruleInfo);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get rule info by id: [{ruleId}]. Error: {e}");
                return (false, ruleInfo);
            }
        }

        private static async Task<Dictionary<SourceFlag, ReportRuleModel>> LoadRuleInfoAsync(string ruleId)
        {
            var result = new Dictionary<SourceFlag, ReportRuleModel>();

            var rule = await RuleManagerService.LoadRuleAsync(ruleId);
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

        private static ReportRuleModel AssemblyRuleModel(SourceFlag flag, string ruleId, string ruleName, string disposalClass, RMRuleInfos ruleInfo)
        {
            if (ruleInfo == null)
            {
                Logger.Warn($"Can't find [{flag}] rule by id: [{ruleId}].");
                return new ReportRuleModel
                {
                    Source = flag
                };
            }

            var result = new ReportRuleModel
            {
                Source = flag,
                RuleId = ruleId,
                RuleName = ruleName,
                RuleDisposalClass = disposalClass,
                RuleAction = SourceGetRuleActionMethods[flag](ruleInfo),
                EnableManualApprova = ruleInfo.EnableManualApproval,
                ExportType = RMExportTypeValue.None,
            };

            if (ruleInfo.EnableExport && ruleInfo.ExportInfo != null)
            {
                result.ExportType = (RMExportTypeValue)ruleInfo.ExportInfo.exportType;
            }

            return result;
        }

        private static int SharePointOnlineGetRuleAction(RMRuleInfos ruleInfo)
        {
            return 0;
        }
    }
}
