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

    internal class WebApplicationFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public WebApplicationFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            WebAppInfo appInfo = objectInfo as WebAppInfo;
            if ((policy.Rule is UrlRule))
            {
                return StringConditionChecker.IsQualified(policy.Condition, appInfo.Url, policy.Value);
            }
            else if (policy.Rule is CustomPropertyTextRule)
            {
                if (!appInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == appInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                string columnValue = appInfo.ColumnInfos[policy.Rule.Value1].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is CustomPropertyNumberRule)
            {
                if (!appInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == appInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                double columnValue;
                try
                {
                    columnValue = double.Parse(appInfo.ColumnInfos[policy.Rule.Value1].ToString());
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
                //if (!siteInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfos[policy.Rule.Value1] || !siteInfo.ColumnInfos[policy.Rule.Value1].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                if (!appInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == appInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                DateTime columnValue;
                if (!DateTime.TryParse(appInfo.ColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
                {
                    return false;
                }
                if (columnValue.Kind != DateTimeKind.Utc)
                {
                    columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
                }
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is CustomPropertyBooleanRule)
            {
                if (!appInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == appInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                bool columnValue = false;
                if (appInfo.ColumnInfos[policy.Rule.Value1] is string)
                {
                    columnValue |= string.Equals("yes", appInfo.ColumnInfos[policy.Rule.Value1] as string, StringComparison.OrdinalIgnoreCase);
                    columnValue |= string.Equals("true", appInfo.ColumnInfos[policy.Rule.Value1] as string, StringComparison.OrdinalIgnoreCase);
                }
                else if (appInfo.ColumnInfos[policy.Rule.Value1] is bool)
                {
                    columnValue = (bool)appInfo.ColumnInfos[policy.Rule.Value1];
                }
                else
                {
                    return false;
                }
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
