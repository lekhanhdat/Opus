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


using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.Common;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.AveLicense.Detail;
using System;
namespace AvePoint.GCommon.Contract.AveLicense
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseNotificationSetting
    {
        [DataMember]
        public ExpirationSetting ByExpirationDate { get; set; }

        [DataMember]
        public ExpirationSetting ByMaintenanceExpirationDate { get; set; }

        [DataMember]
        public ServersSetting ByServers { get; set; }

        [DataMember]
        public UsersSetting ByUsers { get; set; }

        [DataMember]
        public bool PopupMsg { get; set; }

        [DataMember]
        public bool Email { get; set; }

        [DataMember]
        public string EmailNotificationId { get; set; }

    }

    [DataContract]
    public class ExpirationSetting
    {
        [DataMember]
        public DateTimeUnit Date { get; set; }

        [DataMember]
        public DateTimeUnit Interval { get; set; }

        [DataMember]
        public bool HasInterval { get; set; }

        [DataMember]
        public DateTime NextTime { get; set; }
    }

    [DataContract]
    public class ServersSetting
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public bool HasInterval { get; set; }

        [DataMember]
        public DateTimeUnit Interval { get; set; }

        [DataMember]
        public DateTime NextTime { get; set; }
    }

    [DataContract]
    public class DateTimeUnit
    {
        [DataMember]
        public int Value { get; set; }

        [DataMember]
        public DateType Type { get; set; }

        public TimeSpan ToTimeSpan()
        {
            TimeSpan span = new TimeSpan();
            switch (Type)
            {
                case DateType.Day:
                    span = new TimeSpan(Value, 0, 0, 0);
                    break;
                case DateType.Week:
                    span = new TimeSpan(Value * 7, 0, 0, 0);
                    break;
                case DateType.Month:
                    span = new TimeSpan(Value * 30, 0, 0, 0);
                    break;
            }
            return span;
        }
    }

    [DataContract]
    public enum DateType
    {
        [EnumMember]
        Day = 1,

        [EnumMember]
        Week = 2,

        [EnumMember]
        Month = 3,
    }

    [DataContract]
    public class UsersSetting
    {
        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public bool HasInterval { get; set; }

        [DataMember]
        public DateTimeUnit Interval { get; set; }

        [DataMember]
        public DateTime NextTime { get; set; }
    }
}
