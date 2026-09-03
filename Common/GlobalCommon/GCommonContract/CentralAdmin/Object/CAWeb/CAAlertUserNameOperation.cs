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
using AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAlertUserNameOperation : CAOperation
    {
        [DataMember]
        public List<User> Users { get; set; }
        [DataMember]
        public string NameToBeGetAlerts { get; set; }
        [DataMember]
        public List<Alert> Alers { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Alert
    {
        [DataMember]
        public bool IsChecked { get; set; } //Added for GUI.
        [DataMember]
        public string Index { get; set; }
        [DataMember]
        public AlertFrequency Frequency { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public AlertDeliveryChannels DeliveryMethod { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public List<CAUserInfo> Users { get; set; }
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string LoginName { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public List<string> SendAlertsForChanges { get; set; }
        [DataMember]
        public int CurSendAlertsForChange { get; set; }
        //[DataMember]
        //public string FilterIndex { get; set; }
        [DataMember]
        public Dictionary<string, string> Views { get; set; }
        [DataMember]
        public string CurView { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string SmsMessage { get; set; }
        [DataMember]
        public string MobileUrl { get; set; }
        [DataMember]
        public bool SendUrlInSms { get; set; }
        [DataMember]
        public int CurDay { get; set; }
        [DataMember]
        public int CurHour { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class User
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string LoginName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AlertDeliveryChannels
    {
        [EnumMember]
        None,
        [EnumMember]
        Email,
        [EnumMember]
        Sms
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum AlertFrequency
    {
        [EnumMember]
        Immediate,
        [EnumMember]
        Daily,
        [EnumMember]
        Weekly
    }
}
