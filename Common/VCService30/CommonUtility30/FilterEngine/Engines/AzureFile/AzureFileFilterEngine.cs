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
using AvePoint.Common.FilterEngine.ObjectInfos;
using AvePoint.GCommon.Contract.CommonFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Common.FilterEngine
{
    internal class AzureFileFilterEngine : FilterEngineBase
    {
        public AzureFileFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, GCommon.Contract.CommonFilter.FilterPolicy policy)
        {
            AzureFileInfo fileInfo = objectInfo as AzureFileInfo;
            bool isQualified = false;
            if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Name, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is NameAndExtentionRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Name, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is SizeRule)
            {
                isQualified = NumberConditionChecker.IsQualified(policy.Condition, fileInfo.Size, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is FileExtensionsRule)
            {
                String extension = System.IO.Path.GetExtension(fileInfo.Name);
                isQualified = StringConditionChecker.IsQualified(policy.Condition, extension, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is ModifiedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, fileInfo.Modified, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is CreatedRule)
            {
                isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, fileInfo.Created, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is AccessTimeRule || policy.Rule is StubLastAccessTimeRule)
            {
                if (fileInfo.AccessTime == DateTime.MinValue)
                {
                    return false;
                }
                else
                {
                    isQualified = DateTimeConditionChecker.IsQualified(policy.Condition, fileInfo.AccessTime, policy.Value);
                    return isQualified;
                }
            }
            else if (policy.Rule is FilePathRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Path, policy.Value);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
