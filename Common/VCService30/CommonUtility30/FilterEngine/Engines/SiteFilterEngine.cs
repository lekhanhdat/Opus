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




namespace AvePoint.Common.FilterEngine
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon;
    using System.Reflection;
    #endregion

    internal class SiteFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SiteFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            SiteInfo siteInfo = objectInfo as SiteInfo;

            if (policy.Rule is UrlRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteInfo.Url, policy.Value);
            }
            else if (policy.Rule is TitleRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteInfo.Title, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                bool isQualified= DateTimeConditionChecker.IsQualified(policy.Condition, siteInfo.Modified, policy.Value);
                RecordFilterLog("SiteInfo", isQualified, siteInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, siteInfo.Created, policy.Value);
            }
            else if (policy.Rule is OwnerRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByTitle, policy.Value);
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByTitle, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByTitle, policy.Value);
                }
            }
            else if (policy.Rule is TemplateRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteInfo.TemplateName, policy.Value);
            }
            else if (policy.Rule is TemplateIdRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteInfo.Template, policy.Value);
            }
            else if (policy.Rule is InheritanceRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, siteInfo.InheritPermission, policy.Value);
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is CustomPropertyTextRule)
            {
                if (!siteInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                string columnValue = siteInfo.ColumnInfos[policy.Rule.Value1].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is CustomPropertyNumberRule)
            {
                if (!siteInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                double columnValue;
                try
                {
                    columnValue = double.Parse(siteInfo.ColumnInfos[policy.Rule.Value1].ToString());
                }
                catch(Exception e)
                {
                    logger.Warn(e.ToString());
                    return false;
                }
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is CustomPropertyDateTimeRule)
            {
                //if (!siteInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfos[policy.Rule.Value1] || !siteInfo.ColumnInfos[policy.Rule.Value1].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                if (!siteInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                DateTime columnValue;
                if (!DateTime.TryParse(siteInfo.ColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
                {
                    return false;
                }
                if (columnValue.Kind != DateTimeKind.Utc)
                {
                    columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
                }
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is CustomPropertyBooleanRule)
            {
                if (!siteInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                bool columnValue = false;
                if (siteInfo.ColumnInfos[policy.Rule.Value1] is string)
                {
                    columnValue |= string.Equals("yes", siteInfo.ColumnInfos[policy.Rule.Value1] as string, StringComparison.OrdinalIgnoreCase);
                    columnValue |= string.Equals("true", siteInfo.ColumnInfos[policy.Rule.Value1] as string, StringComparison.OrdinalIgnoreCase);
                }
                else if (siteInfo.ColumnInfos[policy.Rule.Value1] is bool)
                {
                    columnValue = (bool)siteInfo.ColumnInfos[policy.Rule.Value1];
                }
                else
                {
                    return false;
                }
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is AuditingRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, siteInfo.EnableAuditing, policy.Value);
            }
            else if (policy.Rule is AnonymousAccessRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, siteInfo.EnableAnonymousAccess, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

    }
}
