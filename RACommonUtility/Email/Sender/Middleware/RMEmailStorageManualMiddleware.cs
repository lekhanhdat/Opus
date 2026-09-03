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
using AvePoint.GCommon.GraphAPI;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Email.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Sender.Middleware
{
    public class RMEMailStorageManualMiddleware : IRMEmailStorageMiddleware
    {
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountWrapperService AADAccountWrapperService => PlatformWindsorManager.GetService<IAccountWrapperService>();


        private readonly HashSet<string> _addedUsers = new();

        public string Convert(Guid templateId,RMEmailTemplateParameters parameters)
        {
            var value = GetUserCacheValue(templateId,parameters);
            _addedUsers.Add(value);
            return value;
        }

        public RMEmailTemplateParameters ConvertRedis(Guid templateId,string parameters)
        {
            var userId = parameters.Split("=AVE=", StringSplitOptions.RemoveEmptyEntries)[0];
            var accountDb = AccountDao.GetUserWithRemovedByUserIds(new List<string> { userId }).OrderByDescending(item => item.CreateTime).First();
            var requestReviewerFirstName = UserService.GetReviewerFirstName(accountDb?.UserId);
            return new RMManualEmailTemplateParameters
            {
                ToUser = GetRecipientEmail(accountDb?.UserPrincipalName),
                RequestReviewer = accountDb?.DisplayName,
                RequestComment = "",
                TemplateType = RMEmailTemplateType.Manual,
                RequestReviewerFirstName = requestReviewerFirstName,
                CurrentDate = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.Ticks)
            };
        }

        public RMEmailTemplateParameters ConvertMemory(Guid templateId, RMEmailTemplateParameters parameters)
        {
            var manualParameters = parameters as RMManualEmailTemplateParameters;
            var accountDb = AccountDao.GetUserWithRemovedByUserIds(new List<string> { manualParameters.UserId }).OrderByDescending(item => item.CreateTime).FirstOrDefault();
            return new RMManualEmailTemplateParameters
            {
                ToUser = GetRecipientEmail(accountDb?.UserPrincipalName),
                RequestReviewer = accountDb?.DisplayName,
                RequestComment = manualParameters.RequestComment,
                TemplateType = RMEmailTemplateType.Manual,
                RequestReviewerFirstName = UserService.GetReviewerFirstName(accountDb?.UserId),
                CurrentDate = GeneralSettingService.ConvertTiksToDateNoTime(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult(), DateTimeOffset.Now.Ticks)
            };

        }

        public bool NeedAdded(Guid templateId,RMEmailTemplateParameters parameters)
        {
            var value = GetUserCacheValue(templateId, parameters);
            return !_addedUsers.Contains(value);
        }

        private static string GetUserCacheValue(Guid templateId,RMEmailTemplateParameters parameters)
        {
            var manualParameters = parameters as RMManualEmailTemplateParameters;

            return manualParameters.UserId+"=AVE="+ templateId ;
        }

        /// <summary>
        /// Preferably use Email, if Email is not available then use UPN.
        /// </summary>
        /// <param name="UPN">userPrincipalName</param>
        /// <returns>Email/UPN</returns>
        private static string GetRecipientEmail(string userPrincipalName)
        {
            if (string.IsNullOrEmpty(userPrincipalName))
            {
                return userPrincipalName;
            }

            var result = userPrincipalName;
            var accounts = AADAccountWrapperService.GetAccountsByUserOrGroupEmails(TenantLocalValue.LogonGroupId, new List<string> { userPrincipalName });
            if (accounts != null && accounts.Count > 0)
            {
                var email = accounts.First().Mail;
                result = email ?? userPrincipalName;
            }
            return result;
        }
    }
}
