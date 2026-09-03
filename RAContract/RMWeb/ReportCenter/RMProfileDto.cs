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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Schedule;
using System;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.ReportCenter
{
    [DataContract]
    public class RMProfileDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string ProfileName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Extension1 { get; set; }
        [DataMember]
        public string Extension2 { get; set; }
        [DataMember]
        public JobType Type { get; set; }
        [DataMember]
        public bool isChecked { get; set; }
        [DataMember]
        public string Modified { get; set; }
        [DataMember]
        public bool IsCreated { get; set; }
        [DataMember]
        public bool IsDestoryed { get; set; }
        [DataMember]
        public TimeRangeType RangeType { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }
        [DataMember]
        public string CreateProfileUserId { get; set; }
        [DataMember]
        public SourceFlag Source { get; set; }

        [DataMember]
        public string ScheduleId { get; set; }
        [DataMember]
        public ScheduleInfo scheduleInfo { get; set; }
        [DataMember]
        public string Extension3 { get; set; }
        [DataMember]
        public int? ObjectLevel { get; set; }
    }

}
