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
    #endregion

    internal class ItemFilterEngine : FilterEngineBase
    {
        public ItemFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ItemInfo itemInfo = objectInfo as ItemInfo;

            if (policy.Rule is UrlRule)
            {
                switch (policy.Condition)
                {
                    case PolicyCondition.Match:
                    case PolicyCondition.Exactly:
                    case PolicyCondition.Equals:
                    case PolicyCondition.Contains:
                        return StringConditionChecker.IsQualified(policy.Condition, itemInfo.Url, policy.Value) ||
                            StringConditionChecker.IsQualified(policy.Condition, itemInfo.DisplayFormUrl, policy.Value);
                    case PolicyCondition.DoesNotMatch:
                    case PolicyCondition.IsExactlyNot:
                    case PolicyCondition.DoesNotContains:
                        return StringConditionChecker.IsQualified(policy.Condition, itemInfo.Url, policy.Value) &&
                            StringConditionChecker.IsQualified(policy.Condition, itemInfo.DisplayFormUrl, policy.Value);
                    default:
                        throw new ConditionNotSupportedException(policy.Condition.ToString());
                }
            }
            else if (policy.Rule is TitleRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.Title, policy.Value);
            }
            else if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.Name, policy.Value);
            }
            else if (policy.Rule is NameAndExtentionRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.Name, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, itemInfo.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, itemInfo.Created, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByEmail, policy.Value);
                }
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreateByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreateByEmail, policy.Value);
                }
            }
            else if (policy.Rule is ContentTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.ContentType, policy.Value);
            }
            else if (policy.Rule is ColumnTextRule)
            {
                if (!itemInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                string columnValue = itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnNumberRule)
            {
                if (!itemInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                double columnValue = double.Parse(itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                if (!itemInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                DateTime columnValue = DateTime.Parse(itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                if (!itemInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                bool columnValue = bool.Parse(itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is MetadataNumberColumnRule)
            {
                if (!itemInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                //Client API Managed Metadata Column Value: 2;#ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                //Wrapper Discover Managed Metadata Column Value:ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                string tempValue = itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                double columnValue;
                if (tempValue.Split(new char[] { '|' }) != null && tempValue.Split(new char[] { '|' }).Length > 0)
                {
                    if (!double.TryParse(tempValue.Split(new char[] { '|' })[0], out columnValue))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is MetadataTextColumnRule)
            {
                if (!itemInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                //Client API Managed Metadata Column Value: 2;#ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                //Wrapper Discover Managed Metadata Column Value:ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                string tempValue = itemInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                string columnValue;
                if (tempValue.Split(new char[] { '|' }) != null && tempValue.Split(new char[] { '|' }).Length > 0)
                {
                    columnValue = tempValue.Split(new char[] { '|' })[0].ToString();
                }
                else
                {
                    return false;
                }
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is VersionsRule)
            {
                return VersionConditionChecker.IsQualified(policy.Condition, itemInfo, policy.Value);
            }
            else if (policy.Rule is InheritanceRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, itemInfo.InheritPermission, policy.Value);
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is WorkflowRule)
            {
                if (!itemInfo.WorkflowStatus.ContainsKey(policy.Rule.Value1.ToLower()) || null == itemInfo.WorkflowStatus[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                string wfStatus = itemInfo.WorkflowStatus[policy.Rule.Value1.ToLower()].ToString();
                if (!string.IsNullOrEmpty(policy.Value.Value2))//Wrokflow Customized Status
                {
                    policy.Value.Value1 = policy.Value.Value2;
                }
                return StringConditionChecker.IsQualified(policy.Condition, wfStatus, policy.Value);

            }
            else if (policy.Rule is ListTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.ListType, policy.Value);
            }
            else if (policy.Rule is TermRule) //add for RevIM term path
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                if (!itemInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                {
                    return false;
                }
                columnValue = itemInfo.TermInfosOfDisplayName[columnName].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

    }
}
