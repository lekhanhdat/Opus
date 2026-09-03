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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.RACommonUtility.Email.Model;
using AvePoint.RA.RACommonUtility.Email.Sender.Storage;
using AvePoint.RA.RACommonUtility.Email.Sender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common.Email;
using AvePoint.RA.RACommonUtility.Email.Sender.Middleware;

namespace AvePoint.RA.Service.Services.Archiver
{
    public class ExportSendEmail
    {
        private static readonly IRALogger Logger = RALogger.GetInstance(typeof(ExportSendEmail));

        private static IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();
        private static readonly RMEmailSender s_emailSender = new(new RMEmailMemoryStorage(new RMEmailStorageDefaultMiddleware()));
        private readonly EmailTemplateDto EmailDto;
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        public ExportSendEmail()
        {
            EmailDto = EmailTemplateService.GetEmailTemplateByInternalType(EmailTemplateInternalType.ExportZipPassword);
        }
        public async Task SendEmailAsync(List<ToExportUserInfo> accounts, ParameterDto para)
        {
            try
            {
                var parameters = new List<RMExportZipPasswordEmailTemplateParameters>();
                foreach (var temp in accounts)
                {
                    parameters.Add(new RMExportZipPasswordEmailTemplateParameters()
                    {
                        RequestReviewer = temp.DisplayName,
                        RequestJobId = para.RestoreJobid,
                        RequestLocation = para.ExportLocation,
                        RequestPassword = para.ZipPassword,
                        ToUser=temp.UserPrincipalName,
                        TemplateType= RMEmailTemplateType.ExportZipPassword,
                        RequestReviewerFirstName = temp.InviteType == AccountType.Group? temp.DisplayName:UserService.GetReviewerFirstNameForExportZip(temp.UserId, temp.UserPrincipalName),
                        UserId = temp.UserId
                    });
                }
                var templateId = RMEmailTemplateId.EXPORT_ZIP_PASSWORD;
                s_emailSender.AddRange(templateId, parameters);
                s_emailSender.SendAsync().GetAwaiter().GetResult();
                Logger.Info($"Succeed send email to escalate/reassign users.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while send email to escalate/reassign users. Error: {e}");
            }
        }
        public void SendEmail(List<ToExportUserInfo> accounts, ParameterDto para)
        {
            try
            {
                var mAccounts = ConverToUserInfo(accounts);
                foreach (var account in mAccounts)
                {
                    para.Reviewer = account.DisplayName;
                    para.CurrentDate = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.Ticks);
                    para.RequestRequesterFirstname = UserService.GetReviewerFirstName(account.UserId);
                    MailUtil.SendEmailTemplate(EmailDto, para, new List<ToUserInfo>() { account });
                }
                Logger.Info($"Succeed send email to escalate/reassign users.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while send email to escalate/reassign users. Error: {e}");
            }
        }
        private List<ToUserInfo> ConverToUserInfo(List<ToExportUserInfo> exportUserInfo)
        {
            List<ToUserInfo> result = new List<ToUserInfo>();
            foreach (var ex in exportUserInfo)
            {
                ToUserInfo temp = new ToUserInfo()
                {
                    UserId = ex.UserId,
                    UserName = ex.UserName,
                    UserPrincipalName = ex.UserPrincipalName,
                    Email = ex.Email,
                    DisplayName = ex.DisplayName,
                    InviteType = (Contract.Object.AccountType)(int)ex.InviteType,
                    RMUserId = ex.RMUserId,
                    Id = ex.Id,
                    SurName = ex.SurName,
                    GivenName = ex.GivenName,
                    TenantId = ex.TenantId,
                };
                result.Add(temp);
            }
            return result;
        }
    }
}
