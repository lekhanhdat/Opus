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
    using System.Diagnostics.CodeAnalysis;
    #endregion

    internal class DocumentFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public DocumentFilterEngine(FilterOption option)
            : base(option)
        {
        }

        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            DocumentInfo documentInfo = objectInfo as DocumentInfo;
            Boolean isQualified = false;
            if (policy.Rule is UrlRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.Url, policy.Value);
                RecordFilterLog(isQualified, documentInfo.Url, policy);
                return isQualified;
            }
            else if (policy.Rule is NameAndExtentionRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.Name, policy.Value);
                RecordFilterLog(isQualified, documentInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.Name, policy.Value);
                RecordFilterLog(isQualified, documentInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, documentInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.Created, policy.Value);
                RecordFilterLog(isQualified, documentInfo.Created.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                if (documentInfo.IsStub)
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.StubLastAccessTime, policy.Value);
                    RecordFilterLog(isQualified, documentInfo.StubLastAccessTime.ToString(), policy);
                    return isQualified;
                }
                else
                {
                    return false;
                }
            }
            else if (policy.Rule is AccessTimeRule)
            {
                if (documentInfo.AccessTime == DateTime.MinValue)
                {
                    return false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, documentInfo.AccessTime, policy.Value);
                    RecordFilterLog(isQualified, documentInfo.AccessTime.ToString(), policy);
                    return isQualified;
                }
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByLogonNameWithPrefix, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByTitle, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.ModifiedByTitle, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    documentInfo.ModifiedByLogonName,
                    documentInfo.ModifiedByTitle,
                    documentInfo.ModifiedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByLogonNameWithPrefix, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByTitle, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentInfo.CreatedByTitle, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    documentInfo.CreatedByLogonName,
                    documentInfo.CreatedByTitle,
                    documentInfo.CreatedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is ContentTypeNameRule || policy.Rule is ContentTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ContentType, policy.Value);
                RecordFilterLog(isQualified, documentInfo.ContentType, policy);
                return isQualified;
            }
            else if (policy.Rule is ContentTypeIdRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ContentTypeId, policy.Value);
                RecordFilterLog(isQualified, documentInfo.ContentTypeId, policy);
                return isQualified;
            }
            else if (policy.Rule is SizeRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, documentInfo.Size, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(documentInfo.Size), policy);
                return isQualified;
            }
            else if (policy.Rule is TermRule)
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                if (!documentInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                {
                    return false;
                }
                columnValue = documentInfo.TermInfosOfDisplayName[columnName].ToString();
                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue, policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnTextRule)
            {
                string columnValue;
                var valueInCollection = base.GetColumnValue(policy, documentInfo.ColumnInfosOfDisplayName, documentInfo.ColumnInfosOfInternalName, documentInfo.IntrNameToDispName, documentInfo.SpecailColumnInfosOfDisplayName);
                if(!TryGetValue(valueInCollection,out columnValue))
                {
                    return false;
                }
                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue, policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnNumberRule)
            {
                double columnValue;
                var valueInCollection = base.GetColumnValue(policy, documentInfo.ColumnInfosOfDisplayName, documentInfo.ColumnInfosOfInternalName, documentInfo.IntrNameToDispName, documentInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection,out  columnValue,true))
                {
                    return false;
                }
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnDateTimeRule)
            {
                DateTime columnValue;
                var valueInCollection = base.GetColumnValue(policy, documentInfo.ColumnInfosOfDisplayName, documentInfo.ColumnInfosOfInternalName, documentInfo.IntrNameToDispName, documentInfo.SpecailColumnInfosOfDisplayName, "DateTime");
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    return false;
                }
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                bool columnValue;
                var valueInCollection =base.GetColumnValue(policy, documentInfo.ColumnInfosOfDisplayName, documentInfo.ColumnInfosOfInternalName, documentInfo.IntrNameToDispName, documentInfo.SpecailColumnInfosOfDisplayName, "Boolean");
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    return false;
                }
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
                return isQualified;
            }
            else if (policy.Rule is CustomPropertyBaseRule)
            {
                isQualified = QualifyCustomProperty(policy, documentInfo.ColumnInfosOfDisplayName);
                return isQualified;
            }
            else if (policy.Rule is VersionsRule)
            {
                isQualified = VersionConditionChecker.IsQualified(policy.Condition, documentInfo, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(documentInfo.VersionSequenceNo), policy);
                return isQualified;
            }
            else if (policy.Rule is InheritanceRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, documentInfo.InheritPermission, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(documentInfo.InheritPermission), policy);
                return isQualified;
            }
            else if (policy.Rule is UserAndGroupRule)//此rule不输出Log。
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is WorkflowRule)
            {
                //if the specific workflow status field value does not exist, if condition is IsExactlyNot return true,otherwise return false
                if (documentInfo.WorkflowStatus == null
                    || documentInfo.WorkflowStatus.Count == 0
                    || !documentInfo.WorkflowStatus.ContainsKey(policy.Rule.Value1.ToLower())
                    || null == documentInfo.WorkflowStatus[policy.Rule.Value1.ToLower()])
                {
                    return policy.Condition == PolicyCondition.IsExactlyNot;
                }
                string wfStatus = documentInfo.WorkflowStatus[policy.Rule.Value1.ToLower()].ToString();
                if (!string.IsNullOrEmpty(policy.Value.Value2))//Wrokflow Customized Status
                {
                    policy.Value.Value1 = policy.Value.Value2;
                }
                isQualified = StringConditionChecker.IsQualified(policy.Condition, wfStatus, policy.Value);
                RecordFilterLog(isQualified, wfStatus, policy);
                return isQualified;

            }
            else if (policy.Rule is FileExtensionsRule)
            {
                String extension = System.IO.Path.GetExtension(documentInfo.Name);
                isQualified = StringConditionChecker.IsQualified(policy.Condition, extension, policy.Value);
                RecordFilterLog(isQualified, extension, policy);
                return isQualified;
            }
            else if (policy.Rule is ListTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ListType, policy.Value);
                RecordFilterLog(isQualified, documentInfo.ListType, policy);
                return isQualified;
            }
            else if (policy.Rule is ParentSiteCustomPropertyColumnTextRule)
            {
                isQualified = QualifyCustomProperty(policy, documentInfo.ParentSiteProperties);
                return isQualified;
            }
            else if (policy.Rule is ParentSiteCollectionCustomPropertyColumnTextRule)
            {
                isQualified = QualifyCustomProperty(policy, documentInfo.ParentSiteCollectionProperties);
                return isQualified;
            }
            else if (policy.Rule is ParentFolderNameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ParentFolderName, policy.Value);
                RecordFilterLog(isQualified, documentInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is ChoiceRule)
            {
                string columnValue;
                var valueInCollection = base.GetColumnValue(policy, documentInfo.ColumnInfosOfDisplayName, documentInfo.ColumnInfosOfInternalName, documentInfo.IntrNameToDispName, documentInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    return false;
                }
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            #region This is for Migration Custom Column filter / Content Type condition will be handled in ContentTypeRule
            else if (policy.Rule is CustomContentTypeRule)
            {
                CustomContentTypeRule ctr = policy.Rule as CustomContentTypeRule;

                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentInfo.ContentType, policy.Value);
                RecordFilterLog(isQualified, documentInfo.ContentType, policy);
                return isQualified;
            }
            else if (policy.Rule is CustomColumnRule)
            {
                CustomColumnRule ccr = policy.Rule as CustomColumnRule;

                bool bCheckRet = true;
                string fieldTitle = string.Empty;
                if (!string.IsNullOrEmpty(ccr.InternalName))
                {
                    if (documentInfo.IntrNameToDispName == null || !documentInfo.IntrNameToDispName.ContainsKey(ccr.InternalName))
                    {
                        bCheckRet = false;
                    }
                    else
                    {
                        fieldTitle = documentInfo.IntrNameToDispName[ccr.InternalName].ToString();
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(ccr.DisplayName))
                    {
                        fieldTitle = ccr.DisplayName.ToLower();
                    }
                    else
                    {
                        bCheckRet = false;
                    }
                }

                if (!bCheckRet)
                {
                    return bCheckRet;
                }

                string fieldType = string.Empty;
                if (!string.IsNullOrEmpty(ccr.FieldType))
                {
                    if (documentInfo.DispNameToType == null || !documentInfo.DispNameToType.ContainsKey(fieldTitle))
                    {
                        bCheckRet = false;
                    }
                    fieldType = ccr.FieldType.ToLower();
                    PolicyValue ftValue = new PolicyValue(fieldType);
                    if (bCheckRet &&
                        (!StringConditionChecker.IsQualified(PolicyCondition.Equals, documentInfo.DispNameToType[fieldTitle].ToString(), ftValue)))
                    {
                        isQualified = false;
                        RecordFilterLog(isQualified, documentInfo.DispNameToType[fieldTitle].ToString(), policy);
                        return isQualified;
                    }
                }
                else
                {
                    fieldType = documentInfo.DispNameToType[fieldTitle].ToString();
                }

                if (!bCheckRet)
                {
                    return bCheckRet;
                }

                //For now, we only support the following field types. All the types beyond DateTime/Number/Boolean will be handled as same as Text.
                object colValue;
                if (documentInfo.ColumnInfosOfDisplayName != null && documentInfo.ColumnInfosOfDisplayName.ContainsKey(fieldTitle))
                {
                    switch (fieldType)
                    {
                        case "datetime":
                            colValue = DateTime.Parse(documentInfo.ColumnInfosOfDisplayName[fieldTitle].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                            colValue = DateTime.SpecifyKind((DateTime)colValue, DateTimeKind.Utc);
                            bCheckRet = DateTimeConditionChecker.IsQualified(policy.Condition, (DateTime)colValue, policy.Value);
                            break;
                        case "number":
                            colValue = double.Parse(documentInfo.ColumnInfosOfDisplayName[fieldTitle].ToString());
                            bCheckRet = NumberConditionChecker.IsQualified(policy.Condition, (double)colValue, policy.Value);
                            break;
                        case "boolean":
                            colValue = bool.Parse(documentInfo.ColumnInfosOfDisplayName[fieldTitle].ToString());
                            bCheckRet = BooleanConditionChecker.IsQualified(policy.Condition, (bool)colValue, policy.Value);
                            break;
                        default:
                            colValue = documentInfo.ColumnInfosOfDisplayName[fieldTitle].ToString();
                            bCheckRet = StringConditionChecker.IsQualified(policy.Condition, colValue.ToString(), policy.Value);
                            break;
                    }
                    RecordFilterLog(bCheckRet, colValue.ToString(), policy);
                    //用动态变量了要测一下。
                }
                else
                {
                    bCheckRet = false;
                }

                return bCheckRet;
            }
            

            #endregion
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }


        protected override PolicyLevel Level
        {
            get { return PolicyLevel.Document; }
        }
    }
}
