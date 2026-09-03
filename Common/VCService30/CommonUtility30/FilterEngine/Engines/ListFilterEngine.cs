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

    internal class ListFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ListFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ListInfo listInfo = objectInfo as ListInfo;

            if (policy.Rule is UrlRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, listInfo.Url, policy.Value);
            }
            else if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, listInfo.Name, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                bool isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, listInfo.Modified, policy.Value);
                RecordFilterLog("ListInfo", isQualified, listInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, listInfo.Created, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, listInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.ModifiedByEmail, policy.Value);
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, listInfo.CreateByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.CreateByEmail, policy.Value);
                }
            }
            else if (policy.Rule is InheritanceRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, listInfo.InheritPermission, policy.Value);
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is CustomPropertyTextRule)
            {
                if (!listInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == listInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                string columnValue = listInfo.ColumnInfos[policy.Rule.Value1].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is CustomPropertyNumberRule)
            {
                if (!listInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == listInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                double columnValue;
                try
                {
                    columnValue = double.Parse(listInfo.ColumnInfos[policy.Rule.Value1].ToString());
                }
                catch (Exception e)
                {
                    logger.Warn(e.ToString());
                    return false;
                }
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is CustomPropertyDateTimeRule)
            {
                if (!listInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == listInfo.ColumnInfos[policy.Rule.Value1] || !listInfo.ColumnInfos[policy.Rule.Value1].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                DateTime columnValue = DateTime.Parse(listInfo.ColumnInfos[policy.Rule.Value1].ToString());
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is CustomPropertyBooleanRule)
            {
                if (!listInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == listInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                bool columnValue;
                try
                {
                    columnValue = bool.Parse(listInfo.ColumnInfos[policy.Rule.Value1].ToString());
                }
                catch (Exception e)
                {
                    logger.Warn(e.ToString());
                    return false;
                }
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is VersioningRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, listInfo.EnableVersioning, policy.Value);
            }
            else if (policy.Rule is AuditingRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, listInfo.EnableAuditing, policy.Value);
            }
            else if (policy.Rule is AnonymousAccessRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, listInfo.EnableAnonymousAccess, policy.Value);
            }
            else if (policy.Rule is RequireCheckoutRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, listInfo.RequireCheckout, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
