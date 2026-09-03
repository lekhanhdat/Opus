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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;
using System.Collections.Generic;
namespace AvePoint.GCommon.Contract.Server.Common.TimeZone
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AveTimeZone
    {
        [DataMember]
        [XmlAttribute]
        public string Id { get; set; }
        [DataMember]
        [XmlAttribute]
        public string DisplayName { get; set; }
        [DataMember]
        [XmlAttribute]
        public string Zone { get; set; }
        [DataMember]
        [XmlAttribute]
        public TimeSpan BaseUtcOffset { get; set; }
        [DataMember]
        [XmlAttribute]
        public bool SupportsDaylightSavingTime { get; set; }
        [DataMember]
        [XmlAttribute]
        public long HashCode { get; set; }
        [DataMember]
        [XmlAttribute]
        public DateTimeKind Kind { get; set; }
        [DataMember]
        [XmlAttribute]
        public List<AdjustmentRule> AdjustmentRules { get; set; }

        public AdjustmentRule GetAdjustmentRuleForTime(DateTime dateTime)
        {
            if ((this.AdjustmentRules != null) && (this.AdjustmentRules.Count != 0))
            {
                DateTime date = dateTime.Date;
                for (int i = 0; i < this.AdjustmentRules.Count; i++)
                {
                    if ((this.AdjustmentRules[i].DateStart <= date) && (this.AdjustmentRules[i].DateEnd >= date))
                    {
                        return this.AdjustmentRules[i];
                    }
                }
            }
            return null;
        }

        public DateTimeKind GetCorrespondingKind()
        {
            return Kind;
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AdjustmentRule
    {
        [DataMember]
        [XmlAttribute]
        public DateTime DateEnd { get; set; }
        [DataMember]
        [XmlAttribute]
        public DateTime DateStart { get; set; }
        [DataMember]
        [XmlAttribute]
        public TimeSpan DaylightDelta { get; set; }
        [DataMember]
        [XmlAttribute]
        public TransitionTime DaylightTransitionEnd { get; set; }
        [DataMember]
        [XmlAttribute]
        public TransitionTime DaylightTransitionStart { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public struct DaylightTime
    {
        [DataMember]
        [XmlAttribute]
        public TimeSpan Delta { get; set; }
        [DataMember]
        [XmlAttribute]
        public DateTime End { get; set; }
        [DataMember]
        [XmlAttribute]
        public DateTime Start { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public struct TransitionTime
    {
        [DataMember]
        [XmlAttribute]
        public DateTime TimeOfDay { get; set; }
        [DataMember]
        [XmlAttribute]
        public int Month { get; set; }
        [DataMember]
        [XmlAttribute]
        public int Week { get; set; }
        [DataMember]
        [XmlAttribute]
        public int Day { get; set; }
        [DataMember]
        [XmlAttribute]
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsFixedDateRule { get; set; }
        public override bool Equals(object obj)
        {
            return ((obj is TransitionTime) && this.Equals((TransitionTime)obj));
        }

        public static bool operator ==(TransitionTime left, TransitionTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TransitionTime left, TransitionTime right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return (this.Month ^ (this.Week << 8));
        }
    }

}
