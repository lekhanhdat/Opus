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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public class ManualApprovalSettings
    {
        [DataMember]
        public ManualApprovalEmailNotificationSetting EmailNotificationSetting { get; set; } = new ManualApprovalEmailNotificationSetting();
        [DataMember]
        public ManualApprovalEscalationSetting EscalationSetting { get; set; } = new ManualApprovalEscalationSetting();
        [DataMember]
        public ManualApprovalDisposalExtentionSetting DisposalExtentionSetting { get; set; } = new ManualApprovalDisposalExtentionSetting();

    }
    [DataContract]
    public class ManualApprovalEmailNotificationSetting
    {
        [DataMember]
        public int Interval { get; set; } = 1;
        [DataMember]
        public ManualApprovalIntervalType IntervalType { get; set; } = ManualApprovalIntervalType.Days;
        [DataMember]
        public ManualApprovalEndType EndType { get; set; } = ManualApprovalEndType.EndOccurrences;
        [DataMember]
        public int OccurrencesTimes { get; set; } = 3;
        [DataMember]
        public ManualApprovalSettingType ManualApprovalSettingType { get; set; } = ManualApprovalSettingType.Interval;
        [DataMember]
        public List<ManualApprovalAdvanceNotificationSetting> AdvanceEmailSetting { get; set; } = new List<ManualApprovalAdvanceNotificationSetting>();
    }
    [DataContract]
    public class ManualApprovalAdvanceNotificationSetting
    {
        [DataMember]
        public int Interval { get; set; } = 1;
        [DataMember]
        public ManualApprovalIntervalType IntervalType { get; set; } = ManualApprovalIntervalType.Days;
        [DataMember]
        public int CurrentStep { get; set; } = 1;
    }


    public class ManualApprovalEscalationSetting
    {
        public ManualApprovalEscalateSettingType EscalateSettingType { get; set; } = ManualApprovalEscalateSettingType.NoAction;

        public SOApproveDBStatus ApprovalStatus { get; set; } = SOApproveDBStatus.Rejected;

        public List<ToUserInfo> ReassignUsers { get; set; } = new List<ToUserInfo>();
    }

    public class ManualApprovalDisposalExtentionSetting
    {
        public int MaxDelayTimes { get; set; } = 3;

        public ManualApprovalExtendType LatestExtendType { get; set; } = ManualApprovalExtendType.Month;

        public int LatestExtendNumber { get; set; } = 1;
    }

    public enum ManualApprovalEscalateSettingType
    {
        None = 0,
        WorkflowNextStep = 1,
        ReassignSpecificUsers = 2,
        NoAction = 3,
    }
    [DataContract]
    public enum ManualApprovalIntervalType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Days = 1,
        [EnumMember]
        Weeks = 2
    }
    [DataContract]
    public enum ManualApprovalEndType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        NoEnd = 1,
        [EnumMember]
        EndOccurrences = 2
    }
    [DataContract]
    public enum ManualApprovalSettingType 
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Interval = 1,
        [EnumMember]
        Advance = 2
    }
}
