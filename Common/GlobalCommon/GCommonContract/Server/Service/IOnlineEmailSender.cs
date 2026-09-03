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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.EmailTemplateSettings.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Reporting.UserNotificationSettings.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.Service
{
    public interface IOnlineEmailSender
    {
        /// <summary>
        /// 同步retry发送邮件
        /// </summary>
        /// <param name="emailDto"></param>
        void SendSyncEmail(EmailMessageDto emailDto, NotificationSettingDto settingDto = null);

        /// <summary>
        /// 同步retry 发多封邮件，目前主要是满足job report的需求
        /// </summary>
        /// <param name="msgs"></param>
        /// <param name="settingDto"></param>
        void SendSyncEmails(List<EmailMessageDto> msgs, NotificationSettingDto settingDto = null);

        /// <summary>
        /// 异步retry发送邮件
        /// </summary>
        /// <param name="emailDto"></param>
        void SendAsyncEmail(EmailMessageDto emailDto, NotificationSettingDto settingDto = null);

        /// <summary>
        /// 异步retry发送邮件使用用户自定义template 不自动设置banner和copyright
        /// </summary>
        /// <param name="emailDto"></param>
        void SendSyncEmailByTemplate(EmailMessageDto emailDto, EmailTemplateDto template, NotificationSettingDto settingDto = null);
    }
}
