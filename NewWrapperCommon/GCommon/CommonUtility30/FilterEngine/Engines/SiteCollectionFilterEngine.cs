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

        public SiteCollectionFilterEngine(FilterOption option)
            : base(option)
        {


        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)//可以 先is 后 as成SiteCollectionInfo
        {
            SiteCollectionInfo siteCollectionInfo = objectInfo as SiteCollectionInfo;
            Boolean isQualified = false;
            if (policy.Rule is UrlRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Url, policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.Url, policy);
            }
            else if (policy.Rule is TitleRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Title, policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.Title, policy);
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.Modified.ToString(), policy);
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Created, policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.Created.ToString(), policy);
            }
            else if (policy.Rule is CreatedByRule || policy.Rule is OwnerRule)
            {
                if (policy.Rule is OwnerRule && policy.Value.Value1.Equals("$deactivatedaccount", StringComparison.OrdinalIgnoreCase))
                {
                    if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                    isQualified = policy.Result.Value;
                }
                else
                {
                    //OwnerRule和CreatedByRule只有Equals和Contains两种过滤条件，所以此处用||逻辑运算符
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonName, policy.Value) ||
                        StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.OwnerLogonNameWithPrefix, policy.Value);
                    RecordFilterLog(isQualified, siteCollectionInfo.OwnerLogonNameWithPrefix, policy);
                }
            }
            else if (policy.Rule is TemplateRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.TemplateName, policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.TemplateName, policy);
            }
            else if (policy.Rule is TemplateIdRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Template, policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.Template, policy);
            }
            else if (policy.Rule is CustomPropertyBaseRule)
            {
                isQualified = QualifyCustomProperty(policy, siteCollectionInfo.Properties);
            }
            else if (policy.Rule is SizeRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.Size, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(siteCollectionInfo.Size), policy);
            }
            else if (policy.Rule is UserAndGroupRule)//这个rule不做Log输出。
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                isQualified = policy.Result.Value;
            }
            else if (policy.Rule is AuditingRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.EnableAuditing, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(siteCollectionInfo.EnableAuditing), policy);
            }
            else if (policy.Rule is AnonymousAccessRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.EnableAnonymousAccess, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(siteCollectionInfo.EnableAnonymousAccess), policy);
            }
            else if (policy.Rule is LockStatusRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.LockStatus.ToString(), policy.Value);
                RecordFilterLog(isQualified, siteCollectionInfo.LockStatus.ToString(), policy);
            }
            else if (policy.Rule is AccessTimeRule)
            {
                if (siteCollectionInfo.AccessTime == DateTime.MinValue)
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, siteCollectionInfo.AccessTime, policy.Value);
                    RecordFilterLog(isQualified, siteCollectionInfo.AccessTime.ToString(), policy);
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
            get { return PolicyLevel.SiteCollection; }
        }
    }
}
