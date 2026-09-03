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


using System.Collections.Generic;
using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Gateway.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.UserRegister;

namespace AvePoint.GCommon.Contract.Server.Service
{
    public interface IOnlineEmailService
    {
        bool SendRegisterEmail(UserRegisterDto accountInfo, string registerURI);
        bool SendInviteEmail(AccountDto invitee, AccountDto inviter, EmailArg emailArg);
        bool SendPurchaseEmail(AccountDto user);
        bool SendExpirationWarnEmail(EmailMessageForPortal info, WarnState state);
        bool SendSupportNoteEmail(OnlineUserDataDto onlineData, AccountDto support, EmailArg emailArg);
        bool SendInviteSupportEmail(AccountDto inviter, AccountDto support, EmailArg emailArg, string ccReceivers, bool isDisableTemporaryAccount = false, bool isManageByPartner = false);
        /// <summary>
        ///  用户Report Database 容量达到最大限度的100%时，发送
        /// </summary>
        bool SendTenantDBQuotaAlertEmail(List<EmailMessageForPortal> infos);
        /// <summary>
        /// 用户Report Database 容量达到最大限度的80%时，发送
        /// </summary>
        bool SendTenantDBQuotaWarningEmail(List<EmailMessageForPortal> infos);

        /// <summary>
        ///  用户Report Database, Control  Database, PE Database 容量达到最大限度的100%时，发送
        /// </summary>
        bool SendTenantDBQuotaAlertEmail(List<EmailMessageForPortal> infos, DBType dbType);
        /// <summary>
        /// 用户Report Database, Control  Database, PE Database 容量达到最大限度的80%时，发送
        /// </summary>
        bool SendTenantDBQuotaWarningEmail(List<EmailMessageForPortal> infos, DBType dbType);
        /// <summary>
        /// 跑job前发现用户Storage容量占用超过上线的80%发送的邮件
        /// </summary>
        bool SendBackupStorageLimitWarningEmail(EmailMessageForPortal info);
        /// <summary>
        /// 跑job前发现用户Storage容量占用超过上线的100%发送的邮件
        /// </summary>
        bool SendBackupStorageLimitAlertEmail(EmailMessageForPortal info);

        bool SendEmail2PortalTenantAdmin(string groupId, EmailTemplate emailTemplate);
    }
}
