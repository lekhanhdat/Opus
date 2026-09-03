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
    #endregion

    internal class DocumentVersionFilterEngine : FilterEngineBase
    {
        public DocumentVersionFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            DocumentVersionInfo documentVersionInfo = objectInfo as DocumentVersionInfo;
            if (policy.Rule is TitleRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Title, policy.Value);
            }
            if (policy.Rule is DocumentName)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Name, policy.Value);
            }
            if (policy.Rule is NameRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Name, policy.Value);
            }
            else if (policy.Rule is SizeRule)
            {
                return NumberConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Size, policy.Value);
            }
            else if (policy.Rule is ModifiedRule || policy.Rule is DocumentModifiedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Modified, policy.Value);
            }
            else if (policy.Rule is CreatedRule)
            {
                return DateTimeConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Created, policy.Value);
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                if (documentVersionInfo.IsStub)
                {
                    return DateTimeConditionChecker.IsQualified(policy.Condition, documentVersionInfo.StubLastAccessTime, policy.Value);
                }
                else
                {
                    return false;
                }
            }
            else if (policy.Rule is StubLastActiveTimeRule)
            {
                if (documentVersionInfo.IsStub)
                {
                    return DateTimeConditionChecker.IsQualified(policy.Condition, documentVersionInfo.LastAccessCompatibleModifiedTime, policy.Value);
                }
                else
                {
                    return false;
                }
            }
            else if (policy.Rule is ModifiedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    return StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByTitle, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByEmail, policy.Value);
                }
                else
                {
                    return StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByTitle, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByEmail, policy.Value);
                }
            }
            else if (policy.Rule is KeepHistoryVersionRule)
            {
                return VersionConditionChecker.IsQualified(policy.Condition, documentVersionInfo, policy.Value);
            }
            else if (policy.Rule is VersionsRule)
            {
                return VersionConditionChecker.IsQualified(policy.Condition, documentVersionInfo, policy.Value);
            }
            else if (policy.Rule is ListTypeRule)
            {
                return StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ListType, policy.Value);
            }
            else if (policy.Rule is ColumnTextRule)
            {
                string columnValue;
                var valueCollection = base.GetColumnValue(policy, documentVersionInfo.ColumnInfos, documentVersionInfo.ColumnInfosOfInternalName, null, null);
                if (!TryGetValue(valueCollection, out columnValue))
                {
                    if (documentVersionInfo.ListColumnExistInfos != null && documentVersionInfo.ListColumnExistInfos.ContainsKey(policy.Rule.Value1) && documentVersionInfo.ListColumnExistInfos[policy.Rule.Value1] == true)
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
            //else if (policy.Rule is SensitivityLabelRule)
            //{
            //    string sensitiveLabel = documentVersionInfo.SensitivityLabel ?? string.Empty;
            //    return StringConditionChecker.IsQualified(policy.Condition, sensitiveLabel, policy.Value);
            //}
            else if (policy.Rule is SensitivityLabelFullNameRule)
            {
                string sensitiveLabelFullName = documentVersionInfo.SensitivityLabelFullName ?? string.Empty;
                return StringConditionChecker.IsQualified(policy.Condition, sensitiveLabelFullName, policy.Value);
            }
            else if (policy.Rule is MetadataTextColumnRule)
            {
                if (!documentVersionInfo.ColumnInfos.ContainsKey(policy.Rule.Value1.ToLower()) || null == documentVersionInfo.ColumnInfos[policy.Rule.Value1.ToLower()])
                {
                    return false;
                }
                //Client API Managed Metadata Column Value: 2;#ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                //Wrapper Discover Managed Metadata Column Value:ccc|22f0dae8-aa52f-4ef5-b609-f59687139bb6
                string tempValue = documentVersionInfo.ColumnInfos[policy.Rule.Value1.ToLower()].ToString();
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
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

    }
}
