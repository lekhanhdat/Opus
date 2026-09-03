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
using System.Text;
using System.IO;
using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;
using AvePoint.GCommon.Contract.Common;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;

namespace AvePoint.GCommon.Contract.Server.Common.Notification.Object
{
    /// <summary>
    /// 用来封装发送提醒的所用参数的类
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NotificationMessage
    {
        /// <summary>
        /// job id
        /// </summary>
        [DataMember]
        public string JobId { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>
        /// 邮件主题
        /// </summary>
        [DataMember]
        public string Subject { get; set; }
        /// <summary>
        /// notificationDto
        /// </summary>
        [DataMember]
        public NotificationDto Notification { get; set; }
        /// <summary>
        /// NotificationSettingDto
        /// </summary>
        [DataMember]
        public NotificationSettingDto NotificationSetting { get; set; }
        /// <summary>
        /// jobDetailMap，用来存放邮件主体信息
        /// </summary>
        [DataMember]
        public Dictionary<string, string> JobDetailMap { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        [DataMember]
        public Stream Attachment { get; set; }
        /// <summary>
        /// 附件名
        /// </summary>
        [DataMember]
        public string AttachmentName { get; set; }
        /// <summary>
        /// 邮件类型，用来区分各个模块使用不同的模板
        /// </summary>
        [DataMember]
        public EmailTemplate EmailTemplate { get; set; }
        /// <summary>
        /// jobReport文件的格式
        /// </summary>
        [DataMember]
        public ReportFileType JobReportType { get; set; }
        /// <summary>
        /// 各自模块的job所使用的agent和media service的id值，用于收集log
        /// </summary>
        [DataMember]
        public List<string> JobUseServices { get; set; }
        [DataMember]
        public List<ImageDto> Images { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ImageDto
    {
        [DataMember]
        public string ContentId { get; set; }
        /// <summary>
        /// 图片Url地址（也可以是Uri）
        /// </summary>
        [DataMember]
        public string SourceUrl { get; set; }
        /// <summary>
        /// 图片左侧文本内容
        /// </summary>
        [DataMember]
        public string LeftText { get; set; }
        /// <summary>
        /// 图片右侧文本内容
        /// </summary>
        [DataMember]
        public string RightText { get; set; }
        /// <summary>
        /// 图片的宽（是0则显示图片默认宽和高）
        /// </summary>
        [DataMember]
        public double Width { get; set; }
        /// <summary>
        /// 图片的高（是0则显示图片默认宽和高）
        /// </summary>
        [DataMember]
        public double Height { get; set; }
        /// <summary>
        /// 图片位置
        /// </summary>
        [DataMember]
        public double Left { get; set; }
        /// <summary>
        /// 图片位置
        /// </summary>
        [DataMember]
        public double Top { get; set; }
    }
}
