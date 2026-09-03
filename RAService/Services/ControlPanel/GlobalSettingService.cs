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
using AvePoint.RA.CommonUtil;
using AvePoint.GCommon.Contract.Server.ControlPanel.AuthenticationManager;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using System;
using System.Collections.Generic;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Server.Common.ExportReport.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.DB.Explorer;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Common;
using System.Threading.Tasks;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.Settings.AuditHandler;

namespace AvePoint.RA.Service.ControlPanel
{
    [Audit]
    public class GlobalSettingService : RMServiceBase, IGlobalSettingService
    {
        public IGlobalStorageSettingDao GlobalStorageSettingDao => PlatformWindsorManager.GetService<IGlobalStorageSettingDao>();
        public IExportSettingsDao ExportSettingsDao => PlatformWindsorManager.GetService<IExportSettingsDao>();
       
        private RALogger logger = RALogger.GetInstance(typeof(GlobalSettingService));
        //public SORulesAndSettings LoadMetaData()
        //{
        //    logger.Info("start to load global setting meta data.");
        //    var client = new DAOAPIClientV1();
        //    SORulesAndSettings settings = new SORulesAndSettings();
        //    settings.StoragePolicies = client.GetAllStoragePolicy();
        //    settings.DataEncryptionProfiles = client.GetAllSecurityProfile();
        //    return settings;

        //}
        public Task<List<ExportReportDto>> GetAllExportLocationAsync()
        {
            logger.Info("start to load global setting export location.");
            var client = new DAOAPIClientV1();
            return client.GetAllExportLocationAsync();
        }

        public string GetCurrentExportLocationId()
        {
            var gssInfosTemp = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            return gssInfosTemp.ExportLocationId.ToString();
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.ExportSettings, Action = AuditAction.ConfigureExportSetting, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public System.Threading.Tasks.Task SaveExportLocationInfoAsync(string ExportLocationId)
        {
            var gssInfosTemp = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
            gssInfosTemp.ExportLocationId = Guid.Parse(ExportLocationId);
            return GlobalStorageSettingDao.SaveOrUpdateAsync(gssInfosTemp);
        }

        public ValidationMessage CheckDocAveConnectionSetting()
        {
            var result = new ValidationMessage();
            try
            {
                var daoApi = new DAOAPIClientV1();
                result.Success = true;
            }
            catch (Exception ex)
            {
                logger.Warn("dao connect failed,error:{0}", ex.ToString());
                result.Message = string.Format(I18NEntity.GetString("RM_JS_Common_FromDocaveMsg"), ex.Message);
                result.Success = false;
                result.FailedType = ResultFailedType.NotConnDocAve;
            }
            return result;
        }

        //public ValidationMessage CheckGlobalStorageSetting()
        //{
        //    var result = new ValidationMessage();
        //    var rmSettings = GlobalStorageSettingDao.GetGlobalSettingInfoFromRA();
        //    result.Success = rmSettings != null;
        //    result.FailedType = !result.Success ? ResultFailedType.NoGlobalStorageSetting : ResultFailedType.None;
        //    return result;
        //}

        public ValidationMessage CheckExportSetting(ValidationType Type, int sourceFlag)
        {
            var result = new ValidationMessage();
            RMCPExportSetting exportSetting = null;
            if (Type == ValidationType.ExportSetting)
            {
                exportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.VEO, sourceFlag);
            }
            if (Type == ValidationType.NNAExportSetting)
            {
                exportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NAA, sourceFlag);
            }
            if (Type == ValidationType.NARAExportSetting)
            {
                exportSetting = ExportSettingsDao.GetExportSetting((int)ExportSettingType.NARA, sourceFlag);
            }
            result.Success = exportSetting != null;
            return result;
        }

        public Task<Dictionary<Guid, int>> GetExportLocationTypesAsync()
        {
            logger.Info("start to load global setting export location types.");
            var client = new DAOAPIClientV1();
            return client.GetExportLocationTypesAsync();
        }
    }
}
