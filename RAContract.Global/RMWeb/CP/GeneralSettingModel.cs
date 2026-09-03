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
using AvePoint.RA.Contract.CodeView;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.RMWeb.CP
{
    /// <summary>
    /// 时间格式
    /// </summary>
    public enum TimeFormat
    {
        h_mm_ss_tt = 0,
        h_mm_ss,
    }
    /// <summary>
    /// 日期格式
    /// </summary>
    public enum DateFormat
    {
        yyyy_MM_dd = 0,
        M_d_yyyy = 1,
        M_d_yy = 2,
        MM_dd_yy = 3,
        d_MMMM_yy = 4,
        MMMM_d_yyyy = 5,
        d_MMM_yyyy = 6,
        dd_MM_yyyy = 7
    }
    /// <summary>
    /// 时间单位
    /// </summary>
    public enum SessionTimeUnit
    {
        hours = 0,
        minutes,
    }
    /// <summary>
    /// 需要搜集的Audit信息
    /// </summary>
    public enum AuditItems
    {
        SessionTimeOut = 0,
        TimeZone = 1,
        DataFormat = 2,
        TimeFormat = 3,
        isSupportDaylight = 4,
        RecordsLabel = 5,
    }
    [DataContract]
    public enum EmailSenderType
    {
        [EnumMember]
        Default = 0,
        [EnumMember]
        O365 = 1,
    }
    [DataContract]
    public class EmailSender
    {
        [DataMember]
        public string DisplayName { get; set; }
        [IgnoreDataMember]
        public string DisplayName_Lower => DisplayName?.ToLower();
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string GivenName { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string RMUserId { get; set; }
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string UserPrincipalName { get; set; }
    }
    [DataContract]
    public class EmailSenderDefinition
    {
        [DataMember]
        public EmailSenderType EmailSenderType { get; set; }
        [DataMember]
        public string AppProfileId { get; set; }
        [DataMember]
        public EmailSender EmailSender { get; set; }
    }

    /// <summary>
    /// general seting页面表单项
    /// </summary>
    [RACodeReview("Allen Yin")]
    [DataContract]
    public class GeneralSettingModel
    {
        [DataMember]
        public int GeneralSetingId { get; set; }
        [DataMember] 
        public int SessionTime { get; set; } /// 单位:分钟或小时，根据 SessionTimeUnitId
        [DataMember]
        public int DataFormatId { get; set; }
        [DataMember]
        public int TimeFormatId { get; set; }
        [DataMember]
        public int SessionTimeUnitId { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        public bool DayLight { get; set; }// 是否启用夏令时
        [DataMember]
        public bool isShowDayLight { get; set; }// 此时区是否支持夏令时
        [DataMember]
        public string SecurityProfileId { get; set; }
        [DataMember]
        public string SecurityProfileName { get; set; }
        [DataMember]
        public EmailSenderDefinition EmailSenderDefinition { get; set; }
        [DataMember]
        public string RecordsLabel { get; set; }
        [IgnoreDataMember]
        public readonly static GeneralSettingModel DefaultSetting = new GeneralSettingModel
        {
            GeneralSetingId = 0,
            DataFormatId = (int)DateFormat.yyyy_MM_dd,
            TimeFormatId = (int)TimeFormat.h_mm_ss,
            SessionTime = 15,
            TimeZoneId = TimeZoneInfo.Local.Id,
            DayLight = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now),
            SessionTimeUnitId = (int)SessionTimeUnit.minutes,
            isShowDayLight = TimeZoneInfo.Local.SupportsDaylightSavingTime,
            EmailSenderDefinition = new EmailSenderDefinition 
            { 
                EmailSenderType = EmailSenderType.Default,
                AppProfileId = string.Empty,
                EmailSender = null,
            }
        };

    }
    /// <summary>
    /// 页面日历控件初始化参数
    /// </summary>
    public class TimeSettingModel
    {
        public List<TimeZoneMsg> TimeZoneInfo { get; set; }
        public string TimeZoneId { get; set; }
        public int offsetHours { get; set; }
        public int offsetMinutes { get; set; }
        public bool isSupportDayLight { get; set; }
        public bool isSetDayLight { get; set; }
        public int sessionTime { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
    }
    public class TimeZoneMsg
    {
        public string id { get; set; }
        public string displayName { get; set; }
        public string simplifyDisplayName { get; set; }
        public string zone { get; set; }
        public int offsetHours { get; set; }
        public int offsetMinutes { get; set; }
        public bool supportsDaylightSavingTime { get; set; }
        public bool autoAdjustClock { get; set; }
    }
    /// <summary>
    /// 经过时区转化后的时间对象
    /// </summary>
    public class TimeModel
    {
        public string FormaTime { get; set; }
        public string FormaDate { get; set; }
        public DateTime DataTime { get; set; }
        public string SimplifyFormatTime { get; set; }
    }
}
