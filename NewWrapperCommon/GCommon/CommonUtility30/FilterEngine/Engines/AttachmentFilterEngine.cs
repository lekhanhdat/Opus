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

    internal class AttachmentFilterEngine : FilterEngineBase
    {
        public AttachmentFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            AttachmentInfo attachmentInfo = objectInfo as AttachmentInfo;
            Boolean isQualified = false;

            if (policy.Rule is NameAndExtentionRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.Name, policy.Value);
                RecordFilterLog(isQualified, attachmentInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.Name, policy.Value);
                RecordFilterLog(isQualified, attachmentInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is SizeRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, attachmentInfo.Size, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(attachmentInfo.Size), policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, attachmentInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.Created, policy.Value);
                RecordFilterLog(isQualified, attachmentInfo.Created.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                if (attachmentInfo.IsStub)
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.StubLastAccessTime, policy.Value);
                    RecordFilterLog(isQualified, attachmentInfo.StubLastAccessTime.ToString(), policy);
                    return isQualified;
                }
                else
                {
                    return false;
                }
            }
            else if (policy.Rule is AccessTimeRule)
            {
                if (attachmentInfo.AccessTime == DateTime.MinValue)
                {
                    return false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, attachmentInfo.AccessTime, policy.Value);
                    RecordFilterLog(isQualified, attachmentInfo.AccessTime.ToString(), policy);
                    return isQualified;
                }
            }
            else if (policy.Rule is ModifiedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ModifiedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ModifiedByTitle, policy.Value);
                RecordFilterLog(isQualified, new List<string>(){ 
                    attachmentInfo.ModifiedByLogonName,
                    attachmentInfo.ModifiedByTitle,
                    attachmentInfo.ModifiedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedByRule)
            {
                if (policy.Condition == PolicyCondition.DoesNotContains)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByLogonNameWithPrefix, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByTitle, policy.Value);
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.CreatedByTitle, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    attachmentInfo.CreatedByLogonName,
                    attachmentInfo.CreatedByTitle,
                    attachmentInfo.CreatedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is ColumnTextRule)
            {
                string columnValue;
                var valueInCollection = base.GetColumnValue(policy, attachmentInfo.ColumnInfosOfDisplayName, attachmentInfo.ColumnInfosOfInternalName, attachmentInfo.IntrNameToDispName, attachmentInfo.SpecailColumnInfosOfDisplayName);
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
                var valueInCollection = base.GetColumnValue(policy, attachmentInfo.ColumnInfosOfDisplayName, attachmentInfo.ColumnInfosOfInternalName, attachmentInfo.IntrNameToDispName, attachmentInfo.SpecailColumnInfosOfDisplayName);
                if (!TryGetValue(valueInCollection, out columnValue,true))
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
                var valueInCollection = base.GetColumnValue(policy, attachmentInfo.ColumnInfosOfDisplayName, attachmentInfo.ColumnInfosOfInternalName, attachmentInfo.IntrNameToDispName, attachmentInfo.SpecailColumnInfosOfDisplayName, "DateTime");
                if (!TryGetValue(valueInCollection, out columnValue))
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
                var valueInCollection = base.GetColumnValue(policy, attachmentInfo.ColumnInfosOfDisplayName, attachmentInfo.ColumnInfosOfInternalName, attachmentInfo.IntrNameToDispName, attachmentInfo.SpecailColumnInfosOfDisplayName, "Boolean");
                if (!TryGetValue(valueInCollection, out columnValue))
                {
                    return false;
                }
                isQualified = BooleanConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(columnValue), policy);
                return isQualified;
            }
            else if (policy.Rule is ListTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, attachmentInfo.ListType, policy.Value);
                RecordFilterLog(isQualified, attachmentInfo.ListType, policy);
                return isQualified;
            }
            else if (policy.Rule is FileExtensionsRule)
            {
                String extension = System.IO.Path.GetExtension(attachmentInfo.Name);
                isQualified = StringConditionChecker.IsQualified(policy.Condition, extension, policy.Value);
                RecordFilterLog(isQualified, extension, policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

        protected override PolicyLevel Level
        {
            get { return PolicyLevel.Attachment; }
        }
    }
}
