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
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOFileExtensionFilter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOSizeFilter : DO2SOFilterConverterBase
    {
        private readonly string mValue1;
        private readonly PolicyValueUnit mValueUnit;
        private readonly int mStartSequenceNo;
        private readonly SizeCondition mCondition;
        private readonly bool mIsVersionFilter;
        private string mAndOrString;

        public override string AndOrString
        {
            get
            {
                return mAndOrString;
            }
        }

        public DO2SOSizeFilter(string value1, PolicyValueUnit valueUnit, int startSequenceNo, SizeCondition condition, bool isVersionFilter)
        {
            this.mValue1 = value1;
            this.mValueUnit = valueUnit;
            this.mStartSequenceNo = startSequenceNo;
            this.mCondition = condition;
            this.mIsVersionFilter = isVersionFilter;
        }
        public override List<SOFilterPolicy> Convert()
        {
            List<SOFilterPolicy> res = new List<SOFilterPolicy>();

            SOFilterPolicy filter = new SOFilterPolicy();
            filter.Level = mIsVersionFilter ? GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion : GCommon.Contract.CommonFilter.PolicyLevel.Document;
            filter.Rule = new GCommon.Contract.CommonFilter.SizeRule()
            {
                Value1 = mIsVersionFilter ? "Version Size" : "Document Size"
            };
            filter.SequenceNo = mStartSequenceNo;
            filter.IsAnd = true;
            if (mCondition == SizeCondition.LessOrEqualThan)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.LessOrEqualThan;
            }
            else if(mCondition == SizeCondition.GreaterOrEqualThan)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.GreaterOrEqualThan;
            }

            filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(mValue1, mValueUnit);
            res.Add(filter);
            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum SizeCondition
        {
            GreaterOrEqualThan,
            LessOrEqualThan,
        }
    }
}
