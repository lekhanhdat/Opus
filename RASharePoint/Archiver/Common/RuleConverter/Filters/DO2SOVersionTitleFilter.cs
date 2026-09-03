using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOVersionTitleFilter : DO2SOFilterConverterBase
    {
        private readonly List<string> mVersionTitles;
        private int mStartSequenceNo;
        private readonly VersionTitleCondition mCondition;
        private string mAndOrString;

        public DO2SOVersionTitleFilter(List<String> versionTitles, int startSequenceNo, VersionTitleCondition condition)
        {
            this.mVersionTitles = versionTitles;
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

            foreach (var versionTitle in mVersionTitles)
            {
                SOFilterPolicy filter = new SOFilterPolicy();
                filter.Level = GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion;
                filter.Rule = new GCommon.Contract.CommonFilter.TitleRule()
                {
                    Value1 = "Title"
                };
                filter.SequenceNo = mStartSequenceNo;
                if (mCondition == VersionTitleCondition.TextMatchIn)
                {
                    filter.IsAnd = false;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Match;
                }
                else if (mCondition == VersionTitleCondition.TextMatchNotIn)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotMatch;
                }
                else if (mCondition == VersionTitleCondition.Contains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Contains;
                }
                else if (mCondition == VersionTitleCondition.NotContains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotContains;
                }
                else if (mCondition == VersionTitleCondition.Equals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Equals;
                }
                else if (mCondition == VersionTitleCondition.NotEquals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotEquals;
                }
                filter.Value = new GCommon.Contract.CommonFilter.PolicyValue(versionTitle);
                res.Add(filter);
                mStartSequenceNo++;
            }

            mAndOrString = GetSOFilterExpression(res);
            return res;
        }

        public enum VersionTitleCondition
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
