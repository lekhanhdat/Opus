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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.Schedule
{
    [DataContract]
    public class ScheduleInfo
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public bool NoSchedule { get; set; }
        [DataMember]
        public string StartTime { get; set; }
        [DataMember]
        public string EndTime { get; set; }

        /// <summary>
        /// Next job start time
        /// </summary>
        [DataMember] 
        public DateTime NextTime { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public bool IsDaylightSaving { get; set; }
        /// <summary>
        /// End time type
        /// </summary>
        [DataMember]
        public EndType EndType { get; set; }
        [DataMember]
        public int OccurrencesTotal { get; set; }

        /// <summary>
        /// already occurrence time
        /// </summary>
        [DataMember]
        public int Occurrences { get; set; }
        [DataMember]
        public int Interval { get; set; }
        [DataMember]
        public IntervalType IntervalType { get; set; }
        [DataMember]
        public ScheduleType JobCategory { get; set; }
        [DataMember]
        public string ProfileId { get; set; }
        [DataMember]
        public string Extentions { get; set; }
        [DataMember]
        public bool DAOMigrated { get; set; }

        [DataMember]
        public int DayOfMonth { get; set; }

        [DataMember]
        public DayOfWeek WeekType { get; set; }

    }


    public class ManualApprovalStoreLocation
    {
        public string TenantId { get; set; }

        public string Url { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool PasswordEncrypted { get; set; }
        
    }
    public class SchedulePageInfo
    {
        public string StartTime { get; set; }
        public string Interval { get; set; }
        public string EndTime { get; set; }
    }

    //
    // Summary:
    //     Specifies the day of the week.
    public enum DayOfWeek
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }

    public class PayloadScheduleInfos
    {
        public List<ScheduleInfo> ScheduleInfo { get; set; }

    }

}
