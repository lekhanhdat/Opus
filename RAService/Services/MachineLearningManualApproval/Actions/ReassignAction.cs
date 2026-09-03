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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.MachineLearningManualApproval.Actions
{
    public class ReassignAction
    {
        private static readonly IRALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IEmailTemplateService EmailTemplateService => PlatformWindsorManager.GetService<IEmailTemplateService>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly MLManualApprovalRecordRepository Repository;

        private readonly RMAccount ActionAccount;

        private readonly EmailTemplateDto EmailDto;

        public ReassignAction(MLManualApprovalRecordRepository repository)
        {
            Repository = repository;
            var accountId = TenantLocalValue.LogonUserId;
            ActionAccount = AccountDao.Find(item => item.UserId == accountId && item.IsRemoved == 0);
            EmailDto = EmailTemplateService.GetEmailTemplateByInternalType(EmailTemplateInternalType.MLManualApproval);
        }

        public async System.Threading.Tasks.Task<ManualApprovalActionResult> Reassign(ManualAprovalEscalateDefinition definition)
        {
            try
            {
                (var synced, var accounts) = await TrySyncUsersAsync(definition.ToUsers);
                if (!synced)
                {
                    return new ManualApprovalActionResult
                    {
                        CompletedStatus = ActionCompletedStatus.Failed,
                        Message = I18NEntity.GetString("RM_RegisterUser_Error_Message")
                    };
                }

                var result = new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Succeed,
                };

                var items = await Repository.QueryItemsAsync(item => definition.ItemIds.Contains(item.Id));
                var accountIds = accounts.Select(item => item.Id).ToArray();
                var nowTicks = DateTime.UtcNow.Ticks;
                items.ForEach(item =>
                {
                    //item.ManualEmailNotificationCount = 0;
                    //item.ManualEmailNotificationLastTime = DateTime.UtcNow.Ticks;
                    //item.ManualIsAutoReassigned = false;
                    var itemActionResult = ReassignItem(item, accountIds, nowTicks, definition.Comment);
                    result.EffectItems.Add(itemActionResult);
                });

                await Repository.UpsertItems(items);

                if (definition.NeedSendEmail)
                {
                    await SendEmailAsync(accounts, definition.Comment);
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute reassign action for items: [{string.Join(", ", definition.ItemIds)}]. Error: {e}");
                return new ManualApprovalActionResult
                {
                    CompletedStatus = ActionCompletedStatus.Failed,
                    Message = e.Message
                };
            }
        }

        private ManualApprovalItemActionResult ReassignItem(ManualApprovalRecord item, int[] reviewIds, long nowTicks, string comment)
        {
            item.MLEscalateFrom = ActionAccount.Id;
            item.MLReviewer = reviewIds;
            item.MLEscalatedComment = comment;
            //item.MLManualActionTime = nowTicks;
            //item.MLManualAudits = ReBuildAudits(item);
            return new ManualApprovalItemActionResult
            {
                IsSucceed = true,
                EffectItemFullPath = item.LeafName
            };
        }

        //private string ReBuildAudits(ManualApprovalRecord item)
        //{
        //    var audits = new List<ReviewAudits>();
        //    if (!string.IsNullOrEmpty(item.ManualAudits))
        //    {
        //        audits = SerializerHelper.DeserializeFromXmlString<List<ReviewAudits>>(item.MLManualAudits);
        //    }
        //    audits.Add(new ReviewAudits
        //    {
        //        ReviewTime = DateTime.UtcNow.Ticks.ToString(),
        //        ReviewBy = ActionAccount.DisplayName,
        //        Action = "RM_JS_MA_ApproveStatus_Reassigned"
        //    });

        //    return SerializerHelper.SerializeToXmlString(audits);
        //}

        private async Task<(bool, List<RMAccount>)> TrySyncUsersAsync(List<ToUserInfo> toUsers)
        {
            List<RMAccount> accounts = new List<RMAccount>();
            try
            {
                await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, toUsers);
                var userIds = toUsers.Select(item => item.UserId).ToList();
                accounts = await AccountDao.FindListAsync(item => userIds.Contains(item.UserId) && item.IsRemoved == 0);
                return (true, accounts);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while try sync users. Error: {e}");
                return (false, accounts);
            }
        }

        private async System.Threading.Tasks.Task SendEmailAsync(List<RMAccount> accounts, string comment)
        {
            try
            {
                var glsSetting = await GeneralSettingService.GetGeneralSettingAsync();

                foreach (var account in accounts)
                {
                    var para = new ManualParameterDto
                    {
                        Reviewer = account.DisplayName,
                        ReviewerEmail = account.UserPrincipalName,
                        Comment = comment,
                        RequestReviewerFirstName = UserService.GetReviewerFirstName(account.UserId),
                        CurrentDate = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.Ticks)
                    };

                    MailUtil.SendEmailTemplate(EmailDto, para, glsSetting.EmailSenderDefinition);
                }

                Logger.Info($"Succeed send ml email to reassign users.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while send ml email to reassign users. Error: {e}");
            }
        }
    }
}
