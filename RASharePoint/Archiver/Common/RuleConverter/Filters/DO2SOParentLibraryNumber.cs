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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.CommonFilter.Rules;


namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOParentLibraryNumber : DO2SOFilterConverterBase
    {
        private readonly string mValue1;
        private readonly int mStartSequenceNo;
        private readonly NumberConditionType mCondition;
        private string mAndOrString;
        private string mFilterName;

        public override string AndOrString
        {
            get
            {
                return mAndOrString;
            }
        }

        public DO2SOParentLibraryNumber(string value1, int startSequenceNo, NumberConditionType condition, string extraValue)
        {
            this.mValue1 = value1;
            this.mStartSequenceNo = startSequenceNo;
            this.mCondition = condition;
            this.mFilterName = extraValue;
        }

        public override List<SOFilterPolicy> Convert()
        {
            List<SOFilterPolicy> res = new List<SOFilterPolicy>();

            ParentLibraryNumberRule rule = new ParentLibraryNumberRule { Value1 = this.mFilterName };
            SOFilterPolicy filter = new SOFilterPolicy();
            filter.Level = GCommon.Contract.CommonFilter.PolicyLevel.Document;
            filter.Rule = rule;
            filter.SequenceNo = mStartSequenceNo;
            filter.IsAnd = true;
            filter.Condition = mCondition switch
            {
                NumberConditionType.LessThanEquals => PolicyCondition.LessOrEqualThan,
                NumberConditionType.GreaterThanEquals => PolicyCondition.GreaterOrEqualThan,
                _ => PolicyCondition.None,
            };
            filter.Value = new GCommon.Contract.CommonFilter.PolicyValue()
            {
                Value1 = this.mValue1,
            };
            res.Add(filter);
            mAndOrString = GetSOFilterExpression(res);
            return res;
        }
    }
}
