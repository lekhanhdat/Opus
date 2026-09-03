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

    internal class SiteCollectionFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SiteCollectionFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            SiteCollectionInfo siteCollectionInfo = objectInfo as SiteCollectionInfo;

            if (policy.Rule is UrlRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Url, policy.Value);
            }
            else if (policy.Rule is TitleRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Title, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                bool isQualified=DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Modified, policy.Value);
                RecordFilterLog("SiteCollectionInfo", isQualified, siteCollectionInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Created, policy.Value);
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.LastAccessTime, policy.Value);
            }
            else if (policy.Rule is StubLastActiveTimeRule)
            {
                //Compatible logic takes the last access time
                return DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.LastAccessCompatibleModifiedTime, policy.Value);
            }
            else if (policy.Rule is OwnerRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonName, policy.Value)
                    || StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerTitle, policy.Value)
                    || StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonNameWithPrefix, policy.Value);
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonName, policy.Value)
                        && StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerTitle, policy.Value)
                        && StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonNameWithPrefix, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonName, policy.Value)
                        || StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerTitle, policy.Value)
                        || StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonNameWithPrefix, policy.Value)
                        || StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Owner, policy.Value);
                }
            }
            else if (policy.Rule is TemplateRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.TemplateName, policy.Value);
            }
            else if (policy.Rule is TemplateIdRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Template, policy.Value);
            }
            else if (policy.Rule is CustomPropertyTextRule)
            {
                if (!siteCollectionInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteCollectionInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                string columnValue = siteCollectionInfo.ColumnInfos[policy.Rule.Value1].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is SizeRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Size, policy.Value);
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is CustomPropertyNumberRule)
            {
                if (!siteCollectionInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteCollectionInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                double columnValue;
                try
                {
                    columnValue = double.Parse(siteCollectionInfo.ColumnInfos[policy.Rule.Value1].ToString());
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
                if (!siteCollectionInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteCollectionInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                DateTime columnValue;
                if (!DateTime.TryParse(siteCollectionInfo.ColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
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
                if (!siteCollectionInfo.ColumnInfos.ContainsKey(policy.Rule.Value1) || null == siteCollectionInfo.ColumnInfos[policy.Rule.Value1])
                {
                    return false;
                }
                bool columnValue = false;
                if (siteCollectionInfo.ColumnInfos[policy.Rule.Value1] is string)
                {
                    string value = siteCollectionInfo.ColumnInfos[policy.Rule.Value1] as string;
                    if (string.Equals("yes", value, StringComparison.OrdinalIgnoreCase)
                        || string.Equals("true",value, StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = true;
                    }
                    else if (string.Equals("no", value, StringComparison.OrdinalIgnoreCase)
                        || string.Equals("false", value, StringComparison.OrdinalIgnoreCase))
                    {
                        columnValue = false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (siteCollectionInfo.ColumnInfos[policy.Rule.Value1] is bool)
                {
                    columnValue = (bool)siteCollectionInfo.ColumnInfos[policy.Rule.Value1];
                }
                else
                {
                    return false;
                }
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is AuditingRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.EnableAuditing, policy.Value);
            }
            else if (policy.Rule is AnonymousAccessRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.EnableAnonymousAccess, policy.Value);
            }
            else if (policy.Rule is LockStatusRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.LockStatus.ToString(), policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

    }
}
