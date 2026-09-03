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
    using AvePoint.GCommon;
    #region using directives
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.RA.Common.Global;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Text;
    #endregion

    internal class DocumentFilterEngine : FilterEngineBase
    {
        private static AveLogger aveLogger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string EMPTYSTRING = "empty";
        public DocumentFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            DocumentInfo documentInfo = objectInfo as DocumentInfo;

            if (policy.Rule is UrlRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.Url, policy.Value);
            }
            else if (policy.Rule is NameAndExtentionRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.Name, policy.Value);
            }
            else if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.Name, policy.Value);
            }
            else if (policy.Rule is ModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.Created, policy.Value);
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.StubLastAccessTime, policy.Value);
                //if (documentInfo.IsStub)
                //{
                //    return DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.StubLastAccessTime, policy.Value);
                //}
                //else
                //{
                //    return false;
                //}
            }
            else if (policy.Rule is StubLastActiveTimeRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.LastAccessCompatibleModifiedTime, policy.Value);
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByEmail, policy.Value);
                }
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreateByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreateByEmail, policy.Value);
                }
            }
            else if (policy.Rule is ContentTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.ContentType, policy.Value);
            }
            else if (policy.Rule is SizeRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, documentInfo.Size, policy.Value);
            }
            else if (policy.Rule is TermRule)//add for RevIM term path
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                if (!documentInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                {
                    return false;
                }
                columnValue = documentInfo.TermInfosOfDisplayName[columnName].ToString();
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnTextRule)
            {
                //if (!documentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                //{
                //    return false;
                //}
                //string columnValue = documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                string columnValue;
                var valueCollection = base.GetColumnValue(policy, documentInfo.ColumnInfos, documentInfo.ColumnInfosOfInternalName, null, null);
                if (!TryGetValue(valueCollection, out columnValue))
                {
                    if (documentInfo.ListColumnExistInfos != null && documentInfo.ListColumnExistInfos.ContainsKey(policy.Rule.Value1) && documentInfo.ListColumnExistInfos[policy.Rule.Value1] == true)
                    {
                        if (policy.Condition == PolicyCondition.DoesNotContains || policy.Condition == PolicyCondition.DoesNotEquals || policy.Condition == PolicyCondition.DoesNotMatch || policy.Condition == PolicyCondition.IsExactlyNot)
                        {
                            return true;//RECO-23011 when the column is null it need fit rule
                        }
                    }
                    return false;
                }
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);

            }
            else if (policy.Rule is ColumnNumberRule)
            {
                if (!documentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                double columnValue = double.Parse(documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                //if (!documentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
                //{
                //    return false;
                //}
                //DateTime columnValue = DateTime.Parse(documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                DateTime columnValue;
                var valueCollection = base.GetColumnValue(policy, documentInfo.ColumnInfos, documentInfo.ColumnInfosOfInternalName, null, null);
                if (!TryGetValue(valueCollection, out columnValue))
                {
                    return false;
                }
                return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                if (!documentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()] || !documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].GetType().Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(EMPTYSTRING, policy.Value.Value1.ToLower(), StringComparison.OrdinalIgnoreCase))
                    {
                        //Only list contains this column and column value is null will return true.
                        if (documentInfo.ListColumnExistInfos != null && documentInfo.ListColumnExistInfos.ContainsKey(policy.Rule.Value1) && documentInfo.ListColumnExistInfos[policy.Rule.Value1] == true)
                        {
                            return true;
                        }
                        //List does not contains this column will return false.
                        else
                        {
                            return false;
                        }
                    }
                    return false;
                }
                bool columnValue = bool.Parse(documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString());
                return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is MetadataNumberColumnRule)
            {
                if (!documentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                //Client API Managed Metadata Column Value: 2;#ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                //Wrapper Discover Managed Metadata Column Value:ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                string tempValue = documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                double columnValue;
                if (tempValue.Split(new char[] { '|' }) != null && tempValue.Split(new char[] { '|' }).Length > 0)
                {
                    if (!double.TryParse(tempValue.Split(new char[] { '|' })[0], out columnValue))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is MetadataTextColumnRule)
            {
                if (!documentInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                //Client API Managed Metadata Column Value: 2;#ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                //Wrapper Discover Managed Metadata Column Value:ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                string tempValue = documentInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
                string columnValue;
                if (tempValue.Split(new char[] { '|' }) != null && tempValue.Split(new char[] { '|' }).Length > 0)
                {
                    columnValue = tempValue.Split(new char[] { '|' })[0].ToString();
                }
                else
                {
                    return false;
                }
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is VersionsRule)
            {
                return VersionConditionChecker.IsQualified(policy.Condition, documentInfo, policy.Value);
            }
            else if (policy.Rule is InheritanceRule)
            {
                return BooleanConditionChecker.IsQualified(policy.Condition, documentInfo.InheritPermission, policy.Value);
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is WorkflowRule)
            {
                if (!documentInfo.WorkflowStatus.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentInfo.WorkflowStatus[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                string wfStatus = documentInfo.WorkflowStatus[policy.Rule.Value1.ToLower()].ToString();
                if (!string.IsNullOrEmpty(policy.Value.Value2))//Wrokflow Customized Status
                {
                    policy.Value.Value1 = policy.Value.Value2;
                }
                return StringConditionChecker.IsQualified(policy.Condition, wfStatus, policy.Value);

            }
            else if (policy.Rule is ListTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.ListType, policy.Value);
            }
            else if (policy.Rule is ParentFolderNameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.ParentFolderName, policy.Value);
            }
            else if (policy.Rule is ParentFolderNameHeirarchicallyRule)
            {
                var folders = documentInfo.ParentFolderIncludingName.Split(new char[] { '\\', '/' });
                bool hasDoesNotCondition = false;
                bool hasDoesNotConditionAndNotQualified = false;
                foreach (var folder in folders)
                {
                    if (policy.Condition == PolicyCondition.DoesNotContains
                        || policy.Condition == PolicyCondition.DoesNotEquals
                        || policy.Condition == PolicyCondition.DoesNotMatch
                        || policy.Condition == PolicyCondition.IsExactlyNot)
                    {
                        hasDoesNotCondition = true;
                        if (!StringConditionChecker.IsQualified(policy.Condition, folder, policy.Value))
                        {
                            hasDoesNotConditionAndNotQualified = true;
                        }
                    }
                    else
                    {
                        if (StringConditionChecker.IsQualified(policy.Condition, folder, policy.Value))
                        {
                            return true;
                        }
                    }
                }
                //1.policy condition包含Does Not条件
                //2.且所有parent folder name都符合rule
                //满足以上两点才算符合Does Not条件
                if (hasDoesNotCondition && !hasDoesNotConditionAndNotQualified)
                {
                    return true;
                }
                return false;
            }
            else if (policy.Rule is ParentListNameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentInfo.ParentListName, policy.Value);
            }
            else if (policy.Rule is RetentionLabelRule)
            {
                string retentionLabel = string.Empty;
                if (documentInfo?.ColumnInfosOfInternalName != null && documentInfo.ColumnInfosOfInternalName.ContainsKey(SPColumnConstants.SP_ComplianceTag))
                {
                    retentionLabel = (documentInfo.ColumnInfosOfInternalName[SPColumnConstants.SP_ComplianceTag] as string) ?? string.Empty;
                }
                return StringConditionChecker.IsQualified(policy.Condition, retentionLabel, policy.Value);
            } 
            else if (policy.Rule is SensitivityLabelRule)
            {
                string sensitiveLabel = documentInfo.SensitivityLabel ?? string.Empty;
                return StringConditionChecker.IsQualified(policy.Condition, sensitiveLabel, policy.Value);
            }
            else if (policy.Rule is SensitivityLabelFullNameRule)
            {
                string sensitiveLabelFullName = documentInfo.SensitivityLabelFullName ?? string.Empty;
                return StringConditionChecker.IsQualified(policy.Condition, sensitiveLabelFullName, policy.Value);
            }
            else
            {
                var methodName = "Check" + policy.Rule.GetType().Name;
                var method = typeof(DocumentFilterEngine).GetMethod(
                    methodName,
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (method == null)
                {
                    throw new RuleNotSupportedException(policy.Rule.ToString());
                }
                try
                {
                    var resultObj = method.Invoke(null, new object[] { policy, documentInfo });
                    return resultObj is bool b && b;
                }
                catch (TargetInvocationException exception) when (exception.InnerException is PropertyNotAssignedException propertyException)
                {
                    ExceptionDispatchInfo.Capture(propertyException).Throw();
                    throw;
                }
            }
        }

        private static bool CheckPropertyBagTextRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            string columnValue = documentInfo.ParentSiteColumnInfos[policy.Rule.Value1].ToString();
            return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckPropertyBagNumberRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            double columnValue;
            try
            {
                columnValue = double.Parse(documentInfo.ParentSiteColumnInfos[policy.Rule.Value1].ToString());
            }
            catch (Exception e)
            {
                aveLogger.Warn(e.ToString());
                return false;
            }
            return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckPropertyBagDateTimeRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            DateTime columnValue;
            if (!DateTime.TryParse(documentInfo.ParentSiteColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
            {
                return false;
            }
            if (columnValue.Kind != DateTimeKind.Utc)
            {
                columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
            }
            return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckPropertyBagBooleanRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            bool columnValue = false;
            if (documentInfo.ParentSiteColumnInfos[policy.Rule.Value1] is string)
            {
                string value = documentInfo.ParentSiteColumnInfos[policy.Rule.Value1] as string;
                if (string.Equals("yes", value, StringComparison.OrdinalIgnoreCase) || string.Equals("true", value, StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = true;
                }
                else if (string.Equals("no", value, StringComparison.OrdinalIgnoreCase) || string.Equals("false", value, StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = false;
                }
                else
                {
                    return false;
                }
            }
            else if (documentInfo.ParentSiteColumnInfos[policy.Rule.Value1] is bool)
            {
                columnValue = (bool)documentInfo.ParentSiteColumnInfos[policy.Rule.Value1];
            }
            else
            {
                return false;
            }
            return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentSiteCollectionBooleanRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteCollectionColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            bool columnValue = false;
            if (documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1] is string)
            {
                string value = documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1] as string;
                if (string.Equals("yes", value, StringComparison.OrdinalIgnoreCase)
                    || string.Equals("true", value, StringComparison.OrdinalIgnoreCase))
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
            else if (documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1] is bool)
            {
                columnValue = (bool)documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1];
            }
            else
            {
                return false;
            }
            return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentSiteCollectionDateTimeRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteCollectionColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            DateTime columnValue;
            if (!DateTime.TryParse(documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1].ToString(), out columnValue))
            {
                return false;
            }
            if (columnValue.Kind != DateTimeKind.Utc)
            {
                columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
            }
            return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentSiteCollectionNumberRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteCollectionColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            double columnValue;
            try
            {
                columnValue = double.Parse(documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1].ToString());
            }
            catch (Exception e)
            {
                aveLogger.Warn(e.ToString());
                return false;
            }
            return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentSiteCollectionTextRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentSiteCollectionColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            string columnValue = documentInfo.ParentSiteCollectionColumnInfos[policy.Rule.Value1].ToString();
            return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentLibraryBooleanRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentListColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentListColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            bool columnValue = false;
            if (documentInfo.ParentListColumnInfos[policy.Rule.Value1] is string)
            {
                string value = documentInfo.ParentListColumnInfos[policy.Rule.Value1] as string;
                if (string.Equals("yes", value, StringComparison.OrdinalIgnoreCase) || string.Equals("true", value, StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = true;
                }
                else if (string.Equals("no", value, StringComparison.OrdinalIgnoreCase) || string.Equals("false", value, StringComparison.OrdinalIgnoreCase))
                {
                    columnValue = false;
                }
                else
                {
                    return false;
                }
            }
            else if (documentInfo.ParentListColumnInfos[policy.Rule.Value1] is bool)
            {
                columnValue = (bool)documentInfo.ParentListColumnInfos[policy.Rule.Value1];
            }
            else
            {
                return false;
            }
            return BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentLibraryDateTimeRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentListColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentListColumnInfos[policy.Rule.Value1] || !documentInfo.ParentListColumnInfos[policy.Rule.Value1].GetType().Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            DateTime columnValue = DateTime.Parse(documentInfo.ParentListColumnInfos[policy.Rule.Value1].ToString());
            return DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentLibraryNumberRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentListColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentListColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            double columnValue;
            try
            {
                columnValue = double.Parse(documentInfo.ParentListColumnInfos[policy.Rule.Value1].ToString());
            }
            catch (Exception e)
            {
                aveLogger.Warn(e.ToString());
                return false;
            }
            return NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }

        private static bool CheckParentLibraryTextRule(FilterPolicy policy, DocumentInfo documentInfo)
        {
            if (!documentInfo.ParentListColumnInfos.ContainsKey(policy.Rule.Value1) || null == documentInfo.ParentListColumnInfos[policy.Rule.Value1])
            {
                return false;
            }
            string columnValue = documentInfo.ParentListColumnInfos[policy.Rule.Value1].ToString();
            return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
        }
    }
}
