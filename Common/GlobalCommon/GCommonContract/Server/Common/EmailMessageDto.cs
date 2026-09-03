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
using System.Text;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.GCommon.Contract.Server.Common.Notification.Object;
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Action;
using AvePoint.GCommon.Contract.Gateway.Object;

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
        ///是否包含附件
        /// </summary>
        [DataMember]
        public bool HasAttachment { get; set; }

        /// <summary>
        /// 是否是log email,有枚举好点，暂时还没需求
        /// </summary>
        [DataMember]
        public bool IsLogEmail { get; set; }
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
        public string ModuleName { get; set; }

        [DataMember]
        public string ErrorMsg { get; set; }
        /// <summary>
        /// jobReport文件的格式
        /// </summary>
        [DataMember]
        public ReportFileType JobReportType { get; set; }

        [DataMember]
        public ModuleCategory ModuleCategory { get; set; }

        [DataMember]
        public bool IsNewTemplate { get; set; }

        [DataMember]
        public string RequestReviewerFirstName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ModuleCategory
    {
        [EnumMember]
        Classic = 0, // common, granular, exchange online, identity manager

        [EnumMember]
        CloudManagement = 1, // administrator, content manager, deployment manager, replicator, report center

        [EnumMember]
        CloudArchiving = 2 // archiver
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EmailMessageForPortal
    {
        [DataMember]
        public List<string> Receivers { get; set; }
        [DataMember]
        public List<string> CcReceivers { get; set; }
        [DataMember]
        public List<string> BccReceivers { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public AccountLanguageType Language { get; set; }

        [DataMember]
        public LicenseType LicenseType { get; set; }

        [DataMember]
        public string GroupId { get; set; }
        public string Receiver
        {
            get
            {
                return GetListString(Receivers);
            }
        }
        public string CcReceiver
        {
            get
            {
                return GetListString(CcReceivers);
            }
        }
        public string BccReceiver
        {
            get
            {
                return GetListString(BccReceivers);
            }
        }
        private string GetListString(List<string> list)
        {
            if (list == null || list.Count == 0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append(item);
                sb.Append(";");
            }
            if (sb.Length > 0)
            {
                sb = sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }
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
        TemplateOnlineInviteUserNotification,
        [EnumMember]
        TemplateOnlineRegisterUserNotification,
        [EnumMember]
        TemplateOnlineExpireWarnNotification,
        [EnumMember]
        TemplateOnlinePurchaseNotification,
        [EnumMember]
        TemplateOnlineInviteSupportNotification,
        [EnumMember]
        TemplateOnlineSupportNoteNotification,
        [EnumMember]
        TemplateOnlineTenantDBQuotaAlertNotification,
        [EnumMember]
        TemplateOnlineTenantDBQuotaWarningNotification,
        [EnumMember]
        TemplateEmailFileInformationNotification,
        [EnumMember]
        TemplateOnlineBackupStorageLimitAlert,
        [EnumMember]
        TemplateOnlineBackupStorageLimitWarning,
        [EnumMember]
        TemplateOnlineGlobalStorageChangedNotification,
        [EnumMember]
        TemplateCATempPermissionEmailNotificationHtml,
        [EnumMember]
        TemplateOnlineExtendStorageChangedNotification,
        [EnumMember]
        TemplateConflictEmailNotificationHtml,
        [EnumMember]
        TemplateOnlineRunningJobNotification,
        [EnumMember]
        TemplateExchangeDataCompletenessCheck,
        [EnumMember]
        TemplateOnlineSupportNoteWithoutSupportAccountNotification,
        [EnumMember]
        TemplateOnlineInviteSupportWithoutSupportAccountNotification,
    }
}
