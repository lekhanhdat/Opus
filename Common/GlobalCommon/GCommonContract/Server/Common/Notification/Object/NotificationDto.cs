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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.Common
{
    /// <summary>
    /// 用存储各个模块的Notification的设置
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NotificationDto : IProfileContent
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember]
        public string Id { get; set; }
        /// <summary>
        /// 用来标识内容的格式（Html or Text）
        /// </summary>
        [DataMember]
        public ContentType ContentType { get; set; }
        /// <summary>
        /// 发送提醒的对象
        /// </summary>
        [DataMember]
        public List<UserSetting> UserSettings { get; set; }
        /// <summary>
        /// 发送log与否
        /// </summary>
        [DataMember]
        public SendLog SendAllLog { get; set; }
        /// <summary>
        /// 发送log与否
        /// </summary>
        [DataMember]
        public ReportLevel SendAllLogLevels { get; set; }
        /// <summary>
        /// Summary Report的级别
        /// </summary>
        [DataMember]
        public ReportLevel SummaryReportLevels { get; set; }
        /// <summary>
        /// Detail Report的级别
        /// </summary>
        [DataMember]
        public ReportLevel DetailReprotLevels { get; set; }
        /// <summary>
        /// 邮件重要性
        /// </summary>
        [DataMember]
        public MailPriority Priority { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserSetting
    {
        /// <summary>
        /// 邮件的Report
        /// </summary>
        [DataMember]
        public ReportRecipient Report { get; set; }
        /// <summary>
        /// notification的类型
        /// </summary>
        [DataMember]
        public SendType SendType { get; set; }
        /// <summary>
        /// 通知的接受者
        /// </summary>
        [DataMember]
        public string Receivers { get; set; }
    }
    /// <summary>
    /// send type
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SendType : int
    {
        [EnumMember]
        EmailAddress = 0,
        [EnumMember]
        ExchangeUser = 1,
        [EnumMember]
        SharePointUser = 2,
        [EnumMember]
        SMS = 3,
        [EnumMember]
        DocAveGroup = 4,
    }
    /// <summary>
    /// send log
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SendLog : int
    {
        [EnumMember]
        NotSendLog = 0,
        [EnumMember]
        SendAllLog = 1,
    }
    /// <summary>
    /// reprot recipient
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportRecipient : int
    {
        [EnumMember]
        SummaryReport = 0,
        [EnumMember]
        DetailedReport = 1,
        [EnumMember]
        SummaryAndDetailed = 2,
        [EnumMember]
        AlertReport = 3,
    }
    /// <summary>
    /// 发送提醒的级别
    /// </summary>
    [Flags, DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportLevel : int
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Success = 1 << 0,
        [EnumMember]
        Failure = 1 << 1,
        [EnumMember]
        Waring = 1 << 2,
    }
    /// <summary>
    /// notification内容的格式
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentType : int
    {
        [EnumMember]
        Html = 0,
        [EnumMember]
        Text = 1,
    }
    /// <summary>
    /// notification重要性
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum MailPriority
    {
        /// <summary>
        /// The email has normal priority.
        /// </summary>
        [EnumMember]
        Normal = 0,
        /// <summary>
        /// The email has low priority.
        /// </summary>
        [EnumMember]
        Low = 1,
        /// <summary>
        /// The email has high priority.
        /// </summary>
        [EnumMember]
        High = 2,
    }
}
