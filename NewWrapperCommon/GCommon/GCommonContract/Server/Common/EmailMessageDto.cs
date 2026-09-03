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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.Adonis.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Server.Common.Notification.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [KnownType(typeof(Dictionary<RCEmailCellDto, List<List<RCEmailCellDto>>>))]
    [KnownType(typeof(RCEmailCellDto))]
    public class EmailMessageDto
    {
        /// <summary>
        /// Notification Profile Id
        /// </summary>
        [DataMember]
        public string NotificationProfileId { get; set; }
        /// <summary>
        /// Job Id
        /// </summary>
        [DataMember]
        public string JobId { get; set; }
        /// <summary>
        /// 收信人地址，有多个收信人用';'分割
        /// </summary>
        [DataMember]
        public string Receivers { get; set; }
        /// <summary>
        /// CC收信人地址，有多个收信人用';'分割
        /// </summary>
        [DataMember]
        public string CcReceivers { get; set; }
        /// <summary>
        /// BCC收信人地址，有多个收信人用';'分割
        /// </summary>
        [DataMember]
        public string BccReceivers { get; set; }
        /// <summary>
        /// 邮件主题
        /// </summary>
        [DataMember]
        public string Subject { get; set; }
        /// <summary>
        /// 邮件内容
        /// </summary>
        [DataMember]
        public string Body { get; set; }
        /// <summary>
        /// 邮件标题
        /// </summary>
        [DataMember]
        public string Title { get; set; }
        /// <summary>
        /// 邮件内容
        /// </summary>
        [DataMember]
        public Dictionary<string, Object> DetailMap { get; set; }
        /// <summary>
        /// 邮件类型，用来区分各个模块使用不同的模板
        /// </summary>
        [DataMember]
        public EmailTemplate EmailTemplate { get; set; }
        /// <summary>
        /// 附件
        /// </summary>
        [DataMember]
        public byte[] Attachment { get; set; }
        /// <summary>
        /// 附件名
        /// </summary>
        [DataMember]
        public string AttachmentName { get; set; }
        [DataMember]
        public List<ImageDto> Images { get; set; }
        /// <summary>
        /// 用来标识内容的格式（Html or Text）
        /// </summary>
        [DataMember]
        public ContentType ContentType { get; set; }
        [DataMember]
        public List<string> JobUseServices { get; set; }

        [DataMember]
        public NotificationSettingDto NotificationSetting { get; set; }
        /// <summary>
        /// jobReport文件的格式
        /// </summary>
        [DataMember]
        public ReportFileType JobReportType { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum EmailTemplate
    {
        [EnumMember]
        TemplateEmailNotificationTest = 0,
        [EnumMember]
        TemplateEmailNotificationHtml,
        [EnumMember]
        TemplateLicenseExpireRemindHtml,
        [EnumMember]
        TemplateReportCenterEmailNotificationHtml,
        [EnumMember]
        TemplateSRPruningReport,
        [EnumMember]
        TemplateDPPruningEmailNotification,
        [EnumMember]
        TemplatePatchReportEmailNotificationHtml,
        [EnumMember]
        TemplateEIEmailNotification,
        [EnumMember]
        TemplateRPDetailEmailNotification,
        [EnumMember]
        TemplateRPHealthCheckEmailNotification,
        [EnumMember]
        TemplateDPEndUserRestoreEmailNotification,
        [EnumMember]
        TemplatePatchDownloadReportEmailNotificationHtml,
        [EnumMember]
        TemplateJobPerformanceAlertEmailNotification,
        [EnumMember]
        TemplatePEEmailNotificationHtml,
        [EnumMember]
        TemplateCATempPermissionEmailNotificationHtml,
    }
}
