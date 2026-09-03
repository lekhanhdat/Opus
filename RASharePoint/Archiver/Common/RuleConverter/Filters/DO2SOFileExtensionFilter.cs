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

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOFileExtensionFilter : DO2SOFilterConverterBase
    {
        private readonly List<string> mFileExtensions;
        private int mStartSequenceNo;
        private readonly FileExtensionCondition mCondition;
        private readonly bool mIsVersionFilter;
        private string mAndOrString;

        public DO2SOFileExtensionFilter(List<String> fileExtensions, int startSequenceNo, FileExtensionCondition condition, bool isVersionFilter)
        {
            this.mFileExtensions = fileExtensions;
            this.mStartSequenceNo = startSequenceNo;
            this.mCondition = condition;
            this.mIsVersionFilter = isVersionFilter;
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
            List<SOFilterPolicy> res= new List<SOFilterPolicy>();


            if (mCondition == FileExtensionCondition.IsEmpty)
            {
                SOFilterPolicy filter = new SOFilterPolicy();
                filter.Level = mIsVersionFilter ? GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion : GCommon.Contract.CommonFilter.PolicyLevel.Document;
                filter.Rule = new GCommon.Contract.CommonFilter.NameRule()
                {
                    Value1 = "Name"
                };
                filter.SequenceNo = mStartSequenceNo;
                filter.IsAnd = true;
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotContains;
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(".");
                res.Add(filter);
            }
            else if (mCondition == FileExtensionCondition.IsNotEmpty)
            {
                SOFilterPolicy filter = new SOFilterPolicy();
                filter.Level = mIsVersionFilter ? GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion : GCommon.Contract.CommonFilter.PolicyLevel.Document;
                filter.Rule = new GCommon.Contract.CommonFilter.NameRule()
                {
                    Value1 = "Name"
                };
                filter.SequenceNo = mStartSequenceNo;
                filter.IsAnd = true;
                filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Contains;
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(".");
                res.Add(filter);
            }
            else
            {
                foreach (var fileExtension in mFileExtensions)
                {
                    SOFilterPolicy filter = new SOFilterPolicy();
                    filter.Level = mIsVersionFilter ? GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion : GCommon.Contract.CommonFilter.PolicyLevel.Document;
                    filter.Rule = new GCommon.Contract.CommonFilter.NameRule()
                    {
                        Value1 = "Name"
                    };
                    filter.SequenceNo = mStartSequenceNo;
                    if (mCondition == FileExtensionCondition.In)
                    {
                        filter.IsAnd = false;
                        filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Match;
                    }
                    else if(mCondition == FileExtensionCondition.NotIn)
                    {
                        filter.IsAnd = true;
                        filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotMatch;
                    }
                    if (fileExtension == "RM_FA_FileType_Empty")
                    {
                        filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotContains;
                        filter.Value = new GCommon.Contract.CommonFilter.PolicyValue($".");
                    }
                    else
                    {
                        filter.Value = new GCommon.Contract.CommonFilter.PolicyValue($"*.{fileExtension}");
                    }
                    res.Add(filter);
                    mStartSequenceNo++;
                }
            }
            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum FileExtensionCondition
        {
            In,
            NotIn,
            IsEmpty,
            IsNotEmpty,
        }
    }
}
