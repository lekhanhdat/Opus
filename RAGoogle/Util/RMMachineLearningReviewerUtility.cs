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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.MachineLearning;
using System.Collections.Concurrent;
using AvePoint.RA.Common.JobService;
using Microsoft.Exchange.WebServices.Data;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Reflection;

namespace RAGoogle.Util
{
    public class RMMachineLearningReviewerUtility
    {

        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static IRecordOwnerDao RecordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private static readonly AvePoint.RA.Contract.RMWeb.IEmailTemplateService EmailTemplateService = PlatformWindsorManager.GetService<AvePoint.RA.Contract.RMWeb.IEmailTemplateService>();
        private static readonly EmailTemplateDto EmailDto = EmailTemplateService.GetEmailTemplateByInternalType(EmailTemplateInternalType.MLManualApproval);
        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly ConcurrentDictionary<int, int> NeedSendEmailUserIntIds = new();

        protected static readonly IRMSubJobDao SubJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        protected static readonly IMLManualEmailNotificationDao MLManualEmailNotificationDao = PlatformWindsorManager.GetService<IMLManualEmailNotificationDao>();

        public static async Task<int[]> GetRecordOwnersAsync(int settingId, RecordOwnerSettingType settingType)
        {
            var owners = RecordOwnerDao.GetRecordOwner(settingId, settingType);

            int[] recordOwners = Array.Empty<int>(); ;
            if (owners != null && owners.Count > 0)
            {
                logger.Info($"start to get setting record owners, setting id:{settingId}, owners count: {owners.Count}");
                try
                {
                    var recordOwnerIDs = owners.Select(a => a.ObjectId).ToList();
                    recordOwners = (await AccountDao.FindListAsync(o => recordOwnerIDs.Contains(o.UserId))).Select(a => a.Id)?.ToArray();
                }
                catch (Exception ex)
                {
                    logger.Error($"failed to get record owners, message: {ex}");
                }
            }
            return recordOwners;
        }

        public static void AddNeedSendEmailUserId(IEnumerable<int> userIds)
        {
            foreach (var userId in userIds)
            {
                try
                {
                    if (!NeedSendEmailUserIntIds.TryGetValue(userId, out _))
                    {
                        if (!NeedSendEmailUserIntIds.TryAdd(userId, userId))
                        {
                            logger.Warn($"An error while adding to NeedSendEmailUserIntIds, userId: {userId}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Exception occurred while processing userId: {userId} {ex.Message}");
                }
            }
            logger.Info($"the length user need send email: {NeedSendEmailUserIntIds.Count()}");
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
                    logger.Info($"save reviewers to db, jobid: {jobId}, count: {reviewIds?.Count}");
                }
                else
                {
                    logger.Info($"No reviewers need to save, jobid: {jobId}");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"failed to save reviewers, message: {ex}");
            }
        }

        private static string GetMainJobId(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                logger.Info("job id is empty.");
            }
            if (JobServiceUtility.IsSubJob(jobId))
            {
                var subJob = SubJobDao.GetSubJob(jobId);
                return subJob?.ParentId;
            }
            return jobId;
        }
    }

}
