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



using System.Runtime.Serialization;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.FilterPolicy
{
    [DataContract]
    public class EDFilterPolicy
    {
        /// <summary>
        /// ÐòÁÐºÅ.
        /// </summary>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// RuleµÄÀàÐÍ.
        /// </summary>
        [DataMember]
        public EDRuleMetaDataType RuleType { get; set; }

        /// <summary>
        /// RuleµÄ×÷ÓÃ¼¶±ð.
        /// </summary>
        [DataMember]
        public EDRuleLevel RuleLevel { get; set; }

        /// <summary>
        /// Rule¶ÔÓ¦µÄValue
        /// </summary>
        [DataMember]
        public string RuleValue { get; set; }

        /// <summary>
        /// ConditionType
        /// </summary>
        [DataMember]
        public EDConditionType ConditionType { get; set; }

        /// <summary>
        /// ValueÀàÐÍ.
        /// </summary>
        [DataMember]
        public object Value { get; set; }

        /// <summary>
        /// ItemÖ®¼äµÄ¹ØÏµ.
        /// </summary>
        [DataMember]
        public EdAndOr AndOr { get; set; }

        /// <summary>
        /// RuleµÄÃû×Ö
        /// </summary>
        [DataMember]
        public EDRuleName RuleName { get; set; }

        /// <summary>
        /// SizeµÄµ¥Î»
        /// </summary>
        [DataMember]
        public EDSizeRangeType SizeType { get; set; }

        /// <summary>
        /// TimeµÄµ¥Î»
        /// </summary>
        [DataMember]
        public EDDateTimeRangeType TimeType { get; set; }

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
