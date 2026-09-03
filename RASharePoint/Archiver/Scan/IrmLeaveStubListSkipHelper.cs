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
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.RA.SharePoint.Archiver
{
    internal static class IrmLeaveStubListSkipHelper
    {
        internal const string SkipReportMessageKey = "RM_SPS_Scanner_SkipLibraryForIrmRejectWithLeaveStub";

        internal static bool ShouldSkipItem(ScheduleConfiguration configuration, IAveList aveList, Rule matchedRule)
        {
            var isIrmRejectList = IsIrmRejectList(aveList);
            var isEnforceRuleActionsJob = IsEnforceRuleActionsJob(configuration);
            var isEnforceRuleActionsItemRule = IsEnforceRuleActionsItemRule(matchedRule);
            return isIrmRejectList
                && isEnforceRuleActionsJob
                && isEnforceRuleActionsItemRule;
        }

        internal static bool TryGetListLevelMatchedRule(ScheduleConfiguration configuration, IAveList aveList, out Rule matchedRule)
        {
            matchedRule = null;

            if (configuration == null || aveList == null)
            {
                return false;
            }

            if (!IsIrmRejectList(aveList))
            {
                return false;
            }

            if (configuration.RuleCollection != null)
            {
                foreach (var rule in configuration.RuleCollection.Values)
                {
                    if (IsListLevelMatchedRule(configuration, rule))
                    {
                        matchedRule = rule;
                        return true;
                    }
                }
            }

            if (IsListLevelMatchedRule(configuration, configuration.currentRule))
            {
                matchedRule = configuration.currentRule;
                return true;
            }

            return false;
        }

        private static bool IsIrmRejectList(IAveList aveList)
        {
            return aveList != null && aveList.IrmEnabled && aveList.IrmReject;
        }

        private static bool IsEnforceRuleActionsJob(ScheduleConfiguration configuration)
        {
            return configuration?.jobtype == AvePoint.RA.Contract.JobMonitor.JobType.RecordsDisposal
                || configuration?.jobtype == AvePoint.RA.Contract.JobMonitor.JobType.OneDriveRecordsDisposal;
        }

        private static bool IsListLevelMatchedRule(ScheduleConfiguration configuration, Rule rule)
        {
            if (!IsDocumentLevelRule(rule))
            {
                return false;
            }

            if (HasDestroyWithLeaveStub(rule))
            {
                return true;
            }

            return IsEnforceRuleActionsJob(configuration) && HasMoveWithLeaveStub(rule);
        }

        private static bool IsEnforceRuleActionsItemRule(Rule rule)
        {
            return IsDocumentLevelRule(rule)
                && (HasDestroyWithLeaveStub(rule) || HasMoveWithLeaveStub(rule) || HasLinkDocumentWithLeaveStub(rule));
        }

        private static bool IsDocumentLevelRule(Rule rule)
        {
            return rule != null && rule.PolicyLevel == PolicyLevel.Document;
        }

        private static bool HasDestroyWithLeaveStub(Rule rule)
        {
            return (rule.KeepDataOption & (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub)
                == (int)KeepDataOption.ArchiveBackupAndRemoveLeaveStub;
        }

        private static bool HasMoveWithLeaveStub(Rule rule)
        {
            return (rule.KeepDataOption & (int)KeepDataOption.ArchiveAndLeaveStub)
                == (int)KeepDataOption.ArchiveAndLeaveStub;
        }

        private static bool HasLinkDocumentWithLeaveStub(Rule rule)
        {
            return (rule.KeepDataOption & (int)KeepDataOption.LinkDocument)
                == (int)KeepDataOption.LinkDocument;
        }
    }
}
