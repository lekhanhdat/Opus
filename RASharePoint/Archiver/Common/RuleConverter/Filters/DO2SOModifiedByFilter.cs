using System;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOModifiedByFilter : DO2SOFilterConverterBase
    {
        private readonly List<string> mModifiedByNames;
        private int mStartSequenceNo;
        private readonly ModifiedByCondition mCondition;
        private string mAndOrString;
        private bool mIsVersionFilter;

        public DO2SOModifiedByFilter(List<String> modifiedByNames, int startSequenceNo, ModifiedByCondition condition, bool isVersionFilter)
        {
            this.mModifiedByNames = modifiedByNames;
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
            List<SOFilterPolicy> res = new List<SOFilterPolicy>();

            foreach (var modifiedName in mModifiedByNames)
            {
                SOFilterPolicy filter = new SOFilterPolicy();
                filter.Level = mIsVersionFilter?GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion: GCommon.Contract.CommonFilter.PolicyLevel.Document;
                filter.Rule = new GCommon.Contract.CommonFilter.ModifiedByRule()
                {
                    Value1 = "Modified by"
                };
                filter.SequenceNo = mStartSequenceNo;
                if (mCondition == ModifiedByCondition.Contains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Contains;
                }
                else if (mCondition == ModifiedByCondition.Equals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Equals;
                }
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(modifiedName);
                res.Add(filter);
                mStartSequenceNo++;
            }

            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum ModifiedByCondition
        {
            Contains,
            Equals
        }
    }
}
