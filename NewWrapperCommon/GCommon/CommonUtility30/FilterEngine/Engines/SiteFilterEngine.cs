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

    internal class SiteFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SiteFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            SiteInfo siteInfo = objectInfo as SiteInfo;
            var isQualified = false;
            if (policy.Rule is UrlRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteInfo.Url, policy.Value);
                RecordFilterLog(isQualified, siteInfo.Url, policy);
            }
            else if (policy.Rule is TitleRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteInfo.Title, policy.Value);
                RecordFilterLog(isQualified, siteInfo.Title, policy);
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, siteInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, siteInfo.Modified.ToString(), policy);
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, siteInfo.Created, policy.Value);
                RecordFilterLog(isQualified, siteInfo.Created.ToString(), policy);
            }

            else if (policy.Rule is CreatedByRule || policy.Rule is OwnerRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonNameWithPrefix, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, siteInfo.CreatedByLogonNameWithPrefix, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>()
                { 
                    siteInfo.CreatedByLogonName,
                    siteInfo.CreatedByTitle,
                    siteInfo.CreatedByLogonNameWithPrefix
                }, policy);
            }
            else if (policy.Rule is TemplateRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteInfo.TemplateName, policy.Value);
                RecordFilterLog(isQualified, siteInfo.TemplateName, policy);
            }
            else if (policy.Rule is TemplateIdRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteInfo.Template, policy.Value);
                RecordFilterLog(isQualified, siteInfo.Template, policy);
            }
            else if (policy.Rule is CustomPropertyBaseRule)
            {
                //Log输出在内部方法中实现
                isQualified = QualifyCustomProperty(policy, siteInfo.Properties);
            }
            else if (policy.Rule is InheritanceRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, siteInfo.InheritPermission, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(siteInfo.InheritPermission), policy);
            }
            else if (policy.Rule is UserAndGroupRule)//本条rule不输出Log。
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                isQualified = policy.Result.Value;
            }
            else if (policy.Rule is AuditingRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, siteInfo.EnableAuditing, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(siteInfo.EnableAuditing), policy);
            }
            else if (policy.Rule is AnonymousAccessRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, siteInfo.EnableAnonymousAccess, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(siteInfo.EnableAnonymousAccess), policy);
            }
            else if (policy.Rule is AccessTimeRule)
            {
                if (siteInfo.AccessTime == DateTime.MinValue)
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, siteInfo.AccessTime, policy.Value);
                    RecordFilterLog(isQualified, siteInfo.AccessTime.ToString(), policy);
                }
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
            return isQualified;
        }


        protected override PolicyLevel Level
        {
            get { return PolicyLevel.Site; }
        }
    }
}
