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

        public WebApplicationFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            WebAppInfo appInfo = objectInfo as WebAppInfo;
            Boolean isQualified = false;
            if ((policy.Rule is UrlRule))
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, appInfo.Url, policy.Value);
                RecordFilterLog(isQualified, appInfo.Url, policy);
                return isQualified;
            }
            else if (policy.Rule is CustomPropertyTextRule)
            {
                //if (!appInfo.Properties.ContainsKey(policy.Rule.Value1) || null == appInfo.Properties[policy.Rule.Value1])
                //{
                //    return false;
                //}
                //string columnValue = appInfo.Properties[policy.Rule.Value1].ToString();
                string columnValue = "";
                if (appInfo.Properties.ContainsKey(policy.Rule.Value1) && null != appInfo.Properties[policy.Rule.Value1])
                {
                    columnValue = appInfo.Properties[policy.Rule.Value1].ToString();
                }
                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue, policy);
                return isQualified;

            }
            else if (policy.Rule is CustomPropertyNumberRule)
            {
                if (!appInfo.Properties.ContainsKey(policy.Rule.Value1) || null == appInfo.Properties[policy.Rule.Value1])
                {
                    return false;
                }
                double columnValue;
                try
                {
                    columnValue = double.Parse(appInfo.Properties[policy.Rule.Value1].ToString());
                }
                catch (Exception e)
                {
                    logger.Warn(e.ToString());
                    return false;
                }
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
                return isQualified;
            }
            else if (policy.Rule is CustomPropertyDateTimeRule)
            {
                //if (!siteInfo.ColumnInfosOfDisplayName.ContainsKey(policy.Rule.Value1) || null == siteInfo.ColumnInfosOfDisplayName[policy.Rule.Value1] || !siteInfo.ColumnInfosOfDisplayName[policy.Rule.Value1].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                if (!appInfo.Properties.ContainsKey(policy.Rule.Value1) || null == appInfo.Properties[policy.Rule.Value1])
                {
                    return false;
                }
                DateTime columnValue;
                if (!DateTime.TryParse(appInfo.Properties[policy.Rule.Value1].ToString(), out columnValue))
                {
                    return false;
                }
                if (columnValue.Kind != DateTimeKind.Utc)
                {
                    columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
                }
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CustomPropertyBooleanRule)
            {
                if (!appInfo.Properties.ContainsKey(policy.Rule.Value1) || null == appInfo.Properties[policy.Rule.Value1])
                {
                    return false;
                }
                bool columnValue = false;
                if (appInfo.Properties[policy.Rule.Value1] is string)
                {
                    columnValue |= string.Equals("yes", appInfo.Properties[policy.Rule.Value1] as string, StringComparison.OrdinalIgnoreCase);
                    columnValue |= string.Equals("true", appInfo.Properties[policy.Rule.Value1] as string, StringComparison.OrdinalIgnoreCase);
                }
                else if (appInfo.Properties[policy.Rule.Value1] is bool)
                {
                    columnValue = (bool)appInfo.Properties[policy.Rule.Value1];
                }
                else
                {
                    return false;
                }
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

        protected override PolicyLevel Level
        {
            get { return PolicyLevel.WebApplication; }
        }
    }
}
