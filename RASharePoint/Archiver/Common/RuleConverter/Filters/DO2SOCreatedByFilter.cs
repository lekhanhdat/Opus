using System;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOCreatedByFilter : DO2SOFilterConverterBase
    {
        private readonly List<string> mCreatedByNames;
        private int mStartSequenceNo;
        private readonly CreatedByCondition mCondition;
        private string mAndOrString;

        public DO2SOCreatedByFilter(List<String> createdByNames, int startSequenceNo, CreatedByCondition condition)
        {
            this.mCreatedByNames = createdByNames;
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

            foreach (var createdbyName in mCreatedByNames)
            {
                SOFilterPolicy filter = new SOFilterPolicy();
                filter.Level = GCommon.Contract.CommonFilter.PolicyLevel.Document;
                filter.Rule = new GCommon.Contract.CommonFilter.CreatedByRule()
                {
                    Value1 = "Created by"
                };
                filter.SequenceNo = mStartSequenceNo;
                if (mCondition == CreatedByCondition.Contains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Contains;
                }
                else if (mCondition == CreatedByCondition.Equals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Equals;
                }
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(createdbyName);
                res.Add(filter);
                mStartSequenceNo++;
            }

            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum CreatedByCondition
        {
            Contains,
            Equals
        }
    }
}
