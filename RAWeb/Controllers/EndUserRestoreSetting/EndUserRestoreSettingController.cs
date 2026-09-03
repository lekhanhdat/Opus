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
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.EndUserRestoreSetting
{
    [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, preferred: false)]
    public class EndUserRestoreSettingController : BaseApiController
    {
        private IEndUserRestoreSettingService _EndUserSetting;
        private IEndUserRestoreSettingService EndUserSetting => PlatformWindsorManager.GetService(ref _EndUserSetting);
        [HttpPost]
        public RAReturnMessage SaveEndUserRestoreSetting([FromBody] EndUserRestoreSettingUIDto setting)
        {
            RAReturnMessage status = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            var specialGroupNames = setting?.PermissionSetting?.SiteCollectionSpecialGroupNames;
            if (!string.IsNullOrEmpty(specialGroupNames) && specialGroupNames.Any(c => "[]|\"'/:<>+=,?@".Contains(c)))
            {
                string bannedCharString = "[]|\"'/:<>+=,?@";
                status.MessageType= RAMessageType.Failed;
                status.ErrorMessage = string.Format(I18NEntity.GetString("RM_AR_EndUserRestoreSetting_GroupName_ErrorMessage"), bannedCharString);
                return status;
            }
            else if (!string.IsNullOrEmpty(specialGroupNames) && specialGroupNames.Length > 256)
            {
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = I18NEntity.GetString("RM_AR_EndUserRestoreSetting_GroupNameLenth_ErrorMessage");
                return status;
            }

            if (!string.IsNullOrWhiteSpace(specialGroupNames))
            {
                var validGroupNames = specialGroupNames
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (validGroupNames.Count > 5)
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("Cannot exceed 5");
                    return status;
                }

                if (validGroupNames.Any(groupName => !IsValidGroupNamePattern(groupName)))
                {
                    status.MessageType = RAMessageType.Failed;
                    status.ErrorMessage = I18NEntity.GetString("RM_AR_EndUserRestoreSetting_GroupName_Wildcard_ErrorMessage");
                    return status;
                }

                setting.PermissionSetting.SiteCollectionSpecialGroupNames = validGroupNames.Any() ? string.Join(";", validGroupNames) : null;
            }

            var tempDto = MiscProfileConvert.ConvertToEndUserRestoreSettingDto(setting);
            var isSuccess=EndUserSetting.SaveEndUserRestoreSettingAsync(tempDto).GetAwaiter().GetResult();
            if (isSuccess == (int)SOMessageType.Successful)
            {
                return status;
            }
            else
            {
                status.MessageType = RAMessageType.Failed;
                status.ErrorMessage = "Unknow error,please check log";
                return status;
            }
        }

        [HttpGet]
        public EndUserRestoreSettingUIDto GetEndUserRestoreSetting()
        {
            return EndUserSetting.GetEndUserRestoreSetting();
        }

        private static bool IsValidGroupNamePattern(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                return false;
            }

            var startsWithWildcard = groupName.StartsWith("*");
            var endsWithWildcard = groupName.EndsWith("*");
            var startIndex = startsWithWildcard ? 1 : 0;
            var endIndex = endsWithWildcard ? groupName.Length - 1 : groupName.Length;
            var coreLength = endIndex - startIndex;
            if (coreLength <= 0)
            {
                return false;
            }

            var core = groupName.Substring(startIndex, coreLength);
            return !core.Contains("*");
        }
    }
}
