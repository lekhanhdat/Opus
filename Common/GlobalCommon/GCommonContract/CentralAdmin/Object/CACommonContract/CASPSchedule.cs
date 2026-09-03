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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{

    [KnownType(typeof(CASPWeeklySchedule))]
    [KnownType(typeof(CASPMonthlySchedule))]
    [KnownType(typeof(CASPMonthlyByDaySchedule))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class CASPSchedule
    {

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASPMinuteSchedule : CASPSchedule
    {
 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASPHourlySchedule : CASPSchedule
    {
 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASPDailySchedule : CASPSchedule
    {
        [DataMember]
        public int BeginHour { get; set; }

        [DataMember]
        public int EndHour { get; set; }

        [DataMember]
        public int BeginMinute { get; set; }

        [DataMember]
        public int EndMinute { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASPWeeklySchedule : CASPDailySchedule
    {
        [DataMember]
        public DayOfWeek BeginDayOfWeek { get; set; }

        [DataMember]
        public DayOfWeek EndDayOfWeek { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASPMonthlySchedule : CASPDailySchedule
    {
        [DataMember]
        public int BeginDay { get; set; }

        [DataMember]
        public int EndDay { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASPMonthlyByDaySchedule : CASPDailySchedule
    {
        [DataMember]
        public CASPWeekOfMonth BeginWeek { get; set; }

        [DataMember]
        public DayOfWeek BeginDay { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CASPWeekOfMonth
    {
        [EnumMember]
        First,
        [EnumMember]
        Second,
        [EnumMember]
        Third,
        [EnumMember]
        Fourth,
        [EnumMember]
        Last
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CASPScheduleType
    {
        [EnumMember]
        Daily = 2,
        [EnumMember]
        Hourly = 1,
        [EnumMember]
        Minutely = 0,
        [EnumMember]
        Monthly = 4,
        [EnumMember]
        None = -1,
        [EnumMember]
        Weekly = 3
    }

}
