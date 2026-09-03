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
using AvePoint.RA.Contract.Discovery.Model.Query;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Discovery.Office365.Converter
{
    public class RMDiscoveryRuleTreeConverter
    {
        public static RMDiscoveryRotRuleDataInfo ConvertToTreeItem(List<RMDiscoveryRotRuleDataInfo> ruleInfo)
        {
            var resultRule = new RMDiscoveryRotRuleDataInfo();
            var ruleDic = ruleInfo.GroupBy(r => r.Category).ToDictionary(rule => rule.Key, rule => rule.OrderBy(item => item.Label).ToList());
            var rotTotalSize = ruleInfo.Sum(rule => rule.FileTotalSize);
            resultRule.Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_RootNode");
            resultRule.FileTotalSize = rotTotalSize;
            resultRule.Expand = true;
            var rTotalSize = ruleDic.TryGetValue(RMDiscoveryRuleCategory.Redundant, out var rRule);
            if (rTotalSize)
            {
                var redundantRules = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_Redundant"),
                    FileTotalSize = rRule.Sum(rule => rule.FileTotalSize),
                    Category = RMDiscoveryRuleCategory.Redundant,
                    Expand = true,
                    Children = rRule.ConvertAll(rule =>
                    {
                        var treeRule = new RMDiscoveryRotRuleDataInfo
                        {
                            Id = rule.Id,
                            FileTotalSize = rule.FileTotalSize,
                            Label = rule.Label,
                            Category = RMDiscoveryRuleCategory.Redundant,
                        };
                        return treeRule;
                    }),
                };
                resultRule.Children.Add(redundantRules);
            }

            var oTotalSize = ruleDic.TryGetValue(RMDiscoveryRuleCategory.Obsolete, out var oRule);
            if (oTotalSize)
            {
                var obsoleteRules = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_Obsolete"),
                    FileTotalSize = oRule.Sum(rule => rule.FileTotalSize),
                    Category = RMDiscoveryRuleCategory.Obsolete,
                    Expand = true,
                    Children = oRule.ConvertAll(rule =>
                    {
                        var treeRule = new RMDiscoveryRotRuleDataInfo
                        {
                            Id = rule.Id,
                            FileTotalSize = rule.FileTotalSize,
                            Label = rule.Label,
                            Category = RMDiscoveryRuleCategory.Obsolete,
                        };
                        return treeRule;
                    }),
                };
                resultRule.Children.Add(obsoleteRules);
            }

            var tTotalSize = ruleDic.TryGetValue(RMDiscoveryRuleCategory.Trivial, out var tRule);
            if (tTotalSize)
            {
                var trivialRules = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_Trivial"),
                    FileTotalSize = tRule.Sum(rule => rule.FileTotalSize),
                    Category = RMDiscoveryRuleCategory.Trivial,
                    Expand = true,
                    Children = tRule.ConvertAll(rule =>
                    {
                        var treeRule = new RMDiscoveryRotRuleDataInfo
                        {
                            Id = rule.Id,
                            FileTotalSize = rule.FileTotalSize,
                            Label = rule.Label,
                            Category = RMDiscoveryRuleCategory.Trivial,
                        };
                        return treeRule;
                    }),
                };
                resultRule.Children.Add(trivialRules);
            }

            return resultRule;
        }

        public static RMDiscoveryRotRuleDataInfo ConvertToFilterItem(List<RMDiscoveryRotRuleDataInfo> ruleInfo)
        {
            var resultRule = new RMDiscoveryRotRuleDataInfo();
            var ruleDic = ruleInfo.GroupBy(r => r.Category).ToDictionary(rule => rule.Key, rule => rule.ToList());
            resultRule.Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_RootNode");
            var rTotalSize = ruleDic.TryGetValue(RMDiscoveryRuleCategory.Redundant, out var rRule);
            if (rTotalSize)
            {
                var redundantRules = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_Redundant"),
                    Category = RMDiscoveryRuleCategory.Redundant,
                    Children = rRule.ConvertAll(rule =>
                    {
                        var treeRule = new RMDiscoveryRotRuleDataInfo
                        {
                            Id = rule.Id,
                            Label = rule.Label,
                            Category = RMDiscoveryRuleCategory.Redundant,
                        };
                        return treeRule;
                    }),
                };
                resultRule.Children.Add(redundantRules);
            }

            var oTotalSize = ruleDic.TryGetValue(RMDiscoveryRuleCategory.Obsolete, out var oRule);
            if (oTotalSize)
            {
                var obsoleteRules = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_Obsolete"),
                    Category = RMDiscoveryRuleCategory.Obsolete,
                    Children = oRule.ConvertAll(rule =>
                    {
                        var treeRule = new RMDiscoveryRotRuleDataInfo
                        {
                            Id = rule.Id,
                            Label = rule.Label,
                            Category = RMDiscoveryRuleCategory.Obsolete,
                        };
                        return treeRule;
                    }),
                };
                resultRule.Children.Add(obsoleteRules);
            }

            var tTotalSize = ruleDic.TryGetValue(RMDiscoveryRuleCategory.Trivial, out var tRule);
            if (tTotalSize)
            {
                var trivialRules = new RMDiscoveryRotRuleDataInfo
                {
                    Label = I18NEntity.GetString("RM_FA_ROTRule_TreeNode_Trivial"),
                    Category = RMDiscoveryRuleCategory.Trivial,
                    Children = tRule.ConvertAll(rule =>
                    {
                        var treeRule = new RMDiscoveryRotRuleDataInfo
                        {
                            Id = rule.Id,
                            Label = rule.Label,
                            Category = RMDiscoveryRuleCategory.Trivial,
                        };
                        return treeRule;
                    }),
                };
                resultRule.Children.Add(trivialRules);
            }

            return resultRule;
        }
    }
}
