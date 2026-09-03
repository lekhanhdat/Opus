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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ManualApproval.Setting
{
    public class ManualApprovalSettingManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalSettingManager));

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        public ManualApprovalSettingManager()
        {       
            var existingSettingJson = FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting).GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(existingSettingJson))
            {
                var settingInfo = new ManualApprovalSettings();
                FunctionSettingDao.NotExistCreateIt(FunctionSettingType.ManualSetting, JsonConvert.SerializeObject(settingInfo)).GetAwaiter().GetResult();
            }
            else
            {
                var existingSettingInfo = JsonConvert.DeserializeObject<ManualApprovalSettings>(existingSettingJson);
                var extendTypeMap = new Dictionary<ManualApprovalExtendType, (ManualApprovalExtendType NewType, int Number)>
                                    {
                                        { ManualApprovalExtendType.After3Month, (ManualApprovalExtendType.Month, 3) },
                                        { ManualApprovalExtendType.After6Month, (ManualApprovalExtendType.Month, 6) },
                                        { ManualApprovalExtendType.After1Year, (ManualApprovalExtendType.Year, 1) }
                                    };
                if (extendTypeMap.TryGetValue(existingSettingInfo.DisposalExtentionSetting.LatestExtendType, out var newExtendInfo))
                {
                    existingSettingInfo.DisposalExtentionSetting.LatestExtendType = newExtendInfo.NewType;
                    existingSettingInfo.DisposalExtentionSetting.LatestExtendNumber = newExtendInfo.Number;
                }
                FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualSetting, JsonConvert.SerializeObject(existingSettingInfo)).GetAwaiter().GetResult();
            }
        }

        public async Task<ManualApprovalSettings> Get()
        {
            try
            {
                var settingInfo = await FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualSetting);
                return JsonConvert.DeserializeObject<ManualApprovalSettings>(settingInfo);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while get manual approval setting info. Error: {e}");
                return new ManualApprovalSettings();
            }
        }

        public async Task<bool> Update(ManualApprovalSettings setting)
        {
            try
            {

                if(setting.EscalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.ReassignSpecificUsers)
                {
                    var toUsers = setting.EscalationSetting.ReassignUsers;
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, toUsers);
                    var userIds = toUsers.Select(item => item.UserId).ToList();
                    var accounts = await AccountDao.FindListAsync(item => userIds.Contains(item.UserId) && item.IsRemoved == 0);
                    if(accounts.Count != toUsers.Count)
                    {
                        Logger.Error($"Has user not found in record.");
                        return false;
                    }

                    var reassignUsers = accounts.ConvertAll(item => new ToUserInfo
                    {
                        DisplayName = item.DisplayName,
                        UserId = item.UserId,
                        RMUserId = item.Id,
                        UserPrincipalName = item.UserPrincipalName,
                        Id = item.AADId,
                        InviteType = item.ObjectType == RMActiveDirectoryObjectType.User || item.ObjectType == RMActiveDirectoryObjectType.UserInGroup ? AccountType.User : AccountType.Group
                    });
                    setting.EscalationSetting.ReassignUsers = reassignUsers;
                }
                CorrectManualApprovalSetting(setting);
                var settingInfo = JsonConvert.SerializeObject(setting);
                return await FunctionSettingDao.AddOrUpdateSettingInfoAsync(FunctionSettingType.ManualSetting, settingInfo);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while update manual approval setting info. Error: {e}");
                return false;
            }
        }

        private static void CorrectManualApprovalSetting(ManualApprovalSettings setting)
        {
            if(setting.EmailNotificationSetting.EndType == ManualApprovalEndType.NoEnd)
            {
                setting.EmailNotificationSetting.OccurrencesTimes = 3;
            }

            if(setting.EscalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.WorkflowNextStep)
            {
                setting.EscalationSetting.ReassignUsers = new List<ToUserInfo>();
            }

            if(setting.EscalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.ReassignSpecificUsers)
            {
                setting.EscalationSetting.ApprovalStatus = SOApproveDBStatus.Rejected;
            }

            if(setting.EscalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.NoAction)
            {
                setting.EscalationSetting.ReassignUsers = new List<ToUserInfo>();
                setting.EscalationSetting.ApprovalStatus = SOApproveDBStatus.Rejected;
            }
        }
    }
}
