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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal abstract class DO2SOTimeBaseFilter : DO2SOFilterConverterBase
    {
        private readonly string mValue1;
        private readonly string mValue2;
        private readonly PolicyValueUnit mValueUnit;
        private readonly int mStartSequenceNo;
        private readonly TimeCondition mCondition;
        private readonly bool mIsVersionFilter;
        private string mAndOrString;

        public override string AndOrString
        {
            get
            {
                return mAndOrString;
            }
        }

        public abstract TimeRuleType RuleType
        {
            get;
        }

        public abstract PolicyRuleBase CreateNewRuleInstance();

        public DO2SOTimeBaseFilter(string value1, PolicyValueUnit valueUnit, int startSequenceNo, TimeCondition condition, bool isVersionFilter)
            : this(value1, string.Empty, valueUnit, startSequenceNo, condition, isVersionFilter)
        {
        }

        public DO2SOTimeBaseFilter(string value1, string value2, PolicyValueUnit valueUnit, int startSequenceNo, TimeCondition condition, bool isVersionFilter)
        {
            this.mValue1 = value1;
            this.mValue2 = value2;
            this.mValueUnit = valueUnit;
            this.mStartSequenceNo = startSequenceNo;
            this.mCondition = condition;
            this.mIsVersionFilter = isVersionFilter;
        }
        public override List<SOFilterPolicy> Convert()
        {
            List<SOFilterPolicy> res = new List<SOFilterPolicy>();

            PolicyRuleBase rule = CreateNewRuleInstance();

            SOFilterPolicy filter = new SOFilterPolicy();
            filter.Level = mIsVersionFilter ? GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion : GCommon.Contract.CommonFilter.PolicyLevel.Document;
            filter.Rule = rule;
            filter.SequenceNo = mStartSequenceNo;
            filter.IsAnd = true;
            if (mCondition == TimeCondition.Before)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Before;
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(mValue1, mValueUnit);
            }
            else if (mCondition == TimeCondition.FromTo)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.FromTo;
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(mValue1, mValueUnit, mValue2, mValueUnit);
            }
            else if (mCondition == TimeCondition.OlderThan)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.OlderThan;
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(mValue1, mValueUnit);
            }
            res.Add(filter);
            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum TimeCondition
        {
            Before,
            OlderThan,
            FromTo,
        }

        public enum TimeRuleType
        {
            None,
            Modified,
            Created
        }
    }
}
