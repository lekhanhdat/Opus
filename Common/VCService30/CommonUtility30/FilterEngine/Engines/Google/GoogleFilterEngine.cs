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
using AvePoint.GCommon.Contract.CommonFilter.Rules;
using System;
using System.Collections.Generic;

namespace AvePoint.Common.FilterEngine.Engines.Google
{
    internal class GoogleFilterEngine : FilterEngineBase
    {
        public GoogleFilterEngine(List<FilterPolicy> policyLists, Dictionary<PolicyLevel, string> filterConditionExpressionLists, FilterEngine engine)
            : base(policyLists, filterConditionExpressionLists, engine)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            var fileInfo = objectInfo as GoogleItemInfo;
            bool isQualified = false;
            if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.Name, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is CreatedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.CreateByEmail, policy.Value);
                return isQualified;
            }
            else if (policy.Rule is ModifiedByRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, fileInfo.ModifiedByUserDisplayName, policy.Value);
                var isQualifiedByEmail = StringConditionChecker.IsQualified(policy.Condition, fileInfo.ModifiedByEmail, policy.Value); 
                return isQualified || isQualifiedByEmail;
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
                string extension = System.IO.Path.GetExtension(fileInfo.Name).TrimStart('.');
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
            else if (policy.Rule is LabelPropertyTextRule)
            {
                var labelName = policy.Rule.Value1;
                var fieldName = policy.Value.Value1;
                if (!fileInfo.LabelInfos.ContainsKey(labelName) || null == fileInfo.LabelInfos[labelName])
                {
                    return false;
                }
                Dictionary<string, List<string>> fieldsDics = (Dictionary<string, List<string>>)fileInfo.LabelInfos[labelName];
                List<string> fieldValues = fieldsDics.GetValueOrDefault($"text/{fieldName}");
                if (fieldValues == null)
                {
                    return false;
                }
                PolicyValue values = new();
                values.Value1 = policy.Value.Value2;
                values.Value1Unit = policy.Value.Value2Unit;
                foreach (string value in fieldValues)
                {
                    if (StringConditionChecker.IsQualified(policy.Condition, value, values))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (policy.Rule is LabelPropertyNumberRule)
            {
                var labelName = policy.Rule.Value1;
                var fieldName = policy.Value.Value1;
                if (!fileInfo.LabelInfos.ContainsKey(labelName) || null == fileInfo.LabelInfos[labelName])
                {
                    return false;
                }
                Dictionary<string, List<string>> fieldsDics = (Dictionary<string, List<string>>)fileInfo.LabelInfos[labelName];
                List<string> fieldValues = fieldsDics.GetValueOrDefault($"number/{fieldName}");
                if(fieldValues == null)
                {
                    return false;
                }
                PolicyValue values = new();
                values.Value1 = policy.Value.Value2;
                values.Value1Unit = policy.Value.Value2Unit;
                foreach (string value in fieldValues)
                {
                    if (long.TryParse(value, out long num) && NumberConditionChecker.IsQualified(policy.Condition, num, values))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (policy.Rule is LabelPropertyDateTimeRule)
            {
                var labelName = policy.Rule.Value1;
                var fieldName = policy.Value.Value1;
                if (!fileInfo.LabelInfos.ContainsKey(labelName) || null == fileInfo.LabelInfos[labelName])
                {
                    return false;
                }
                Dictionary<string, List<string>> fieldsDics = (Dictionary<string, List<string>>)fileInfo.LabelInfos[labelName];
                List<string> fieldValues = fieldsDics.GetValueOrDefault($"datetime/{fieldName}");
                PolicyValue values = new();
                values.Value1 = policy.Value.Value2;
                values.Value1Unit = policy.Value.Value2Unit;
                values.Value2 = policy.Value.Value3;
                values.Value2Unit = policy.Value.Value3Unit;
                if (fieldValues == null)
                {
                    return false;
                }
                foreach (string value in fieldValues)
                {
                    if (DateTime.TryParse(value, out DateTime date) && DateTimeConditionChecker.IsQualified(policy.Condition, date, values))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if(policy.Rule is LabelNameRule)
            {
                foreach(var labelName in fileInfo.LabelInfos.Keys)
                {
                    isQualified = StringConditionChecker.IsQualified(policy.Condition, (string) labelName, policy.Value);
                    if (isQualified)
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }
    }
}
