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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMRuleManageMent;
using AvePoint.RA.Contract.TaxonomyModel;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.RA.Contract.RMWeb.Audit;

namespace AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler
{
    public class ContentRepositoryAuditUtil
    {
        public static Guid NeedReAuditorInAfter = new Guid("939C582E-73F3-4940-AFED-9F92629C9797");
        public static string GetApplyTermMethodString(DeployTermMethod method, bool isExoSource = false)
        {
            string result = string.Empty;
            switch (method)
            {
                case DeployTermMethod.UseDefaultTerm:
                    result = isExoSource ? "RM_JS_EXO_SetPresetTerm" : "RM_JS_SPS_AutoClassification_UseDefault";
                    break;
                case DeployTermMethod.UseAutoClassification:
                    result = "RM_JS_SPS_AutoClassification_UseRule";
                    break;
                case DeployTermMethod.NoDefaultTerm:
                    result = "RM_JS_SPS_AutoClassification_NoDefaultValue";
                    break;
                case DeployTermMethod.UseIntelligenceClassification:
                    result = "RM_MachineLearning_DeployTermMethodIntelligence";
                    break;
                default:
                    break;
            }
            return result;
        }
        public static string GetApplyLabelMethodString(DeployLabelMethod method) =>
            method switch
            {
                DeployLabelMethod.UseAutoClassification => "RM_JS_SPS_Label_AutoClassification_UseRule",
                DeployLabelMethod.UseManualClassification => "RM_JS_SPS_Label_AutoClassification_NoDefaultValue",
                DeployLabelMethod.UseIntelligenceClassification => "RM_MachineLearning_DeployTermMethodIntelligence",
                _ => string.Empty
            };

        public static string GetSkipOverrideString(AutoJobOption skipOverride)
        {
            string result = string.Empty;
            switch (skipOverride)
            {
                case AutoJobOption.None:
                    break;
                case AutoJobOption.SkipAndKeep:
                    result = "RM_JS_SPS_AutoClassification_SkipOverrideOption_Skip";
                    break;
                case AutoJobOption.Override:
                    result = "RM_JS_SPS_AutoClassification_SkipOverrideOption_Override";
                    break;
                case AutoJobOption.Append:
                    result = "RM_JS_SPS_AutoClassification_AppendLabel";
                    break;
                default:
                    break;
            }
            return result;
        }
        public static string GetFilterGroupCretiaStr(List<FilterGroup> filterGroups)
        {
            string result = string.Empty;
            foreach (var filterGroup in filterGroups)
            {
                foreach (var filter in filterGroup.Filters)
                {
                    //result += System.Web.HttpUtility.HtmlEncode(filter.FilterCretia) + "<br>";
                    result += filter.FilterCretia + "<br>";
                }
                result += GetFilterGroupCretiaStr(filterGroup.FilterGroups);
            }
            return result;
        }
        public static string GetRulesCretiaString(List<ClassificationRule> autoRules)
        {
            string result = string.Empty;
            List<ClassificationRule> normalAutoRules = autoRules.Where(r => !r.IsDefaultRule).ToList();
            for (int i = 0; i < normalAutoRules.Count; i++)
            {
                var autoRule = normalAutoRules[i];
                result += GetFilterGroupCretiaStr(autoRule.FilterGroups);
                if (!string.IsNullOrEmpty(autoRule.AndOrExpression))
                {
                    autoRule.AndOrExpression = autoRule.AndOrExpression.Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));
                    result += autoRule.AndOrExpression.Length == 1 ? $"({autoRule.AndOrExpression})" : autoRule.AndOrExpression;
                    result += "<br>";
                }
                result += "RM_JS_SPS_AutoClassification_DisplayPolicyApplyTerm " + autoRule.TermName;
                if (i == normalAutoRules.Count - 1)
                {
                    result += "<br>";
                }
            }

            List<ClassificationRule> defaultRule = autoRules.Where(r => r.IsDefaultRule).ToList();
            if (!defaultRule[0].NoDefaultTerm)
            {
                result += "RM_JS_SPS_AutoClassification_DisplayPolicyDefaultTerm " + defaultRule[0].TermName;
            }
            return result;
        }
        public static string GetRulesLabelCretiaString(List<ClassificationRule> autoRules)
        {
            string result = string.Empty;
            List<ClassificationRule> normalAutoRules = autoRules.Where(r => !r.IsDefaultRule).ToList();
            for (int i = 0; i < normalAutoRules.Count; i++)
            {
                var autoRule = normalAutoRules[i];
                result += GetFilterGroupCretiaStr(autoRule.FilterGroups);
                if (!string.IsNullOrEmpty(autoRule.AndOrExpression))
                {
                    autoRule.AndOrExpression = autoRule.AndOrExpression.Replace("And", I18NEntity.GetString("RM_JS_Rule_ConditionAnd")).Replace("Or", I18NEntity.GetString("RM_JS_Rule_ConditionOr"));
                    result += autoRule.AndOrExpression.Length == 1 ? $"({autoRule.AndOrExpression})" : autoRule.AndOrExpression;
                    result += "<br>";
                }
                result += "RM_JS_SPS_AutoClassification_DisplayPolicyApplyLabel " + autoRule.TermName;
                if (i == normalAutoRules.Count - 1)
                {
                    result += "<br>";
                }
            }

            List<ClassificationRule> defaultRule = autoRules.Where(r => r.IsDefaultRule).ToList();
            if (!defaultRule[0].NoDefaultTerm)
            {
                result += "RM_JS_SPS_AutoClassification_DisplayPolicyDefaultLabel " + defaultRule[0].TermName;
            }
            return result;
        }
    }

    public static class AuditHelper
    {
        public static void SaveAuditItem(RMAuditInfo info, string targetSetting, string oldValue, string newValue)
        {
            var item = new AuditItem { TargetSetting = targetSetting, OldValue = oldValue, NewValue = newValue };
            info.ModifyContent.Add(item);
        }
        public static void SaveOldAuditItem(RMAuditInfo info, string targetSetting, string oldValue)
        {
            var item = new AuditItem { TargetSetting = targetSetting, OldValue = oldValue };
            info.ModifyContent.Add(item);
        }

        public static void SaveNewAuditItem(RMAuditInfo info, string targetSetting, string newValue)
        {
            var item = info.ModifyContent.Where(o => o.TargetSetting != null && o.TargetSetting.Equals(targetSetting)).FirstOrDefault();
            if (item != null)
            {
                item.NewValue = newValue;
            }
            else
            {
                item = new AuditItem { TargetSetting = targetSetting, NewValue = newValue };
                info.ModifyContent.Add(item);
            }
        }

        public static void ReSaveOldAuditItem(RMAuditInfo info, string targetSetting, string oldValue)
        {
            var item = info.ModifyContent.Where(o => o.TargetSetting != null && o.TargetSetting.Equals(targetSetting)).FirstOrDefault();
            if (item != null)
            {
                item.OldValue = oldValue;
            }
            else
            {
                item = new AuditItem { TargetSetting = targetSetting, OldValue = oldValue };
                info.ModifyContent.Add(item);
            }
        }
    }
}
