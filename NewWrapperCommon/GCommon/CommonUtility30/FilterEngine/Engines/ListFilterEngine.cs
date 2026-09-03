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

    internal class ListFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ListFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ListInfo listInfo = objectInfo as ListInfo;
            Boolean isQualified = false;
            if (policy.Rule is UrlRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.Url, policy.Value);
                RecordFilterLog(isQualified, listInfo.Url, policy);
            }
            else if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.Title, policy.Value);
                RecordFilterLog(isQualified, listInfo.Title, policy);
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, listInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, listInfo.Modified.ToString(), policy);
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, listInfo.Created, policy.Value);
                RecordFilterLog(isQualified, listInfo.Created.ToString(), policy);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.ModifiedByLogonNameWithPrefix, policy.Value);
                RecordFilterLog(isQualified, new List<string>(){ 
                    listInfo.ModifiedByLogonName,
                    listInfo.ModifiedByTitle,
                    listInfo.ModifiedByLogonNameWithPrefix
                }, policy);
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByLogonNameWithPrefix, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, listInfo.CreatedByLogonNameWithPrefix, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    listInfo.CreatedByLogonName,
                    listInfo.CreatedByTitle,
                    listInfo.CreatedByLogonNameWithPrefix
                }, policy);
            }
            else if (policy.Rule is ColumnsRule)
            {
                string columnName = policy.Value.Value1;
                //column name的格式为[xxx]时，表示的是internal name，则把中括号去掉特殊处理。
                if (columnName.StartsWith("[", StringComparison.OrdinalIgnoreCase) && columnName.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                {
                    string realValue1 = columnName.Trim(new char[] { '[', ']' });
                    PolicyValue tempValue = new PolicyValue(realValue1, policy.Value.Value1Unit, policy.Value.Value2, policy.Value.Value2Unit);
                    isQualified = CollectionConditionChecker.IsQualified(policy.Condition, listInfo.InternalColumns, tempValue);
                    RecordFilterLog(isQualified, listInfo.InternalColumns, policy);
                }
                else
                {
                    isQualified = CollectionConditionChecker.IsQualified(policy.Condition, listInfo.DisplayColumns, policy.Value);
                    RecordFilterLog(isQualified, listInfo.DisplayColumns, policy);
                }
            }
            else if (policy.Rule is ContentTypeCollectionRule || policy.Rule is ContentTypeCollectionNameRule)
            {
                isQualified = CollectionConditionChecker.IsQualified(policy.Condition, listInfo.ContentTypes, policy.Value);
                RecordFilterLog(isQualified, listInfo.ContentTypes, policy);
            }
            else if (policy.Rule is ContentTypeCollectionIdRule)
            {
                isQualified = CollectionConditionChecker.IsQualified(policy.Condition, listInfo.ContentTypeIds, policy.Value);
                RecordFilterLog(isQualified, listInfo.ContentTypeIds, policy);
            }
            else if (policy.Rule is TemplateRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.TemplateName, policy.Value);
                RecordFilterLog(isQualified, listInfo.TemplateName, policy);
            }
            else if (policy.Rule is TemplateIdRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, listInfo.Template, policy.Value);
                RecordFilterLog(isQualified, listInfo.Template, policy);
            }
            else if (policy.Rule is InheritanceRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, listInfo.InheritPermission, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(listInfo.InheritPermission), policy);
            }
            else if (policy.Rule is UserAndGroupRule)//这个Rule不做Log输出。
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                isQualified = policy.Result.Value;
            }
            else if (policy.Rule is CustomPropertyBaseRule)
            {
                isQualified = QualifyCustomProperty(policy, listInfo.Properties);
            }
            else if (policy.Rule is VersioningRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, listInfo.EnableVersioning, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(listInfo.EnableVersioning), policy);
            }
            else if (policy.Rule is AuditingRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, listInfo.EnableAuditing, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(listInfo.EnableAuditing), policy);
            }
            else if (policy.Rule is AnonymousAccessRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, listInfo.EnableAnonymousAccess, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(listInfo.EnableAnonymousAccess), policy);
            }
            else if (policy.Rule is AccessTimeRule)
            {
                if (listInfo.AccessTime == DateTime.MinValue)
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, listInfo.AccessTime, policy.Value);
                    RecordFilterLog(isQualified, listInfo.AccessTime.ToString(), policy);
                }
            }
            else if (policy.Rule is ItemCountRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, listInfo.ItemCount, policy.Value);
                RecordFilterLog(isQualified, listInfo.AccessTime.ToString(), policy);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
            return isQualified;
        }

        protected override PolicyLevel Level
        {
            get { return PolicyLevel.List; }
        }
    }
}
