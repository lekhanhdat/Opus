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
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.CP
{
    public class JobNotificationDto
    {
        [DataMember]
        public int ProfileId { get; set; }

        [DataMember]
        public string ProfileName { get; set; }

        [DataMember]
        public string ProfileDes { get; set; }

        [DataMember]
        public List<ToUserInfo> ProfileEmailReceivers { get; set; }

        [DataMember]
        public NotificationInterval ProfileInterval { get; set; }

        [DataMember]
        public List<NotificationJobInfo> ProfileJobInfos { get; set; } = [];

        [DataMember]
        public string ProfileCreatedTime { get; set; }
    }

    public class EmailReceiver
    {
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string UserPrincipalName { get; set; }
    }

    public class NotificationInterval
    {
        [DataMember]
        [JsonProperty("intervalType")]
        public NotificationIntervalType IntervalType { get; set; }

        [DataMember]
        [JsonProperty("weeklyType")]
        public DayOfWeek WeeklyType { get; set; }
    }

    public class NotificationJobInfo
    {
        [DataMember]
        [JsonProperty("jobType")]
        public NotificationJobType JobType { get; set; }

        [DataMember]
        [JsonProperty("jobStatuses")]
        public List<JobStatus> JobStatuses { get; set; }
    }

    public enum NotificationIntervalType
    {
        None,
        Daily,
        Weekly,
    }

    public enum NotificationJobType
    {
        None,
        RMArchiverBackup,
        SOPreScan,
        EnforceRetention,
        ArchiverRestore,
        Discovery,
        DataSync,
        EnforceRuleAction,
        SyncNode,
        DashboardData,
        TermSync,
    }
}
