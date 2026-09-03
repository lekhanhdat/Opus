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

    internal class ItemVersionFilterEngine : FilterEngineBase
    {
        public ItemVersionFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ItemVersionInfo itemVersionInfo = objectInfo as ItemVersionInfo;
            if (policy.Rule is TitleRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.Title, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, itemVersionInfo.Modified, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByEmail, policy.Value);
                }
            }
            else if (policy.Rule is KeepHistoryVersionRule)
            {
                return VersionConditionChecker.IsQualified(policy.Condition, itemVersionInfo, policy.Value);
            }
            else if (policy.Rule is VersionsRule)
            {
                return VersionConditionChecker.IsQualified(policy.Condition, itemVersionInfo, policy.Value);
            }
            else if (policy.Rule is ListTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ListType, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

    }
}
