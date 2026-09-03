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
    using AvePoint.GCommon.Contract.CommonFilter.Rules;
    #endregion

    internal class FolderFilterEngine : FilterEngineBase
    {
        public FolderFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            FolderInfo folderInfo = objectInfo as FolderInfo;
            
            if (policy.Rule is UrlRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, folderInfo.Url, policy.Value);
            }
            else if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, folderInfo.Name, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, folderInfo.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, folderInfo.Created, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, folderInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.ModifiedByEmail, policy.Value);
            }
            else if (policy.Rule is CreatedByRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, folderInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, folderInfo.CreateByEmail, policy.Value);
            }
            else if (policy.Rule is InheritanceRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, folderInfo.InheritPermission, policy.Value);
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is AuditingRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, folderInfo.EnableAuditing, policy.Value);
            }
            else if (policy.Rule is ColumnTextRule)
            {
                if (!folderInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                string columnValue = folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is ColumnNumberRule)
            {
                if (!folderInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                double columnValue;
                if (!double.TryParse(folderInfo.ColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
                {
                    return false;
                }
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                if (!folderInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                DateTime columnValue;
                if (!DateTime.TryParse(folderInfo.ColumnInfos[policy.Rule.Value1].ToString(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out columnValue))
                {
                    return false;
                }
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                if (!folderInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !folderInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                bool columnValue;
                if (!bool.TryParse(folderInfo.ColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
                {
                    return false;
                }
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is TermRule)//add for RevIM folder rule
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                if (!folderInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                {
                    return false;
                }
                columnValue = folderInfo.TermInfosOfDisplayName[columnName].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ContentTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, folderInfo.ContentType, policy.Value);
            }
            else if(policy.Rule is OrphanedFolderRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, folderInfo.IsOrphanedFolder, policy.Value);
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
