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

    internal class AttachmentFilterEngine : FilterEngineBase
    {
        public AttachmentFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            AttachmentInfo attachmentInfo = objectInfo as AttachmentInfo;

            if (policy.Rule is NameAndExtentionRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.Name, policy.Value);
            }
            else if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.Name, policy.Value);
            }
            else if (policy.Rule is SizeRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, attachmentInfo.Size, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.Created, policy.Value);
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                if (attachmentInfo.IsStub)
                {
                    return DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.StubLastAccessTime, policy.Value);
                }
                else
                {
                    return false;
                }
            }
            else if (policy.Rule is StubLastActiveTimeRule)
            {
                if (attachmentInfo.IsStub)
                {
                    return DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.LastAccessCompatibleModifiedTime, policy.Value);
                }
                else
                {
                    return false;
                }
            }
            else if (policy.Rule is ModifiedByRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ModifiedByEmail, policy.Value);
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreateByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreateByEmail, policy.Value);
                }
            }
            else if (policy.Rule is ColumnTextRule)
            {
                if (!attachmentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                string columnValue = attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is ColumnNumberRule)
            {
                if (!attachmentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                double columnValue = double.Parse(attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                if (!attachmentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                DateTime columnValue = DateTime.Parse(attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                if (!attachmentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                bool columnValue = bool.Parse(attachmentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ListTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ListType, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
