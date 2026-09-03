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
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOFileNameFilter;
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOTimeBaseFilter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOKeepLastVersionFilter : DO2SOFilterConverterBase
    {
        private readonly string mValue1;
        private readonly int mStartSequenceNo;
        private readonly KeepLastVersionCondition mCondition;
        private string mAndOrString;

        public DO2SOKeepLastVersionFilter(string value1, int startSequenceNo, KeepLastVersionCondition condition)
        {
            this.mValue1 = value1;
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

            SOFilterPolicy filter = new SOFilterPolicy();
            filter.Level = GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion;
            filter.Rule = new GCommon.Contract.CommonFilter.KeepHistoryVersionRule()
            {
                Value1 = "Keep the Latest Version"
            };
            filter.SequenceNo = mStartSequenceNo;
            filter.IsAnd = true;
            if (mCondition == KeepLastVersionCondition.MajorAndMintorVersions)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.MajorAndMintorVersions;
            }
            else if(mCondition == KeepLastVersionCondition.MajorWithoutMinorVersions)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.MajorWithoutMinorVersions;
            }
            else if (mCondition == KeepLastVersionCondition.MinorOfEachMajorVersion)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.MinorOfEachMajorVersion;
            }
            else if (mCondition == KeepLastVersionCondition.MinorOfTheLatestMajorVersion)
            {
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.MinorOfTheLatestMajorVersion;
            }
            filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(mValue1);
            res.Add(filter);
            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum KeepLastVersionCondition
        {
            MajorAndMintorVersions,
            MajorWithoutMinorVersions,
            MinorOfEachMajorVersion,
            MinorOfTheLatestMajorVersion
        }
    }
}
