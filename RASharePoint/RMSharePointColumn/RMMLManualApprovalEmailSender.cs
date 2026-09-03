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
using AvePoint.RA.Common;
using AvePoint.RA.Common.JobService;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMSharePointColumn
{
    public class RMMLManualApprovalEmailSender
    {

        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ConcurrentDictionary<int, int> NeedSendEmailUserIntIds = new();

        protected static readonly IMLManualEmailNotificationDao MLManualEmailNotificationDao = PlatformWindsorManager.GetService<IMLManualEmailNotificationDao>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        protected static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        protected static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();
        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static readonly IEmailTemplateService EmailTemplateService = PlatformWindsorManager.GetService<IEmailTemplateService>();

        private static readonly EmailTemplateDto EmailDto;

        static RMMLManualApprovalEmailSender()
        {
            EmailDto = EmailTemplateService.GetEmailTemplateByInternalType(EmailTemplateInternalType.MLManualApproval);
        }

        public static void AddNeedSendEmailUserId(IEnumerable<int> userIds)
        {
            foreach (var userId in userIds)
            {
                if (!NeedSendEmailUserIntIds.TryGetValue(userId, out _))
                {
                    if (!NeedSendEmailUserIntIds.TryAdd(userId, userId))
                    {
                        Logger.Warn($"An error while add need send email, id: {userId}");
                    }
                }
            }
        }

        public static void Commit(string jobId)
        {
            try
            {
                if (!NeedSendEmailUserIntIds.IsEmpty)
                {
                    var mainJobdId = GetMainJobId(jobId);
                    var reviewIds = NeedSendEmailUserIntIds?.Keys?.Distinct().ToList();
                    MLManualEmailNotificationDao.BatchAdd(new MLManualEmailDto
                    {
                        JobId = mainJobdId,
                        ReviewerIds = reviewIds
                    });
                    Logger.Info($"save reviewers to db, jobid: {jobId}, count: {reviewIds?.Count}");
                }
                else
                {
                    Logger.Info($"No reviewers need to save, jobid: {jobId}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"failed to save reviewers, message: {ex}");
            }
        }

        public static async Task SendAsync(string jobId)
        {
            using (new PerformanceScope("Send waiting for approval email to reviewers."))
            {
                try
                {
                    Logger.Info("start to send email.");
                    var mainJobId = GetMainJobId(jobId);
                    var reviewerIds = MLManualEmailNotificationDao.GetReviewerIds(jobId);
                    await SendEmialToReviewersAsync(reviewerIds);
                    Logger.Info($"Successfully sent email to reviewers, jobId: {jobId}");
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to send email, jobId: {jobId}, message: {e}");
                }
                finally
                {
                    MLManualEmailNotificationDao.Remove(jobId);
                    Logger.Info("remove reviewers cache in db.");
                }
            }
        }

        private static string GetMainJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                Logger.Info("job id is empty.");
            }
            if (JobServiceUtility.IsSubJob(jobId))
            {
                var subJob = SubJobDao.GetSubJob(jobId);
                return subJob?.ParentId;
            }
            return jobId;
        }

        private static async Task<List<string>> SendEmialToReviewersAsync(List<int> reviewerIds)
        {
            var succeedSendEmailUser = new List<string>();
            try
            {
                var glsSetting = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
				var accounts = await AccountDao.GetUserByIdsAsync(reviewerIds);
                foreach (var account in accounts)
                {
                    var para = new ManualParameterDto
                    {
                        Reviewer = account.DisplayName,
                        ReviewerEmail = account.UserPrincipalName,
                        Comment = "",
                        RequestReviewerFirstName = UserService.GetReviewerFirstName(account.UserId),
                        CurrentDate = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.Ticks)
                    };

                    MailUtil.SendEmailTemplate(EmailDto, para, glsSetting.EmailSenderDefinition);
                    succeedSendEmailUser.Add(account.UserId);
                }
                Logger.Info($"Total need send email user: [{reviewerIds?.Count}], Successful send email user count: [{succeedSendEmailUser.Count}].");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while send email to owners. Error: {e}");
            }
            return succeedSendEmailUser;
        }

    }
}
