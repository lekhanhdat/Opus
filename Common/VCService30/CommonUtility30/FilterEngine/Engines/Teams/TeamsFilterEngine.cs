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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Common.FilterEngine.Conditions;
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.CommonFilter.Rules;

namespace AvePoint.Common.FilterEngine.Engines.Teams
{
    internal class TeamsFilterEngine : FilterEngineBase
    {
        public TeamsFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterExpressionLists, FilterEngine engine) 
            : base(policyLists, filterExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            var teamsInfo = objectInfo as TeamsInfo;
            bool isQualified = false;
            switch (policy.Rule)
            {
                case TeamsClassificationRule:
                    return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.Classification, policy.Value);
                case CreatedByRule:
                    {
                        if (policy.Condition == PolicyCondition.DoesNotContains)
                        {
                            return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.OwnerLogonName, policy.Value)
                                && StringConditionChecker.IsQualified(policy.Condition, teamsInfo.OwnerTitle, policy.Value)
                                && StringConditionChecker.IsQualified(policy.Condition, teamsInfo.OwnerLogonNameWithPrefix, policy.Value);
                        }
                        else
                        {
                            return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.OwnerLogonName, policy.Value)
                                || StringConditionChecker.IsQualified(policy.Condition, teamsInfo.OwnerTitle, policy.Value)
                                || StringConditionChecker.IsQualified(policy.Condition, teamsInfo.OwnerLogonNameWithPrefix, policy.Value)
                                || StringConditionChecker.IsQualified(policy.Condition, teamsInfo.Owner, policy.Value);
                        }
                    }
                case CustomPropertyTextRule:
                    {
                        if (!teamsInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == teamsInfo.ColumnInfos[policy.Rule.Value1])
                        {
                            return false;
                        }
                        string columnValue = teamsInfo.ColumnInfos[policy.Rule.Value1].ToString();
                        return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                    }
                case CustomPropertyNumberRule:
                    {
                        if (!teamsInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == teamsInfo.ColumnInfos[policy.Rule.Value1])
                        {
                            return false;
                        }
                        double columnValue = double.Parse(teamsInfo.ColumnInfos[policy.Rule.Value1].ToString());
                        return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                    }
                case CustomPropertyDateTimeRule:
                    {
                        if (!teamsInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == teamsInfo.ColumnInfos[policy.Rule.Value1] || !teamsInfo.ColumnInfos[policy.Rule.Value1].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                        DateTime columnValue = DateTime.Parse(teamsInfo.ColumnInfos[policy.Rule.Value1].ToString());
                        return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                    }
                case CustomPropertyBooleanRule:
                    {
                        if (!teamsInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == teamsInfo.ColumnInfos[policy.Rule.Value1])
                        {
                            return false;
                        }
                        bool columnValue = false;
                        if (teamsInfo.ColumnInfos[policy.Rule.Value1] is string)
                        {
                            string value = teamsInfo.ColumnInfos[policy.Rule.Value1] as string;
                            if (string.Equals("yes", value, StringComparison.OrdinalIgnoreCase) || string.Equals("true", value, StringComparison.OrdinalIgnoreCase))
                            {
                                columnValue = true;
                            }
                            else if (string.Equals("no", value, StringComparison.OrdinalIgnoreCase) || string.Equals("false", value, StringComparison.OrdinalIgnoreCase))
                            {
                                columnValue = false;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else if (teamsInfo.ColumnInfos[policy.Rule.Value1] is bool)
                        {
                            columnValue = (bool)teamsInfo.ColumnInfos[policy.Rule.Value1];
                        }
                        else
                        {
                            return false;
                        }
                        return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                    }
                case DisplayNameRule:
                    return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.DisplayName, policy.Value);
                case MemberRule:
                    {
                        if (teamsInfo.Members == null || teamsInfo.Members.Count == 0) return policy.Condition == PolicyCondition.IsEmpty;
                        return MemberInfoesValidate(teamsInfo.Members, policy.Condition, policy.Value);
                    }
                case OwnerRule:
                    {
                        return MemberInfoesValidate(teamsInfo.Owners, policy.Condition, policy.Value);
                    }
                case UrlRule:
                    return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.Url, policy.Value);
                case TitleRule:
                    return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.Title, policy.Value);
                case ModifiedRule:
                    return DateTimeConditionChecker.IsQualified(policy.Condition, teamsInfo.Modified, policy.Value);
                case CreatedRule:
                    return DateTimeConditionChecker.IsQualified(policy.Condition, teamsInfo.Created, policy.Value);
                case SizeRule:
                    return NumberConditionChecker.IsQualified(policy.Condition, teamsInfo.Size, policy.Value);
                case SensitivityLabelRule:
                    return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.SensitiveLabel, policy.Value);
                case SensitivityLabelFullNameRule:
                    return StringConditionChecker.IsQualified(policy.Condition, teamsInfo.SensitiveLabelFullName, policy.Value);
                case TeamStatusRule:
                    return PolicyValueUnitConditionChecker.IsQualified(policy.Condition, teamsInfo.TeamsStatus, policy.Value);
                case PrivacyRule:
                    return PolicyValueUnitConditionChecker.IsQualified(policy.Condition, teamsInfo.Privacy, policy.Value);
                case TeamsTypeRule:
                    return PolicyValueUnitConditionChecker.IsQualified(policy.Condition, teamsInfo.TeamsType, policy.Value);
                case StubLastActiveTimeRule:
                case StubLastAccessTimeRule:
                    return DateTimeConditionChecker.IsQualified(policy.Condition, teamsInfo.LastAccessCompatibleModifiedTime, policy.Value);
                default:
                    throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

        private static bool MemberInfoesValidate(List<MemberInfo> infos, PolicyCondition policyCondition, PolicyValue policyValue)
        {
            bool hasDoesNotCondition = policyCondition == PolicyCondition.DoesNotContains
                                    || policyCondition == PolicyCondition.DoesNotMatch
                                    || policyCondition == PolicyCondition.DoesNotEquals;
            bool hasDoesNotConditionAndNotQualified = false;
            foreach (var member in infos)
            {
                if (hasDoesNotCondition)
                {
                    if (!StringConditionChecker.IsQualified(policyCondition, member.Name, policyValue)
                        || !StringConditionChecker.IsQualified(policyCondition, member.EmailAddress, policyValue))
                    {
                        hasDoesNotConditionAndNotQualified = true;
                    }
                }
                else if (StringConditionChecker.IsQualified(policyCondition, member.Name, policyValue)
                        || StringConditionChecker.IsQualified(policyCondition, member.EmailAddress, policyValue))
                {
                    return true;
                }
            }

            if (hasDoesNotCondition && !hasDoesNotConditionAndNotQualified)
            {
                return true;
            }
            return false;
        }
    }
}
