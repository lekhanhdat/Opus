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
using AvePoint.Common.FilterEngine.ObjectInfos.Connector;
using AvePoint.GCommon.Contract.CommonFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Common.FilterEngine.Engines.Connector
{
    internal class CustomizeConnectorFilterEngine : FilterEngineBase
    {

        public CustomizeConnectorFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        { }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            var info = objectInfo as CustomizeConnectorItemInfo;
            if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, info.Name, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, info.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, info.Created, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule || policy.Rule is CreatedByRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, info.ModifiedByTitle, policy.Value);
            }
            else if(policy.Rule is ColumnTextRule)
            {
                if (!info.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == info.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                string columnValue = info.ColumnInfos[policy.Rule.Value1].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnNumberRule)
            {
                if (!info.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == info.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                double columnValue = double.Parse(info.ColumnInfos[policy.Rule.Value1].ToString());
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                if (!info.ColumnInfos.ContainsKey(policy.Rule.Value1) || string.IsNullOrWhiteSpace(info.ColumnInfos[policy.Rule.Value1]?.ToString()))
                {
                    return false;
                }
                DateTime columnValue = DateTime.Parse(info.ColumnInfos[policy.Rule.Value1].ToString());
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
