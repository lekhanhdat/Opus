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
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMReport;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EndUserPermissionSetting = AvePoint.GCommon.Contract.Server.EndUserRestoreSetting.EndUserPermissionSetting;

namespace AvePoint.RA.Service.Services.Settings
{
    [Audit]
    public class EndUserRestoreSettingService : RMServiceBase, IEndUserRestoreSettingService
    {
        private IRMMiscProfileDao MiscProfileDao => PlatformWindsorManager.GetService<IRMMiscProfileDao>();
        private RALogger logger = RALogger.GetInstance(typeof(RMReportService));

        public async Task<EndUserRestoreSettingUIDto> GetEndUserRestoreSettingAsync()
        {
            EndUserRestoreSettingDto setting = new EndUserRestoreSettingDto();
            RMMiscProfile profile = new RMMiscProfile()
            {
                Type = (int)ProfileType.EndUserArchiverSetting,
                Name= "EndUserRestoreSetting"
            };
            RMMiscProfile dbProfile = await MiscProfileDao.LoadAsync(profile);
            if (dbProfile != null)
            {
                setting = SerializerHelper.DeserializeByDataContractSerializer<EndUserRestoreSettingDto>(dbProfile.Extension);
                if (setting.PermissionSetting == null)
                {
                    setting.PermissionSetting = new EndUserPermissionSetting() { IsSearchGroupTeamSite = true, IsSearchSiteCollection = true };
                }
                else
                {
                    if (setting.PermissionSetting.IsSearchGroupTeamSite == null)
                    {
                        setting.PermissionSetting.IsSearchGroupTeamSite = true;
                    }
                    if (setting.PermissionSetting.IsSearchSiteCollection == null)
                    {
                        setting.PermissionSetting.IsSearchSiteCollection = true;
                    }
                }
                if (setting.PermissionSetting.StubOopRestoreSetting == null)
                {
                    setting.PermissionSetting.StubOopRestoreSetting = new StubOopRestoreSetting
                    {
                        IsEnableManualInputDesStubLocation = true,
                        IsEnableSearchStubLocation = true,
                        IsEnableStubOopRestore = true
                    };
                }
            }
            else
            {
                setting.Footer = "";
                setting.IsCustomizeStubRestorePage = false;
                setting.IsRestoreArchivedTier = false;
                setting.IsIncludeSharedLinks = false;
                setting.Logo = "";
                setting.IsAllowRestore = true;
                setting.Message = I18NEntity.GetString("StorageOptimization.Gui_357cafb4-ed90-4141-b4e3-bd67a82624f6");
                setting.PermissionSetting = new EndUserPermissionSetting();
                setting.PermissionSetting.IsExportGroupTeamSite = true;
                setting.PermissionSetting.IsExportSiteCollection = true;
                setting.PermissionSetting.IsExportStubLink = true;
                setting.PermissionSetting.IsRestoreGroupTeamSite = true;
                setting.PermissionSetting.IsRestoreSiteCollection = true;
                setting.PermissionSetting.IsRestoreStubLink = true;
                setting.PermissionSetting.SiteCollection = 0;
                setting.PermissionSetting.TeamsAndGroup = 0;
                setting.PermissionSetting.SiteCollectionSpecialGroupNames = null;
                setting.PermissionSetting.IsSearchGroupTeamSite = true;
                setting.PermissionSetting.IsSearchSiteCollection = true;
                setting.PermissionSetting.StubOopRestoreSetting = new StubOopRestoreSetting
                {
                    IsEnableManualInputDesStubLocation = true,
                    IsEnableSearchStubLocation = true,
                    IsEnableStubOopRestore = true
                };
                RMMiscProfile mProfile = new RMMiscProfile()
                {
                    Type = (int)ProfileType.EndUserArchiverSetting,
                    Name = "EndUserRestoreSetting"
                };
                profile.Id = Guid.NewGuid().ToString();
                profile.Extension = SerializerHelper.SerializeByDataContractSerializer(setting);
                await MiscProfileDao.CreateAsync(profile);
            }
            return MiscProfileConvert.ConvertToEndUserRestoreSettingUIDto(setting);
        }

        public EndUserRestoreSettingUIDto GetEndUserRestoreSetting()
        {
            return GetEndUserRestoreSettingAsync().GetAwaiter().GetResult();
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.EndUserRestoreSetting, Action = AuditAction.ConfigureEndUserRestoreSetting, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public async Task<int> SaveEndUserRestoreSettingAsync(EndUserRestoreSettingDto setting, bool daoMigrated = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(setting.Logo))
                {
                    if (!setting.Logo.StartsWith("data:image"))
                    {
                        throw new Exception("logo is not a image.");
                    }
                    else if (!CheckLogoSize(setting.Logo))
                    {
                        throw new Exception("logo size exceed 5MB.");
                    }
                }
                RMMiscProfile profile = new RMMiscProfile()
                {
                    Type = (int)ProfileType.EndUserArchiverSetting,
                    Name = "EndUserRestoreSetting"
                };
                RMMiscProfile dbProfile = MiscProfileDao.Load(profile);
                if (dbProfile == null)
                {
                    profile.Id = Guid.NewGuid().ToString();
                    profile.Extension= SerializerHelper.SerializeByDataContractSerializer(setting);
                    profile.DAOMigrated = daoMigrated;
                    MiscProfileDao.Create(profile);
                }
                else
                {
                    profile.Id = dbProfile.Id;
                    profile.Extension = SerializerHelper.SerializeByDataContractSerializer(setting);
                    await MiscProfileDao.UpdateAsync(profile);
                }
                logger.Info("Save end user restore setting successful.");
                return (int)SOMessageType.Successful;
            }
            catch (Exception ex)
            {
                logger.Error($"Save end user restore setting failed, error {ex}");
                return (int)SOMessageType.Failed;
            }
        }
        private bool CheckLogoSize(string logoBase64String)
        {
            var logoString = logoBase64String;
            if (logoString.StartsWith("data:image/bmp;base64,"))
            {
                logoString = logoString.Replace("data:image/bmp;base64,", null);
            }
            else if (logoString.StartsWith("data:image/png;base64,"))
            {
                logoString = logoString.Replace("data:image/png;base64,", null);
            }
            else if (logoString.StartsWith("data:image/jpeg;base64,"))
            {
                logoString = logoString.Replace("data:image/jpeg;base64,", null);
            }
            else if (logoString.StartsWith("data:image/jpg;base64,"))
            {
                logoString = logoString.Replace("data:image/jpg;base64,", null);
            }
            var length = logoString.Replace("=", null).Length;
            var size = length - Math.Ceiling(Convert.ToDecimal(length / 8)) * 2;
            if (size > 1024 * 1024 * 5)
            {
                return false;
            }
            return true;
        }
    }
}
