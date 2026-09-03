using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.CommonFilter;

namespace AvePoint.RA.SharePoint.Archiver.Common.RuleConverter.Filters
{
    internal class DO2SODocumentModifiedTimeForVersionFilter : DO2SOTimeBaseFilter
    {
        public DO2SODocumentModifiedTimeForVersionFilter(string value1, PolicyValueUnit valueUnit, int startSequenceNo, TimeCondition condition, bool isVersionFilter = true)
            : base(value1, valueUnit, startSequenceNo, condition, isVersionFilter)
        {
        }

        public DO2SODocumentModifiedTimeForVersionFilter(string value1, string value2, PolicyValueUnit valueUnit, int startSequenceNo, TimeCondition condition, bool isVersionFilter = true)
            : base(value1, value2, valueUnit, startSequenceNo, condition, isVersionFilter)
        {
        }

        public override TimeRuleType RuleType
        {
            get
            {
                return TimeRuleType.Modified;
            }
        }

        public override PolicyRuleBase CreateNewRuleInstance()
        {
            return new GCommon.Contract.CommonFilter.DocumentModifiedRule() { Value1 = "Document Modified Time" };
        }
    }
}
