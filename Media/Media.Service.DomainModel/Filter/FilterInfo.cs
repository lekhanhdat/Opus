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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using AvePoint.GCommon.Contract.CommonFilter;
    using global::Media.Common;
    using System;
    using System.Text;

    #endregion

    public class FilterInfo : ICloneable
    {
        public Int32 SequenceNo { get; set; }
        public String Criteria { get; set; }
        public String Criteria1 { get; set; }
        public String Criteria2 { get; set; }
        public PolicyValueUnit Value1Unit { get; set; }
        public FilterLevel Level { get; set; }
        public FilterRuleType RuleType { get; set; }
        public FilterCondition Condition { get; set; }
        public Boolean IncludePartialData { get; set; }
        public long StartTime { get; set; }
        public long EndTime { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("FilterInfo: ");
            sb.Append(SequenceNo.ToString());
            sb.Append(" ");
            sb.Append(Level.ToString());
            sb.Append(" ");
            sb.Append(RuleType.ToString());
            sb.Append(" ");
            sb.Append(Condition.ToString());
            sb.Append(" ");
            sb.Append(Criteria);
            sb.Append(" ");
            sb.Append(Criteria1);
            sb.Append(" ");
            sb.Append(Criteria2);
            return sb.ToString();
        }

        public static FilterInfo FromFilterPolicy(FilterPolicy filter)
        {
            return new FilterInfo()
            {
                //Condition = EnumConverter.ToEnum<FilterCondition>(filter.Condition.ToString()),
                RuleType = EnumConverter.ToEnum<FilterRuleType>(filter.RuleType.ToString()),
                Level = EnumConverter.ToEnum<FilterLevel>(filter.Level.ToString()),
                Criteria = filter.Rule.Value1,
                Criteria1 = filter.Value != null ? filter.Value.Value1 : null,
                Criteria2 = filter.Value != null ? filter.Value.Value2 : null,
            };
        }

        public object Clone()
        {
            return new FilterInfo()
            {
                SequenceNo = this.SequenceNo,
                Criteria = this.Criteria,
                Criteria1 = this.Criteria1,
                Criteria2 = this.Criteria2,
                Level = this.Level,
                RuleType = this.RuleType,
                Condition = this.Condition,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
            };
        }
    }
}