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
using AvePoint.GCommon.Contract.CommonFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Common.FilterEngine
{
    internal class ExchangeMessageFilterEngine : FilterEngineBase
    {
        public ExchangeMessageFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ExchangeMessageInfo itemInfo = objectInfo as ExchangeMessageInfo;
            if (policy.Rule is SubjectRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.Subject, policy.Value);
            }
            if (policy.Rule is AttachmentRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, itemInfo.AttachmentCount, policy.Value);
            }
            if (policy.Rule is SendToRule)
            {
                Boolean isQualified = false;
                if (policy.Condition == PolicyCondition.DoesNotContains|| policy.Condition == PolicyCondition.DoesNotMatch || policy.Condition == PolicyCondition.DoesNotEquals)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendToDisplayName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendToEmailAddress, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendToDisplayWithAddress, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendToDisplayName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendToEmailAddress, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendToDisplayWithAddress, policy.Value);
                }
                return isQualified;
                //return StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendTo, policy.Value);
            }
            if (policy.Rule is SendFromRule)
            {
                Boolean isQualified = false;
                if (policy.Condition == PolicyCondition.DoesNotContains || policy.Condition == PolicyCondition.DoesNotMatch || policy.Condition == PolicyCondition.DoesNotEquals)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFromDisplayName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFromEmailAddress, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFromDisplayWithAddress, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFromDisplayName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFromEmailAddress, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFromDisplayWithAddress, policy.Value);
                }
                return isQualified;
                //return StringConditionChecker.IsQualified(policy.Condition, itemInfo.SendFrom, policy.Value);
            }
            if (policy.Rule is SizeRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, itemInfo.ItemSize, policy.Value);
            }
            if (policy.Rule is SendDateUTCRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, itemInfo.SendDateUTC, policy.Value);
            }
            if (policy.Rule is TermRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.TermValue, policy.Value);
            }
            if (policy.Rule is RetentionLabelRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.RetentionLabel.ToString(), policy.Value);
            }
            if(policy.Rule is SensitivityLabelRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, itemInfo.SensitivityLabel, policy.Value);
            }
            throw new LevelNotSupportedException(policy.Rule.ToString());
        }

    }
}
