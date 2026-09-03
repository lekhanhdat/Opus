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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Schedule.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.LicenseManager
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseNotificationPlan : PlanDto
    {
        [DataMember]
        public LicenseNotificationSetting ExtendSetting { set; get; }
    }

    /// <summary>
    /// GUI页面设置,如果页面有更改，只需修改这个类
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseNotificationSetting 
    {
        [DataMember]
        public ExpireSettingDto ByExpireDate { set; get; }
        [DataMember]
        public ExpireSettingDto ByMaintenanceExpireDate { set; get; }
        [DataMember]
        public ExpireSettingDto ByAgentsNum { set; get; }
        [DataMember]
        public bool PopupMsg { set; get; }
        [DataMember]
        public bool Email { set; get; }
        [DataMember]
        public NotificationDto Notification { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IntervalDto 
    {
        [DataMember]
        public bool HasInterval { set; get; }
        [DataMember]
        public int NotifyInterval { set; get; }
        [DataMember]
        public IntervalType NotifyIntervalType { set; get; }
    }

    [DataContract]
    public class RemindDto 
    {
        [DataMember]
        public int RemindMeFrom { set; get; }
        [DataMember]
        public IntervalType RemindIntervalType { set; get; }
    }

    [DataContract]
    public class ExpireSettingDto 
    {
        [DataMember]
        public RemindDto RemindDto { set; get; }
        [DataMember]
        public IntervalDto RemindInterval { set; get; }
    }
}
