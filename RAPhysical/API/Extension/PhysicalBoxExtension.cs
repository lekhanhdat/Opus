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
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.RAPhysical.Discover;
using AvePoint.RA.RAPhysical.Discover.DiscoverImps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.API
{
    public static  class PhysicalBoxExtension
    {
        private static IPhysicalDiscover _physicalfullDiscover = new PhysicalFullDiscover();
        private static IRuleManagerService _ruleManagerService =>  (IRuleManagerService)PlatformWindsorManager.GetService(typeof(IRuleManagerService));

        public static bool BoxUnderContainer(this IPhysicalBox box)
        {
            if (box.Ancestors != null && box.Ancestors.Count > 0 && box.ParentId != box.LocationId)
            {
                return true;
            }
            return false;
        }
        public static bool AreAllFolderRulesCalculateRule(this IPhysicalBox box)
        {
            var folders = _physicalfullDiscover.GetPhysicalFiles(box).ToList();

            var folderRuleIds = folders.Where(f => f.RuleId != Guid.Empty).Select(f => f.RuleId).Distinct().ToList();

            var ruleDict = _ruleManagerService.GetRulesByIds(folderRuleIds).ToDictionary(r => new Guid(r.Id));

            return folders.All(f =>
                f.RuleId != Guid.Empty &&
                ruleDict.TryGetValue(f.RuleId, out var folderRule) &&
                folderRule.PhysicalRule?.IsCalculationDisposalDate == true);
        }
        public static bool IsLastestSubFolderActionDueDateRule(this Rule rule)
        {
            if (rule?.PhysicalRule?.Filters == null)
            {
                return false;
            }

            return rule.PhysicalRule.Filters.Any(r => r.Rule is LastestFolderDisposalDueDateRule);
        }
        public static bool HasLatestFolderDisposalDueDateRule(this IEnumerable<Rule> rules)
        {
            if (rules == null) return false;

            return rules.Any(r => r.IsLastestSubFolderActionDueDateRuleBySOFilter());
        }
        public static bool IsLastestSubFolderActionDueDateRuleBySOFilter(this Rule rule)
        {
            if (rule?.PhysicalRule?.SOFilters == null)
            {
                return false;
            }
            return rule.PhysicalRule.SOFilters.Any(r => r.Rule is LastestFolderDisposalDueDateRule);
        }
    }

    public static class PhysicalFileExtension
    {
        public static bool FolderUnderContainer(this IPhysicalFile folder)
        {
            if (folder.Ancestors != null && folder.Ancestors.Count > 1)
            {
                if (folder.ParentId == folder.LocationId || folder.Ancestors[1] == folder.BoxId)
                {
                    //folder under location or location/box
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}
