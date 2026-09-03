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
    using System.Reflection;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon;
    using System.Reflection;
    #endregion

    internal class MicroFeedItemFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MicroFeedItemFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            MicroFeedItemInfo itemInfo = objectInfo as MicroFeedItemInfo;
            Boolean isQualified = false;
            if (policy.Rule is ParticipationRule)
            {
                isQualified = StringListConditionChecker.IsQualified(policy.Condition, itemInfo.ParticipationLogonName, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.ParticipationLogonNameWithPrefix, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.ParticipationTitle, policy.Value);
                RecordFilterLog(isQualified, isQualified ? policy.Value.Value1 : "Null", policy);
                return isQualified;

            }
            else if (policy.Rule is PostedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.PostedByLogonName, policy.Value)
                    || StringConditionChecker.IsQualified(policy.Condition, itemInfo.PostedByLogonNameWithPrefix, policy.Value)
                    || StringConditionChecker.IsQualified(policy.Condition, itemInfo.PostedByTitle, policy.Value);
                RecordFilterLog(isQualified, new List<string>()
                { 
                    itemInfo.PostedByLogonName,
                    itemInfo.PostedByTitle,
                    itemInfo.PostedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is RepliedByRule)
            {
                isQualified = StringListConditionChecker.IsQualified(policy.Condition, itemInfo.RepliedByLogonName, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.RepliedByLogonNameWithPrefix, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.RepliedByTitle, policy.Value);
                RecordFilterLog(isQualified, isQualified ? policy.Value.Value1 : "Null", policy);
                return isQualified;
            }
            else if (policy.Rule is LikedByRule)
            {
                isQualified = StringListConditionChecker.IsQualified(policy.Condition, itemInfo.LikedByLogonName, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.LikedByLogonNameWithPrefix, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.LikedByTitle, policy.Value);
                RecordFilterLog(isQualified, isQualified ? policy.Value.Value1 : "Null", policy);
                return isQualified;
            }
            else if (policy.Rule is MentionRule)
            {
                isQualified = StringListConditionChecker.IsQualified(policy.Condition, itemInfo.MentionLogonName, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.MentionLogonNameWithPrefix, policy.Value)
                    || StringListConditionChecker.IsQualified(policy.Condition, itemInfo.MentionTitle, policy.Value);
                RecordFilterLog(isQualified, (isQualified ? policy.Value.Value1 : "Null"), policy);
                return isQualified;
            }
            else if (policy.Rule is PostContentRule)
            {
                isQualified = StringListConditionChecker.IsQualified(policy.Condition, itemInfo.PostContents, policy.Value);
                RecordFilterLog(isQualified, (isQualified ? policy.Value.Value1 : "Null"), policy);
                return isQualified;
            }
            else if (policy.Rule is TagRule)
            {
                isQualified = StringListConditionChecker.IsQualified(policy.Condition, itemInfo.Tags, policy.Value);
                RecordFilterLog(isQualified, (isQualified ? policy.Value.Value1 : "Null"), policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }


        protected override PolicyLevel Level
        {
            get { return PolicyLevel.Newsfeed; }
        }
    }
}
