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

    internal class ItemFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ItemFilterEngine(FilterOption option)
            : base(option)
        {
        }


        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToLower")]
        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            ItemInfo itemInfo = objectInfo as ItemInfo;
            Boolean isQualified = false;

            if (policy.Rule is UrlRule)
            {
                switch (policy.Condition)
                {
                    case PolicyCondition.RexMatch:
                    case PolicyCondition.Match:
                    case PolicyCondition.Exactly:
                    case PolicyCondition.Equals:
                    case PolicyCondition.Contains:
                        isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.Url, policy.Value) ||
                            StringConditionChecker.IsQualified(policy.Condition, itemInfo.DisplayFormUrl, policy.Value);
                        break;
                    case PolicyCondition.RexNotMatch:
                    case PolicyCondition.DoesNotMatch:
                    case PolicyCondition.IsExactlyNot:
                    case PolicyCondition.DoesNotContains:
                        isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.Url, policy.Value) &&
                            StringConditionChecker.IsQualified(policy.Condition, itemInfo.DisplayFormUrl, policy.Value);
                        break;
                    default:
                        throw new ConditionNotSupportedException(policy.Condition.ToString());
                }
                RecordFilterLog(isQualified, itemInfo.Url, policy);
                RecordFilterLog(isQualified, itemInfo.DisplayFormUrl, policy);
                return isQualified;
            }
            else if (policy.Rule is TitleRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.Title, policy.Value);
                RecordFilterLog(isQualified, itemInfo.Title, policy);
                return isQualified;
            }
            else if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.Title, policy.Value);
                RecordFilterLog(isQualified, itemInfo.Title, policy);
                return isQualified;
            }
            else if (policy.Rule is NameAndExtentionRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.Title, policy.Value);
                RecordFilterLog(isQualified, itemInfo.Title, policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, itemInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, itemInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, itemInfo.Created, policy.Value);
                RecordFilterLog(isQualified, itemInfo.Created.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByLogonNameWithPrefix, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.ModifiedByLogonNameWithPrefix, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    itemInfo.ModifiedByLogonName,
                    itemInfo.ModifiedByTitle,
                    itemInfo.ModifiedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByLogonNameWithPrefix, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, itemInfo.CreatedByLogonNameWithPrefix, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    itemInfo.CreatedByLogonName,
                    itemInfo.CreatedByTitle,
                    itemInfo.CreatedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is ContentTypeNameRule || policy.Rule is ContentTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.ContentType, policy.Value);
                RecordFilterLog(isQualified, itemInfo.ContentType, policy);
                return isQualified;
            }
            else if (policy.Rule is ContentTypeIdRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.ContentTypeId, policy.Value);
                RecordFilterLog(isQualified, itemInfo.ContentTypeId, policy);
                return isQualified;
            }
            else if (policy.Rule is TermRule)
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                if (!itemInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                {
                    return false;
                }
                columnValue = itemInfo.TermInfosOfDisplayName[columnName].ToString();
                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue, policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnTextRule)
            {
                string columnValue;
                var valueInCollection = base.GetColumnValue(policy, itemInfo.ColumnInfosOfDisplayName, itemInfo.ColumnInfosOfInternalName, itemInfo.IntrNameToDispName, itemInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection, out columnValue))
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
                var valueInCollection = base.GetColumnValue(policy, itemInfo.ColumnInfosOfDisplayName, itemInfo.ColumnInfosOfInternalName, itemInfo.IntrNameToDispName, itemInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection,out columnValue,true))
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
                var valueInCollection = base.GetColumnValue(policy, itemInfo.ColumnInfosOfDisplayName, itemInfo.ColumnInfosOfInternalName, itemInfo.IntrNameToDispName, itemInfo.SpecailColumnInfosOfDisplayName, "DateTime");
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    return false;
                }
                columnValue = DateTime.SpecifyKind(columnValue, DateTimeKind.Utc);
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnBooleanRule)
            {
                bool columnValue;
                var valueInCollection = base.GetColumnValue(policy, itemInfo.ColumnInfosOfDisplayName, itemInfo.ColumnInfosOfInternalName, itemInfo.IntrNameToDispName, itemInfo.SpecailColumnInfosOfDisplayName, "Boolean");
                if (!TryGetValue(valueInCollection,out columnValue))
                {
                    return false;
                }
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, columnValue.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CustomPropertyBaseRule)
            {
                isQualified = QualifyCustomProperty(policy, itemInfo.ColumnInfosOfDisplayName);
                return isQualified;
            }
            else if (policy.Rule is VersionsRule)
            {
                isQualified = VersionConditionChecker.IsQualified(policy.Condition, itemInfo, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(itemInfo.VersionSequenceNo), policy);
                return isQualified;
            }
            else if (policy.Rule is InheritanceRule)
            {
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, itemInfo.InheritPermission, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(itemInfo.InheritPermission), policy);
                return isQualified;
            }
            else if (policy.Rule is UserAndGroupRule)
            {
                if (!policy.Result.HasValue) throw new PolicyNotEvaluatedException();
                return policy.Result.Value;
            }
            else if (policy.Rule is WorkflowRule)
            {
                //if the specific workflow status field value does not exist, if condition is IsExactlyNot return true,otherwise return false
                if (itemInfo.WorkflowStatus == null
                    || itemInfo.WorkflowStatus.Count == 0
                    || !itemInfo.WorkflowStatus.ContainsKey(policy.Rule.Value1.ToLower())
                    || null == itemInfo.WorkflowStatus[policy.Rule.Value1.ToLower()])
                {
                    return policy.Condition == PolicyCondition.IsExactlyNot;
                }
                string wfStatus = itemInfo.WorkflowStatus[policy.Rule.Value1.ToLower()].ToString();
                if (!string.IsNullOrEmpty(policy.Value.Value2))//Workflow Customized Status
                {
                    policy.Value.Value1 = policy.Value.Value2;
                }
                isQualified = StringConditionChecker.IsQualified(policy.Condition, wfStatus, policy.Value);
                RecordFilterLog(isQualified, wfStatus, policy);
                return isQualified;

            }
            else if (policy.Rule is ListTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.ListType, policy.Value);
                RecordFilterLog(isQualified, itemInfo.ListType, policy);
                return isQualified;

            }
            else if (policy.Rule is ParentSiteCustomPropertyColumnTextRule)
            {
                isQualified = QualifyCustomProperty(policy, itemInfo.ParentSiteProperties);
                return isQualified;
            }
            else if (policy.Rule is ParentSiteCollectionCustomPropertyColumnTextRule)
            {
                isQualified = QualifyCustomProperty(policy, itemInfo.ParentSiteCollectionProperties);
                return isQualified;
            }
            else if (policy.Rule is ChoiceRule)
            {
                string columnValue;
                if (!TryGetValue(
                    base.GetColumnValue(policy, itemInfo.ColumnInfosOfDisplayName, itemInfo.ColumnInfosOfInternalName, itemInfo.IntrNameToDispName, itemInfo.SpecailColumnInfosOfDisplayName),
                    out columnValue))
                {
                    return false;
                }
                return StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
            }
            else if (policy.Rule is AccessTimeRule)
            {
                if (itemInfo.AccessTime == DateTime.MinValue)
                {
                    return false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, itemInfo.AccessTime, policy.Value);
                    RecordFilterLog(isQualified, itemInfo.AccessTime.ToString(), policy);
                    return isQualified;
                }
            }

            #region This is for Migration Custom Column filter / Content Type condition will be handled in ContentTypeRule
            else if (policy.Rule is CustomContentTypeRule)
            {
                CustomContentTypeRule ctr = policy.Rule as CustomContentTypeRule;

                isQualified = StringConditionChecker.IsQualified(policy.Condition, itemInfo.ContentType, policy.Value);
                RecordFilterLog(isQualified, itemInfo.ContentType, policy);
                return isQualified;
            }
            else if (policy.Rule is CustomColumnRule)
            {
                CustomColumnRule ccr = policy.Rule as CustomColumnRule;

                bool bCheckRet = true;
                string fieldTitle = string.Empty;
                if (!string.IsNullOrEmpty(ccr.InternalName))
                {
                    if (itemInfo.IntrNameToDispName == null || !itemInfo.IntrNameToDispName.ContainsKey(ccr.InternalName))
                    {
                        bCheckRet = false;
                    }
                    else
                    {
                        fieldTitle = itemInfo.IntrNameToDispName[ccr.InternalName].ToString();
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
                    if (itemInfo.DispNameToType == null || !itemInfo.DispNameToType.ContainsKey(fieldTitle))
                    {
                        bCheckRet = false;
                    }
                    fieldType = ccr.FieldType.ToLower();
                    PolicyValue ftValue = new PolicyValue(fieldType);
                    if (bCheckRet &&
                        (!StringConditionChecker.IsQualified(PolicyCondition.Equals, itemInfo.DispNameToType[fieldTitle].ToString(), ftValue)))
                    {
                        bCheckRet = false;
                        RecordFilterLog(bCheckRet, itemInfo.DispNameToType[fieldTitle].ToString(), policy);
                    }
                }
                else
                {
                    fieldType = itemInfo.DispNameToType[fieldTitle].ToString();
                }

                if (!bCheckRet)
                {
                    return bCheckRet;
                }

                //For now, we only support the following field types. All the types beyond DateTime/Number/Boolean will be handled as same as Text.
                object colValue;
                if (itemInfo.ColumnInfosOfDisplayName != null && itemInfo.ColumnInfosOfDisplayName.ContainsKey(fieldTitle))
                {
                    switch (fieldType)
                    {
                        case "datetime":
                            colValue = DateTime.Parse(itemInfo.ColumnInfosOfDisplayName[fieldTitle].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                            colValue = DateTime.SpecifyKind((DateTime)colValue, DateTimeKind.Utc);
                            bCheckRet = DateTimeConditionChecker.IsQualified(policy.Condition, (DateTime)colValue, policy.Value);
                            break;
                        case "number":
                            colValue = double.Parse(itemInfo.ColumnInfosOfDisplayName[fieldTitle].ToString());
                            bCheckRet = NumberConditionChecker.IsQualified(policy.Condition, (double)colValue, policy.Value);
                            break;
                        case "boolean":
                            colValue = bool.Parse(itemInfo.ColumnInfosOfDisplayName[fieldTitle].ToString());
                            bCheckRet = BooleanConditionChecker.IsQualified(policy.Condition, (bool)colValue, policy.Value);
                            break;
                        default:
                            colValue = itemInfo.ColumnInfosOfDisplayName[fieldTitle].ToString();
                            bCheckRet = StringConditionChecker.IsQualified(policy.Condition, colValue.ToString(), policy.Value);
                            break;
                    }
                    RecordFilterLog(bCheckRet, colValue.ToString(), policy);
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
            get { return  PolicyLevel.Item; }
        }
    }
}
