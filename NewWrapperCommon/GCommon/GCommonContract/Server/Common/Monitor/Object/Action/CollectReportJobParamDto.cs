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

using AvePoint.Common.Module.JobMonitor.Entities;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.ManagedAccount.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.SystemSetting.Object;
using AvePoint.GCommon.Contract.Server.Job.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CollectReportJobParamDto : BaseJobParamDto, ISystemSettingContent
    {
        [DataMember]
        public List<BaseJobDto> JobDtos { set; get; }

        [DataMember]
        public CollectReportSettingDto Setting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CollectReportSettingDto
    {
        [DataMember]
        public bool ScheduleDefined { get; set; }

        //[DataMember]
        //public List<ScheduleDto> ScheduleDtos { get; set; }

        [DataMember]
        public string UNCPath { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }
        
        [DataMember]
        public AccountProfileDto AccountProfile { get; set; }

        [DataMember]
        public List<JobReportDetailStatus> Filters { get; set; }

        [DataMember]
        public ReportFileType FileType { get; set; }

        [DataMember]
        public TimeZoneType ZoneType { get; set; }

        [DataMember]
        public ProfileDto Profile { get; set; }

        [DataMember]
        public NotificationDto Notification { get; set; }
    }
}
