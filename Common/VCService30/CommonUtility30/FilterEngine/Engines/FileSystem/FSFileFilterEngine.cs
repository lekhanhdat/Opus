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
using AvePoint.GCommon.Contract.CommonFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Common.FilterEngine
{
    internal class FSFileFilterEngine : FilterEngineBase
    {

        public FSFileFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, GCommon.Contract.CommonFilter.FilterPolicy policy)
        {
            FSFileInfo fileInfo = objectInfo as FSFileInfo;
            bool isQualified = false;
            if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Name, policy.Value);
                RecordFilterLog("fsfile", isQualified, fileInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is NameAndExtentionRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Name, policy.Value);
                RecordFilterLog("fsfile", isQualified, fileInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is SizeRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, fileInfo.Size, policy.Value);
                RecordFilterLog("fsfile", isQualified, Convert.ToString(fileInfo.Size), policy);
                return isQualified;
            }
            else if (policy.Rule is FileExtensionsRule)
            {
                String extension = System.IO.Path.GetExtension(fileInfo.Name);
                isQualified = StringConditionChecker.IsQualified(policy.Condition, extension, policy.Value);
                RecordFilterLog("fsfile", isQualified, extension, policy);
                return isQualified;
            }
            else if (policy.Rule is ContentTypeRule)
            {
                String extension = System.IO.Path.GetExtension(fileInfo.Name);
                isQualified = StringConditionChecker.IsQualified(policy.Condition, extension, policy.Value);
                RecordFilterLog("fsfile", isQualified, extension, policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, fileInfo.Modified, policy.Value);
                RecordFilterLog("fsfile", isQualified, fileInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, fileInfo.Created, policy.Value);
                RecordFilterLog("fsfile", isQualified, fileInfo.Created.ToString(), policy);
                return isQualified;
            } 
            else if (policy.Rule is AccessTimeRule|| policy.Rule is StubLastAccessTimeRule)
            {
                if (fileInfo.AccessTime == DateTime.MinValue)
                {
                    return false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, fileInfo.AccessTime, policy.Value);
                    RecordFilterLog("fsfile", isQualified, fileInfo.AccessTime.ToString(), policy);
                    return isQualified;
                }
            }
            else if (policy.Rule is ColumnTextRule)
            {
                string columnName = policy.Rule.Value1;
                string columnValue = string.Empty;
                //var valueInCollection = base.GetColumnValue(policy, fileInfo.ColumnInfosOfDisplayName, fileInfo.ColumnInfosOfInternalName, fileInfo.IntrNameToDispName, fileInfo.SpecailColumnInfosOfDisplayName);
                if (!fileInfo.CGTag.TryGetValue(columnName, out columnValue))
                {
                    return false;
                }

                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog("fsfile", isQualified, columnValue, policy);
                return isQualified;
            }
            else if (policy.Rule is FSTermRule || policy.Rule is TermRule)
            {
                string columnValue;
                string columnName = policy.Rule.Value1.ToLowerInvariant();
                //if (!fileInfo.TermInfosOfDisplayName.ContainsKey(columnName))
                //{
                //    return false;
                //}
                columnValue = fileInfo.TermInfosOfDisplayName[columnName].ToString();
                isQualified = StringConditionChecker.IsQualified(policy.Condition, columnValue, policy.Value);
                RecordFilterLog("fsfile", isQualified, columnValue, policy);
                return isQualified;
            }
            else if (policy.Rule is OwnerRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Owner, policy.Value);
                RecordFilterLog("fsfile", isQualified, fileInfo.Owner, policy);
                return isQualified;
            }
            else if (policy.Rule is FilePathRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.FilePath, policy.Value);
                RecordFilterLog("fsfile", isQualified, fileInfo.FilePath, policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
