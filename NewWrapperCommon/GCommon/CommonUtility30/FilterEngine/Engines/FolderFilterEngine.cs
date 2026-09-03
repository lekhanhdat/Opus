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

    internal class FolderFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FolderFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            FolderInfo folderInfo = objectInfo as FolderInfo;
            Boolean isQualified = false;

            if (policy.Rule is UrlRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, folderInfo.Url, policy.Value);
                RecordFilterLog(isQualified, folderInfo.Url, policy);
            }
            else if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, folderInfo.Name, policy.Value);
                RecordFilterLog(isQualified, folderInfo.Name, policy);
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, folderInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, folderInfo.Modified.ToString(), policy);
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, folderInfo.Created, policy.Value);
                RecordFilterLog(isQualified, folderInfo.Created.ToString(), policy);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, folderInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.ModifiedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.ModifiedByTitle, policy.Value);
                RecordFilterLog(isQualified, new List<string>(){ 
                    folderInfo.ModifiedByLogonName,
                    folderInfo.ModifiedByTitle,
                    folderInfo.ModifiedByLogonNameWithPrefix
                }, policy);
            }
            else if (policy.Rule is CreatedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, folderInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.CreatedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.CreatedByTitle, policy.Value);
                RecordFilterLog(isQualified, new List<string>(){ 
                    folderInfo.CreatedByLogonName,
                    folderInfo.CreatedByTitle,
                    folderInfo.CreatedByLogonNameWithPrefix
                }, policy);
            }
            else if (policy.Rule is InheritanceRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, folderInfo.InheritPermission, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(folderInfo.InheritPermission), policy);
            }
            else if (policy.Rule is UserAndGroupRule)//此rule不输出Log。
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                isQualified = policy.Result.Value;
            }
            else if (policy.Rule is AuditingRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, folderInfo.EnableAuditing, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(folderInfo.EnableAuditing), policy);
            }
            else if (policy.Rule is ContentTypeNameRule || policy.Rule is ContentTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, folderInfo.ContentType, policy.Value);
                RecordFilterLog(isQualified, folderInfo.ContentType, policy);
            }
            else if (policy.Rule is ContentTypeIdRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, folderInfo.ContentTypeId, policy.Value);
                RecordFilterLog(isQualified, folderInfo.ContentTypeId, policy);
            }
            else if (policy.Rule is ColumnTextRule)
            {
                string columnValue ;
                var valueInCollection = base.GetColumnValue(policy, folderInfo.ColumnInfosOfDisplayName, folderInfo.ColumnInfosOfInternalName, folderInfo.IntrNameToDispName, folderInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                }
                RecordFilterLog(isQualified, columnValue, policy);
            }
            else if (policy.Rule is ColumnNumberRule)
            {
                double columnValue;
                var valueInCollection = base.GetColumnValue(policy, folderInfo.ColumnInfosOfDisplayName, folderInfo.ColumnInfosOfInternalName, folderInfo.IntrNameToDispName, folderInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection,out columnValue,true))
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                }
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                DateTime columnValue ;
                var valueInCollection = base.GetColumnValue(policy, folderInfo.ColumnInfosOfDisplayName, folderInfo.ColumnInfosOfInternalName, folderInfo.IntrNameToDispName, folderInfo.SpecailColumnInfosOfDisplayName, "DateTime");
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    isQualified = false;
                }
                else
                {
                    if (columnValue.Kind != DateTimeKind.Utc)
                    {
                        columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
                    }
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                }
                RecordFilterLog(isQualified, columnValue.ToString(), policy);
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                bool columnValue;
                var valueInCollection = base.GetColumnValue(policy, folderInfo.ColumnInfosOfDisplayName, folderInfo.ColumnInfosOfInternalName, folderInfo.IntrNameToDispName, folderInfo.SpecailColumnInfosOfDisplayName, "Boolean");
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    isQualified = false;
                }
                else
                {
                    isQualified = BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                }
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
            }
            else if (policy.Rule is CustomPropertyBaseRule)
            {
                isQualified = QualifyCustomProperty(policy, folderInfo.ColumnInfosOfDisplayName);
            }
            else if (policy.Rule is TermRule)
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                if (!folderInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                {
                    return false;
                }
                columnValue = folderInfo.TermInfosOfDisplayName[columnName].ToString();
                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue, policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
            return isQualified;
        }

        protected override PolicyLevel Level
        {
            get { return PolicyLevel.Folder; }
        }
    }
}
