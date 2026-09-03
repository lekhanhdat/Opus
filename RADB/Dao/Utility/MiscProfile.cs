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
using AvePoint.GCommon.Contract.Security;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Model;
using FluentFTP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AvePoint.GCommon.Utility.I18N.ContextValues.Configuration;

namespace AvePoint.RA.DB.Dao.Utility
{
    public class MiscProfileConvert
    {
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        public static StubSettingDto ConvertRMMiscProfileToStubSettingDto(RMMiscProfile rmProfile)
        {
            var gls = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            StubSettingDto stubSettingDto = new StubSettingDto();
            StubSettingParaDto para = new StubSettingParaDto();
            stubSettingDto.Id = rmProfile.Id;
            stubSettingDto.Name = rmProfile.Name;
            if (rmProfile.ModifiedTime <= 0)
            {
                stubSettingDto.LastModifiedTime = string.Empty;
            }
            else
            {
                stubSettingDto.LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, rmProfile.ModifiedTime, true).SimplifyFormatTime;
            }
            para = SerializerHelper.DeserializeByDataContractSerializer<StubSettingParaDto>(rmProfile.Extension);
            if (para != null)
            {
                stubSettingDto.StubType = para.StubType;
                stubSettingDto.StubContent = para.StubContent;
                stubSettingDto.IsDeclareStubAsRecords = para.IsDeclareStubAsRecords;
                stubSettingDto.IsEnabledRetention = para.IsEnabledRetention;
                if (para.IsEnabledRetention)
                {
                    stubSettingDto.RetentionValue = para.RetentionValue == 0 ? 1 : para.RetentionValue;
                    stubSettingDto.RetentionUnit = (DateUnit)para.RetentionUnit;
                }
            }
            stubSettingDto.IsRemoved = rmProfile.IsRemoved;
            return stubSettingDto;
        }
        public static StubSettingDto ConvertToStubSettingDto(StubSettingUIDto stubSettingUIDto)
        {
            StubSettingDto stubSettingDto = new StubSettingDto();
            stubSettingDto.Id = stubSettingUIDto.Id;
            stubSettingDto.Name = stubSettingUIDto.Name;
            stubSettingDto.StubType = stubSettingUIDto.StubType;
            stubSettingDto.StubContent = stubSettingUIDto.StubContent;
            stubSettingDto.StubCustomizeTags = stubSettingUIDto.StubCustomizeTags;
            stubSettingDto.IsDeclareStubAsRecords = stubSettingUIDto.IsDeclareStubAsRecords;
            stubSettingDto.IsEnabledRetention = stubSettingUIDto.IsEnabledRetention;
            if (stubSettingUIDto.IsEnabledRetention)
            {
                stubSettingDto.RetentionValue = stubSettingUIDto.RetentionValue == 0 ? 1 : stubSettingUIDto.RetentionValue;
                stubSettingDto.RetentionUnit = (DateUnit)stubSettingUIDto.RetentionUnit;
            }
            return stubSettingDto;
        }
        public static StubSettingUIDto ConvertToStubSettingUIDto(StubSettingDto stubSettingDto)
        {
            StubSettingUIDto stubSettingUIDto = new StubSettingUIDto();
            stubSettingUIDto.Id = stubSettingDto.Id;
            stubSettingUIDto.Name = stubSettingDto.Name;
            stubSettingUIDto.StubType = stubSettingDto.StubType;
            stubSettingUIDto.StubContent = stubSettingDto.StubContent;
            stubSettingUIDto.StubCustomizeTags = stubSettingDto.StubCustomizeTags;
            stubSettingUIDto.IsDeclareStubAsRecords = stubSettingDto.IsDeclareStubAsRecords;
            stubSettingUIDto.LastModifiedTime= stubSettingDto.LastModifiedTime;
            stubSettingUIDto.IsEnabledRetention = stubSettingDto.IsEnabledRetention;
            if (stubSettingDto.IsEnabledRetention)
            {
                stubSettingUIDto.RetentionValue = stubSettingDto.RetentionValue == 0 ? 1 : stubSettingDto.RetentionValue;
                stubSettingUIDto.RetentionUnit = (int)stubSettingDto.RetentionUnit;
            }
            return stubSettingUIDto;
        }
        public static EndUserRestoreSettingDto ConvertToEndUserRestoreSettingDto(EndUserRestoreSettingUIDto endUserUIDto)
        {
            EndUserRestoreSettingDto endUserDto = new EndUserRestoreSettingDto();
            endUserDto.IsRestoreArchivedTier = endUserUIDto.IsRestoreArchivedTier;
            endUserDto.IsCustomizeStubRestorePage = endUserUIDto.IsCustomizeStubRestorePage;
            endUserDto.IsIncludeSharedLinks = endUserUIDto.IsIncludeSharedLinks;
            endUserDto.Logo= endUserUIDto.Logo;
            endUserDto.Message = endUserUIDto.Message;
            endUserDto.Footer= endUserUIDto.Footer;
            endUserDto.IsAllowRestore= endUserUIDto.IsAllowRestore;
            if (!endUserUIDto.IsAllowRestore)
            {
                endUserDto.PermissionSetting = new EndUserPermissionSetting() { IsSearchGroupTeamSite = true,IsSearchSiteCollection = true};
            }
            else
            {
                endUserDto.PermissionSetting = endUserUIDto.PermissionSetting;
                if (endUserUIDto.PermissionSetting.StubOopRestoreSetting == null)
                {
                    endUserDto.PermissionSetting.StubOopRestoreSetting = new StubOopRestoreSetting
                    {
                        IsEnableSearchStubLocation = true,
                        IsEnableManualInputDesStubLocation = true,
                        IsEnableStubOopRestore = true
                    };
                }
                else
                {
                    StubOopRestoreSetting uiStubOopRestoreSetting = endUserUIDto.PermissionSetting.StubOopRestoreSetting;
                    endUserDto.PermissionSetting.StubOopRestoreSetting = new StubOopRestoreSetting
                    {
                        IsEnableStubOopRestore = (uiStubOopRestoreSetting.IsEnableSearchStubLocation || uiStubOopRestoreSetting.IsEnableManualInputDesStubLocation) && uiStubOopRestoreSetting.IsEnableStubOopRestore,
                        IsEnableManualInputDesStubLocation = uiStubOopRestoreSetting.IsEnableManualInputDesStubLocation && uiStubOopRestoreSetting.IsEnableStubOopRestore,
                        IsEnableSearchStubLocation = uiStubOopRestoreSetting.IsEnableSearchStubLocation && uiStubOopRestoreSetting.IsEnableStubOopRestore
                    };
                }
            }
            return endUserDto;
        }
        public static EndUserRestoreSettingUIDto ConvertToEndUserRestoreSettingUIDto(EndUserRestoreSettingDto endUserDto)
        {
            EndUserRestoreSettingUIDto endUserUIDto = new EndUserRestoreSettingUIDto();
            endUserUIDto.IsRestoreArchivedTier = endUserDto.IsRestoreArchivedTier;
            endUserUIDto.IsCustomizeStubRestorePage = endUserDto.IsCustomizeStubRestorePage;
            endUserUIDto.IsIncludeSharedLinks= endUserDto.IsIncludeSharedLinks;
            endUserUIDto.Logo = endUserDto.Logo;
            endUserUIDto.Message = endUserDto.Message;
            endUserUIDto.Footer = endUserDto.Footer;
            endUserUIDto.IsAllowRestore = endUserDto.IsAllowRestore;
            endUserUIDto.PermissionSetting = endUserDto.PermissionSetting;
            return endUserUIDto;
        }
        public static RMMiscProfile ConvertStubSettingDtoToRMMiscProfile(StubSettingDto stubSettingDto, bool daoMigrated = false)
        {
            RMMiscProfile rmProfile = new RMMiscProfile();
            StubSettingParaDto para = new StubSettingParaDto();
            if (stubSettingDto.Id == null)
            {
                rmProfile.Id = Guid.NewGuid().ToString();
            }
            else
            {
                rmProfile.Id = stubSettingDto.Id;
            }
            rmProfile.Name = stubSettingDto.Name;
            rmProfile.Type = (int)ProfileType.StubSetting;
            //rmProfile.ModifiedTime = stubSettingUIDto.LastModifiedTime;
            para.StubType = stubSettingDto.StubType;
            para.StubContent = stubSettingDto.StubContent;
            para.IsDeclareStubAsRecords = stubSettingDto.IsDeclareStubAsRecords;
            para.IsEnabledRetention = stubSettingDto.IsEnabledRetention;
            if (stubSettingDto.IsEnabledRetention)
            {
                para.RetentionValue = stubSettingDto.RetentionValue == 0 ? 1 : stubSettingDto.RetentionValue;
                para.RetentionUnit = (int)stubSettingDto.RetentionUnit;
            }
            rmProfile.IsRemoved = stubSettingDto.IsRemoved;
            rmProfile.ModifiedTime = DateTime.UtcNow.Ticks;
            rmProfile.Extension = SerializerHelper.SerializeByDataContractSerializer(para);
            rmProfile.DAOMigrated = daoMigrated;
            return rmProfile;
        }
    }
}
