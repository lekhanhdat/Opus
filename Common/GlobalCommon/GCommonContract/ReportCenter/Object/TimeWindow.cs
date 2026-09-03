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

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TimeWindow
    {
        [DataMember]
        public DateTimeOffset StartTime { get; set; }

        [DataMember]
        public DateTimeOffset EndTime { get; set; }

        [DataMember]
        public PeriodType PeriodType { get; set; }

        [DataMember]
        public FrequencyType Frequency { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        /// <summary>
        /// TimeZoneId 所在时区的当前Local时间
        /// </summary>
        [DataMember]
        public DateTime LocalNow { get; set; }

        public override string ToString()
        {
            return string.Format("StartTime:{0},EndTime:{1},PeriodType:{2},Frequency:{3}.", StartTime.ToString(), EndTime.ToString(), PeriodType.ToString(), Frequency.ToString());
        }
    }
}
