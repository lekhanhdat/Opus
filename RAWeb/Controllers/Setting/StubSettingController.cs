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
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Archiver;
using AvePoint.RA.Service.Services.StorageDevice;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Context;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StubSettingDto = AvePoint.GCommon.Contract.Server.StubSetting.StubSettingDto;

namespace AvePoint.RA.Web.Controllers.Setting
{
    [RMApiAuthorize(RMPermissionMasks.RuleManagementEnduser, RMSOPermissionMasks.RuleManagementEnduser, preferred: false)]
    [ValidateOnlyGoogleLicenseFilter]
    public class StubSettingController : BaseApiController
    {
        private IStubSettingService _StubSettingService;
        private IStubSettingService StubSettingService => PlatformWindsorManager.GetService(ref _StubSettingService);
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementAdmin, RMSOPermissionMasks.RuleManagementAdmin)]
        public async Task<RAReturnMessage> CreateOrEditStubSetting([FromBody] StubSettingUIDto sdr)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful };
            StubSettingDto dto = MiscProfileConvert.ConvertToStubSettingDto(sdr);
            SetCulture();
            dto.StubContent = ReplaceStubTags(dto.StubContent, true);
            if (ValidateStubSetting(dto) == (int)CreateOrEditStatus.Success)
            {
                if (dto.Id == null)
                {
                    result = await StubSettingService.CreateStubSettingAsync(dto);
                }
                else
                {
                    result = await StubSettingService.UpdateStubSettingAsync(dto);
                }
            }
            else
            {
                result.MessageType = RAMessageType.Failed;
            }
            return result;
        }
        [HttpPost]
        public StubSettingResult GetAllStubSettings([FromBody] StubSettingResult sdr)
        {
            if (sdr.StubSettingUIDtosList == null)
            {
                sdr.StubSettingUIDtosList = new List<StubSettingUIDto>();
            }
            return StubSettingService.GetAllStubSettings(sdr);
        }
        [HttpPost]
        public List<StubSettingUIDto> GetAllStubSettingsNotPaged()
        {
            return StubSettingService.GetAllStubSettingsNotPaged();
        }
        [HttpPost]
        public StubSettingUIDto GetStubSettingById([FromBody] string Id)
        {
            SetCulture();
            var stubSetting = StubSettingService.GetStubSettingById(Id);
            if (stubSetting != null)
            {
                stubSetting.StubContent = ReplaceStubTags(stubSetting.StubContent, false);
            }
            return stubSetting;
        }
        [HttpGet]
        public List<int> GetAllUsingObsoleteStubTypes()
        {
            return StubSettingService.GetAllUsingObsoleteStubTypes();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.RuleManagementAdmin, RMSOPermissionMasks.RuleManagementAdmin)]
        public async Task<RAReturnMessage> DeleteStubSettings([FromBody] List<string> ids)
        {
            return await StubSettingService.DeleteStubSettingAsync(ids);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage RunStubDisposalJob([FromBody] bool fromTimerJobPage)
        {
            var message = StubSettingService.RunStubDisposalJob(JobRunBy.Control, TenantLocalValue.LogonUserEmail);
            if (message == null || message.MessageType == RAMessageType.Failed)
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return new RAReturnMessage();
        }

        private void SetCulture()
        {

            string cultureName = null;

            var language = Request.Query[SPAppConstants.ParamLanguage];
            if (!string.IsNullOrEmpty(language))
            {
                cultureName = language;
            }
            else
            {
                // obtain it from HTTP header AcceptLanguages
                var languages = Request.GetTypedHeaders().AcceptLanguage;
                if (languages != null && languages.Count > 0)
                {
                    cultureName = languages.First().Value.Value;
                }
            }
            System.Globalization.CultureInfo ci = null;
            try
            {
                ci = System.Globalization.CultureInfo.CreateSpecificCulture(cultureName);
            }
            catch
            {
                ci = EnvironmentContext.GetDefaultCulture();
            }

            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
        private int ValidateStubSetting(StubSettingDto sdr)
        {
            int result = (int)CreateOrEditStatus.Success;
            switch (sdr.StubType)
            {
                case (int)LeaveStubType.Html:
                    break;
                case (int)LeaveStubType.Txt:
                    break;
                case (int)LeaveStubType.Link:
                    if (sdr.StubCustomizeTags != (int)StubCustomizeTag.None || !string.IsNullOrEmpty(sdr.StubContent))
                    {
                        result = (int)RAFailedType.ParameterIsIncorrect;
                    }
                    break;
                case (int)LeaveStubType.Aspx:
                    break;
                default:
                    result = (int)RAFailedType.ParameterIsIncorrect;
                    break;
            }
            if (sdr.StubCustomizeTags > 31) //31 is Enum StubCustomizeTag sum
            {
                result = (int)RAFailedType.ParameterIsIncorrect;
            }
            return result;
        }
        private string ReplaceStubTags(string stubContent, bool isSaveToDB)
        {
            if (string.IsNullOrEmpty(stubContent))
            {
                return string.Empty;
            }
            string stubFileName = $"[{I18NEntity.GetString("StorageOptimization.Gui_9FE3A6A6-DB1B-478A-9C84-3793B070A958")}]";
            string stubFilePath = $"[{I18NEntity.GetString("StorageOptimization.Gui_FB4CF4C0-AA67-43A7-9C37-97719E9B97A3")}]";
            string stubArchivedTime = $"[{I18NEntity.GetString("StorageOptimization.Gui_E5E06835-59BF-4AB1-903D-B0BF3EA6E15B")}]";
            string stubRuleName = $"[{I18NEntity.GetString("StorageOptimization.Gui_AE414513-8007-44BC-98B9-8E6B1212C257")}]";
            string stubRestoreLink = $"[{I18NEntity.GetString("RM_AR_CP_Stub_Panel_RestoreLink")}]";

            if (isSaveToDB)
            {
                if (stubContent.Contains(stubFileName))
                {
                    stubContent = stubContent.Replace(stubFileName, RMConstants.STUBFILENAMEMAPPING);
                }
                if (stubContent.Contains(stubFilePath))
                {
                    stubContent = stubContent.Replace(stubFilePath, RMConstants.STUBFILEPATHMAPPING);
                }
                if (stubContent.Contains(stubArchivedTime))
                {
                    stubContent = stubContent.Replace(stubArchivedTime, RMConstants.STUBARCHIVEDTIMEMAPPING);
                }
                if (stubContent.Contains(stubRuleName))
                {
                    stubContent = stubContent.Replace(stubRuleName, RMConstants.STUBRULENAMEMAPPING);
                }
                if (stubContent.Contains(stubRestoreLink))
                {
                    stubContent = stubContent.Replace(stubRestoreLink, RMConstants.STUBRESTORELINKMAPPING);
                }
            }
            else
            {
                if (stubContent.Contains(RMConstants.STUBFILENAMEMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBFILENAMEMAPPING, stubFileName);
                }
                if (stubContent.Contains(RMConstants.STUBFILEPATHMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBFILEPATHMAPPING, stubFilePath);
                }
                if (stubContent.Contains(RMConstants.STUBARCHIVEDTIMEMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBARCHIVEDTIMEMAPPING, stubArchivedTime);
                }
                if (stubContent.Contains(RMConstants.STUBRULENAMEMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBRULENAMEMAPPING, stubRuleName);
                }
                if (stubContent.Contains(RMConstants.STUBRESTORELINKMAPPING))
                {
                    stubContent = stubContent.Replace(RMConstants.STUBRESTORELINKMAPPING, stubRestoreLink);
                }
            }
            return stubContent;
        }
    }
}

