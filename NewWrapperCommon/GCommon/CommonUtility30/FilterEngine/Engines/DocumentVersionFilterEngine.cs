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
        public DocumentVersionFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            DocumentVersionInfo documentVersionInfo = objectInfo as DocumentVersionInfo;
            Boolean isQualified = false;
            if (policy.Rule is TitleRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Title, policy.Value);
                RecordFilterLog(isQualified, documentVersionInfo.Title, policy);
                return isQualified;
            }
            if (policy.Rule is FileExtensionsRule)
            {
                String extension = System.IO.Path.GetExtension(documentVersionInfo.Name);
                isQualified = StringConditionChecker.IsQualified(policy.Condition, extension, policy.Value);
                RecordFilterLog(isQualified, extension, policy);
                return isQualified;
            }
            if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Name, policy.Value);
                RecordFilterLog(isQualified, documentVersionInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is SizeRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Size, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(documentVersionInfo.Size), policy);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, documentVersionInfo.Modified, policy.Value);
                RecordFilterLog(isQualified, documentVersionInfo.Modified.ToString(), policy);
                return isQualified;
            }
            else if (policy.Rule is StubLastAccessTimeRule)
            {
                if (documentVersionInfo.IsStub && documentVersionInfo.StubLastAccessTime != new DateTime())
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, documentVersionInfo.StubLastAccessTime, policy.Value);
                    RecordFilterLog(isQualified, documentVersionInfo.StubLastAccessTime.ToString(), policy);
                    return isQualified;
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
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByLogonName, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByLogonNameWithPrefix, policy.Value) && StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByTitle, policy.Value);
                    
                }
                else
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByLogonName, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByLogonNameWithPrefix, policy.Value) || StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ModifiedByTitle, policy.Value);
                }
                RecordFilterLog(isQualified, new List<string>(){ 
                    documentVersionInfo.ModifiedByLogonName,
                    documentVersionInfo.ModifiedByTitle,
                    documentVersionInfo.ModifiedByLogonNameWithPrefix
                }, policy);
                return isQualified;
            }
            else if (policy.Rule is KeepHistoryVersionRule)
            {
                isQualified = VersionConditionChecker.IsQualified(policy.Condition, documentVersionInfo, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(documentVersionInfo.VersionSequenceNo), policy);
                return isQualified;
            }
            else if (policy.Rule is VersionsRule)
            {
                isQualified = VersionConditionChecker.IsQualified(policy.Condition, documentVersionInfo, policy.Value);
                RecordFilterLog(isQualified, Convert.ToString(documentVersionInfo.VersionSequenceNo), policy);
                return isQualified;
            }
            else if (policy.Rule is ListTypeRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, documentVersionInfo.ListType, policy.Value);
                RecordFilterLog(isQualified, documentVersionInfo.ListType, policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }


        protected override PolicyLevel Level
        {
            get { return PolicyLevel.DocumentVersion; }
        }
    }
}
