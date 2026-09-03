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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.Common.FilterEngine
{
    internal class TreeNodeFilterEngine : FilterEngineBase
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public TreeNodeFilterEngine(FilterOption option)
            : base(option)
        {
        }

        protected override bool IsQualified(ObjectInfoBase objectInfo, FilterPolicy policy)
        {
            var treeNodeInfo = objectInfo as TreeNodeInfo;
            Boolean isQualified = false;

            if (policy.Rule is NameRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, treeNodeInfo.Name, policy.Value);
                RecordFilterLog(isQualified, treeNodeInfo.Name, policy);
                return isQualified;
            }
            else if (policy.Rule is UrlRule)
            {
                isQualified = StringConditionChecker.IsQualified(policy.Condition, treeNodeInfo.Url, policy.Value);
                RecordFilterLog(isQualified, treeNodeInfo.Url, policy);
                return isQualified;
            }
            else
            {
                throw new RuleNotSupportedException(policy.Rule.ToString());
            }
        }

        protected override PolicyLevel Level
        {
            get { return PolicyLevel.AdvancedSearch; }
        }
    }
}
