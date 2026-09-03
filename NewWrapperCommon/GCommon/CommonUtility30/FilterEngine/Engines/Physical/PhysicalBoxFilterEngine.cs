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

    internal class PhysicalBoxFilterEngine : FilterEngineBase
    {
        public PhysicalBoxFilterEngine(FilterOption option) : base(option)
        {
        }

        protected override PolicyLevel Level { get { return PolicyLevel.PhysicalBox; } }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            PhysicalBoxInfo physicalBoxInfo = objectInfo as PhysicalBoxInfo;
            if (policy.Rule is NameAndExtentionRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.Title, policy.Value);
            }
            else if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.Title, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.Created, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.ModifiedByLogonName, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.ModifiedByLogonName, policy.Value);
                }
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.CreatedByLogonName, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.CreatedByLogonName, policy.Value);
                }
            }
            else if (policy.Rule is SizeRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, physicalBoxInfo.Size, policy.Value);
            }
            else if (policy.Rule is ColumnTextRule)
            {
                if (!physicalBoxInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                string columnValue = physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is ColumnNumberRule)
            {
                if (!physicalBoxInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                double columnValue = double.Parse(physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                if (!physicalBoxInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                DateTime columnValue = DateTime.Parse(physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                if (!physicalBoxInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                bool columnValue = bool.Parse(physicalBoxInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

    }
}
