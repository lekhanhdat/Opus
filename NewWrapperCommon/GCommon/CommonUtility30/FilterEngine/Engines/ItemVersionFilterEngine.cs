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
        public ItemVersionFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ItemVersionInfo itemVersionInfo = objectInfo as ItemVersionInfo;
            Boolean isQualified = false;
            if (policy.Rule is TitleRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.Title, policy.Value);
                RecordFilterLog(isQualified, itemVersionInfo.Title, policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, itemVersionInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, itemVersionInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByLogonNameWithPrefix, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ModifiedByLogonNameWithPrefix, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    itemVersionInfo.ModifiedByLogonName,
                    itemVersionInfo.ModifiedByTitle,
                    itemVersionInfo.ModifiedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is KeepHistoryVersionRule)
            {
                isQualified = VersionConditionChecker.IsQualified(policy.Condition, itemVersionInfo, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(itemVersionInfo.VersionSequenceNo), policy);
                return isQualified;
            }
            else if (policy.Rule is VersionsRule)
            {
                isQualified = VersionConditionChecker.IsQualified(policy.Condition, itemVersionInfo, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(itemVersionInfo.VersionSequenceNo), policy);
                return isQualified;
            }
            else if (policy.Rule is ListTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemVersionInfo.ListType, policy.Value);
                RecordFilterLog(isQualified, itemVersionInfo.ListType, policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }


        protected override PolicyLevel Level
        {
            get { return PolicyLevel.ItemVersion; }
        }
    }
}
