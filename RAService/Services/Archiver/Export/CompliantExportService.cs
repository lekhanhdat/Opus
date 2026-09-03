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
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.Archiver.Export;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using AvePoint.RA.Service.Services.StorageDevice;
using Microsoft.Graph.Models;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Archiver.Export
{
    [Audit]
    public class CompliantExportService : ICompliantExportService
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(CompliantExportService));

        private IStorageDeviceService _StorageDeviceService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService(ref _StorageDeviceService);

        private IKeyValueService _keyValueService;
        private IKeyValueService IKeyValueService => PlatformWindsorManager.GetService(ref _keyValueService);

        private ILicenseHelperService _licenseHelperService;
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService(ref _licenseHelperService);


        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.CompliantExport, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveExportSetting(ExportSettingsInfo exportInfo)
        {
            RAReturnMessage result = new RAReturnMessage() { MessageType = RAMessageType.Successful, ErrorMessage = string.Empty };
            try
            {
                if(!string.IsNullOrWhiteSpace(exportInfo.DefaultStorageDeviceId))
                {
                    var storageDto = StorageDeviceService.GetStorageDeviceById(exportInfo.DefaultStorageDeviceId);
                    if (storageDto == null)
                    {
                        throw new Exception("device id is not exist");
                    }
                    await StorageDeviceService.SetUsingDeviceByIdAsync(storageDto.Id, SettingProfilesType.ExportLocationDevice,isCompliantExport:true);
                }
                if (exportInfo.VEOExportInfos != null && exportInfo.VEOExportInfos.Any())
                {
                    if (VEOV3CommonMethod.HasUpgradedVEOV3())
                    {
                        CompliantExportFactory.GetCompliantExporter(ExportTypeValue.VEO).SaveAndUploadExportInfos(exportInfo.VEOExportInfos);
                    }
                    else
                    {
                        _logger.Warn($"not upgraded to veo3 yet");
                    }                        
                }
                if (exportInfo.NARAExportInfos != null && exportInfo.NARAExportInfos.Any())
                {
                    CompliantExportFactory.GetCompliantExporter(ExportTypeValue.NARA).SaveAndUploadExportInfos(exportInfo.NARAExportInfos);
                }
                if (exportInfo.NAAExportInfos != null && exportInfo.NAAExportInfos.Any())
                {
                    CompliantExportFactory.GetCompliantExporter(ExportTypeValue.NAA).SaveAndUploadExportInfos(exportInfo.NAAExportInfos);
                }
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                result.MessageType = RAMessageType.Failed;
            }
            return result;
        }

        public async Task<List<BaseExportInfo>> LoadExportSetting(ExportTypeValue type)
        {
            return CompliantExportFactory.GetCompliantExporter(type).LoadExportInfos();
        }

        public async Task<List<BaseExportInfo>> LoadAllExportSettings()
        {
            List<BaseExportInfo> allExportInfos = new List<BaseExportInfo>();
            var hasOpusILOrSOLicense = LicenseHelperService.HasOpusSPILOrSOLicense;
            var hasGoogleLicense = LicenseHelperService.HasOpusGoogleLicense;
            if (hasOpusILOrSOLicense || hasGoogleLicense)
            {
                allExportInfos.AddRange(CompliantExportFactory.GetCompliantExporter(ExportTypeValue.NARA).LoadExportInfos());
            }
            if(hasOpusILOrSOLicense)
            {
                allExportInfos.AddRange(CompliantExportFactory.GetCompliantExporter(ExportTypeValue.NAA).LoadExportInfos());
            }
            if (hasOpusILOrSOLicense && VEOV3CommonMethod.HasUpgradedVEOV3())
            {
                allExportInfos.AddRange(CompliantExportFactory.GetCompliantExporter(ExportTypeValue.VEO).LoadExportInfos());
            }            
            return allExportInfos;
        }
    }
}
