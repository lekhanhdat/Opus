using AvePoint.GCommon.Contract.StorageOptimization.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters.DO2SOFileExtensionFilter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SOVersionNameFilter : DO2SOFilterConverterBase
    {
        private readonly List<string> mFileNames;
        private int mStartSequenceNo;
        private readonly VersionNameCondition mCondition;
        private string mAndOrString;

        public DO2SOVersionNameFilter(List<String> fileNames, int startSequenceNo, VersionNameCondition condition)
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
                filter.Level = GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion;
                filter.Rule = new GCommon.Contract.CommonFilter.NameRule()
                {
                    Value1 = "Name"
                };
                filter.SequenceNo = mStartSequenceNo;
                if (mCondition == VersionNameCondition.TextMatchIn)
                {
                    filter.IsAnd = false;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Match;
                }
                else if (mCondition == VersionNameCondition.TextMatchNotIn)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotMatch;
                }
                else if (mCondition == VersionNameCondition.Contains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Contains;
                }
                else if (mCondition == VersionNameCondition.NotContains)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.DoesNotContains;
                }
                else if (mCondition == VersionNameCondition.Equals)
                {
                    filter.IsAnd = true;
                    filter.Condition = GCommon.Contract.CommonFilter.PolicyCondition.Equals;
                }
                else if (mCondition == VersionNameCondition.NotEquals)
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

        public enum VersionNameCondition
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