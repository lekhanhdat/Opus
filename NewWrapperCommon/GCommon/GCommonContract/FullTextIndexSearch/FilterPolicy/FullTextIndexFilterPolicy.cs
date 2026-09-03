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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.FullTextIndexSearch.FilterPolicy
{
    [DataContract]
    public class FullTextIndexFilterPolicy
    {
        /// <summary>
        /// 序列号.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Rule的类型.
        /// </summary>
        [DataMember]
        public FullTextIndexRuleMetaDataType RuleType { get; set; }

        /// <summary>
        /// Rule的作用级别.
        /// </summary>
        [DataMember]
        public FullTextIndexRuleLevel RuleLevel { get; set; }

        /// <summary>
        /// Rule对应的Value
        /// </summary>
        [DataMember]
        public string RuleValue { get; set; }

        /// <summary>
        /// ConditionType
        /// </summary>
        [DataMember]
        public FullTextIndexConditionType ConditionType { get; set; }

        /// <summary>
        /// Value类型.
        /// </summary>
        [DataMember]
        public object Value { get; set; }

        /// <summary>
        /// Item之间的关系.
        /// </summary>
        [DataMember]
        public FullTextIndexAndOr AndOr { get; set; }

        /// <summary>
        /// Rule的名字
        /// </summary>
        [DataMember]
        public FullTextIndexRuleName RuleName { get; set; }

        /// <summary>
        /// Size的单位
        /// </summary>
        [DataMember]
        public FullTextIndexSizeRangeType SizeType { get; set; }

        /// <summary>
        /// Time的单位
        /// </summary>
        [DataMember]
        public FullTextIndexDateTimeRangeType TimeType { get; set; }

        [DataMember]
        public PolicyDateTime policyDateTime { get; set; }

    }

    [DataContract]
    public class PolicyDateTime
    {
        [DataMember]
        public long Time { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public bool AutoDST { get; set; }
    }
}
