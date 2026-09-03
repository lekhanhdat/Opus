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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOFileExtensionFilter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOFileNameFilter : DO2SOFilterConverterBase
    {
        private readonly List<string> mFileNames;
        private int mStartSequenceNo;
        private readonly FileNameCondition mCondition;
        private string mAndOrString;

        public DO2SOFileNameFilter(List<String> fileNames, int startSequenceNo, FileNameCondition condition)
        {
            this.mFileNames = fileNames;
            this.mStartSequenceNo = startSequenceNo;
            this.mCondition = condition;
        }

        public override string AndOrString
        {
            get
            {
                return mAndOrString;
            }
        }

        public override List<SOFilterPolicy> Convert()
        {
            List<SOFilterPolicy> res = new List<SOFilterPolicy>();

            foreach (var fileName in mFileNames)
            {
                SOFilterPolicy filter = new SOFilterPolicy();
                filter.Level = GCommon.Contract.CommonFilter.PolicyLevel.Document;
                filter.Rule = new GCommon.Contract.CommonFilter.NameRule()
                {
                    Value1 = "Name"
                };
                filter.SequenceNo = mStartSequenceNo;
                if (mCondition == FileNameCondition.TextMatchIn)
                {
                    filter.IsAnd = false;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Match;
                }
                else if (mCondition == FileNameCondition.TextMatchNotIn)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotMatch;
                }
                else if (mCondition == FileNameCondition.Contains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Contains;
                }
                else if (mCondition == FileNameCondition.NotContains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotContains;
                }
                else if (mCondition == FileNameCondition.Equals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Equals;
                }
                else if (mCondition == FileNameCondition.NotEquals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotEquals;
                }
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(fileName);
                res.Add(filter);
                mStartSequenceNo++;
            }

            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum FileNameCondition
        {
            TextMatchIn,
            TextMatchNotIn,
            Contains,
            NotContains,
            Equals,
            NotEquals
        }
    }
}
