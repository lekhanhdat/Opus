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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RATeams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Archiver
{
    [RMApiAuthorize(RMSOPermissionMasks.ControlPanelAdmin, preferred: false)]
    public class RetentionApiController : BaseApiController
    {
        private IRMArchiverSettingsService _RMArchiverSettingsService;
        private IRMArchiverSettingsService RMArchiverSettingsService => PlatformWindsorManager.GetService(ref _RMArchiverSettingsService);
        private ISettingProfileService _SettingProfileService;
        private ISettingProfileService SettingProfileService => PlatformWindsorManager.GetService(ref _SettingProfileService);

        private IRMSecurityTrimmingHelper mSecurityTrimmingHelper;
        public IRMSecurityTrimmingHelper SecurityTrimmingHelper
        {
            get
            {
                if (mSecurityTrimmingHelper == null)
                {
                    mSecurityTrimmingHelper = (IRMSecurityTrimmingHelper)PlatformWindsorManager.GetService(typeof(IRMSecurityTrimmingHelper));
                }
                return mSecurityTrimmingHelper;
            }
        }
        
        private ILicenseHelperService _licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();

        private ITenantService _tenantService => PlatformWindsorManager.GetService<ITenantService>();

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMPermissionExtensionMasks.GoogleAdmin, RMSOPermissionMasks.ControlPanelAdmin, PermissionJoinType.Any)]
        public RAReturnMessage ManualRunRetentionJob([FromBody] bool fromTimerJobPage)
        {
            if (SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.ControlPanelAdmin).GetAwaiter().GetResult() && _licenseHelperService.HasOpusSPILOrSOLicense)
            {
                var message = RMArchiverSettingsService.RunArchiverRetentionJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                if (message == null || message.MessageType == RAMessageType.Failed)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }

                if (TeamsPermissionHelper.HasUpgradeTeamsFeature())
                {
                    var tMessage = RMArchiverSettingsService.RunTeamsArchiverRetentionJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                    if (tMessage == null || tMessage.MessageType == RAMessageType.Failed)
                    {
                        return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                    }

                    var eMessage = RMArchiverSettingsService.RunEXOArchiverRetentionJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                    if (eMessage == null || eMessage.MessageType == RAMessageType.Failed)
                    {
                        return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                    }
                }
            }
            if (SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSEnduser).GetAwaiter().GetResult())
            {
                var fsMessage = RMArchiverSettingsService.RunFSRetentionJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                if (fsMessage == null || fsMessage.MessageType == RAMessageType.Failed)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            if (SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin).GetAwaiter().GetResult())
            {
                var gDriveMessage = RMArchiverSettingsService.RunGDriveArchiverRetentionJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
                if (gDriveMessage == null || gDriveMessage.MessageType == RAMessageType.Failed)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            return new RAReturnMessage();
        }

        [HttpPost]
        public RAReturnMessage RunDeleteRestoredDataJob([FromBody] bool fromTimerJobPage)
        {
            var message = RMArchiverSettingsService.RunArchiverDeleteRestoredDataJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
            if (message == null || message.MessageType == RAMessageType.Failed)
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return new RAReturnMessage();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage RunArchiverDeduplicationJob([FromBody] bool fromTimerJobPage)
        {
            if (!SettingProfileService.IsEnableArchiverDeduplication())
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = "De-duplication not enabled" };
            }
            var message = RMArchiverSettingsService.RunArchiverDeduplicationJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
            if (message == null || message.MessageType == RAMessageType.Failed)
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return new RAReturnMessage();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public bool IsEnableDeduplication()
        {
            return SettingProfileService.IsEnableArchiverDeduplication();
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage RunDeleteOrphanDatasJob([FromBody] List<string> needDeleteJobIds)
        {
            var message = RMArchiverSettingsService.RunDeleteOrphanDatasJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail, needDeleteJobIds);
            if (message == null || message.MessageType == RAMessageType.Failed)
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty, ErrorMessage = "Run delete orphan data job failed." };
            }
            return new RAReturnMessage();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<IActionResult> DownloadCurrentRetentionSettings()
        {
            try
            {
                var fileStream = await RMArchiverSettingsService.GetCurrentRetentionSettingsFileStream();
                var fileName = await RMArchiverSettingsService.GetUploadedCustomRetentionSettingsFileName();
                return File(fileStream, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in DownloadCurrentRetentionSettings: ", ex);
                return BadRequest(I18NEntity.GetString("RM_Retention_Settings_GetFailed"));
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<IActionResult> GetRetentionSettings()
        {
            var result = await RMArchiverSettingsService.GetRetentionSettingsAsync();
            return Ok(result);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public async Task<RAReturnMessage> SaveRetentionSettings()
        {
            try
            {
                if (!_tenantService.IsNewOpusTenant())
                {
                    Logger.Warn("Tenant is not new Opus tenant, cannot save retention settings.");
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.MissingRequiredSettings,
                        Extension = string.Empty,
                        ErrorMessage = I18NEntity.GetString("RM_Retention_Settings_SaveFailed"),
                    };
                }

                bool hasNoFiles = Request.Form.Files is null || Request.Form.Files.Count == 0;
                bool isNoChangeDirectSave =
                    bool.TryParse(Request.Form["IsNoChangeDirectSave"], out var value) && value;
                if (hasNoFiles)
                {
                    if (isNoChangeDirectSave)
                    {
                        return new RAReturnMessage()
                        {
                            MessageType = RAMessageType.Successful,
                            FaildType = RAFailedType.None,
                            Extension = string.Empty,
                            ErrorMessage = string.Empty,
                        };
                    }
                    return await RMArchiverSettingsService.RemoveRetentionSettingsAsync();
                }

                var file = Request.Form.Files["RetentionSettingsFileUp"];

                #region File Validation
                if (file.Length > 5 * 1024 * 1024)
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.UpdateFailed,
                        Extension = string.Empty,
                        ErrorMessage = "File size should not exceed 5MB."
                    };

                HashSet<string> allowedExtensions = [".csv"];
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.UpdateFailed,
                        Extension = string.Empty,
                        ErrorMessage = "Only .csv files are allowed."
                    };

                HashSet<string> allowedMimeTypes = ["text/csv"];
                if (!allowedMimeTypes.Contains(file.ContentType))
                    return new RAReturnMessage()
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.UpdateFailed,
                        Extension = string.Empty,
                        ErrorMessage = "Invalid file type."
                    };
                #endregion

                var fileStream = file.OpenReadStream();
                return await RMArchiverSettingsService.SaveRetentionSettingsAsync(fileStream, file.FileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Error in SaveRetentionSettings: ", ex);
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.UpdateFailed,
                    Extension = string.Empty,
                    ErrorMessage = I18NEntity.GetString("RM_Retention_Settings_SaveFailed"),
                };
            }
        }
    }
}
