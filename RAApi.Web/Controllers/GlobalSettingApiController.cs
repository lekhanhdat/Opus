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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Global.Utils;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Api.Web.Controllers
{

    [Route("api/globalSettingApi/[action]")]
    [ApiController]
    public class GlobalSettingApiController : RAWebApiBase
    {
        private RALogger logger = RALogger.GetInstance(typeof(GlobalSettingApiController));
        private IGlobalKeyValueService GlobalKeyValueService => PlatformWindsorManager.GetService<IGlobalKeyValueService>();
        private IStorageDeviceService _StorageDeviceService;
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService(ref _StorageDeviceService);

        private const string CUSTOM_SETTING = "CUSTOM_SETTING";

        private const string DEPLOY_OTHER_DC_TENANT_INFO = "DEPLOY_OTHER_DC_TENANT_INFO";

        [HttpGet]
        public string GetGlobalSetting()
        {
            var key = $"{CUSTOM_SETTING}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.GlobalCustomSetting}";
            var keyValue = GlobalKeyValueService.Get(key);
            return keyValue == null ? "" : keyValue.Value;
        }

        [HttpPost]
        public bool UpdateGlobalSetting([FromBody] string customerSetting)
        {
            if (string.IsNullOrEmpty(customerSetting) || customerSetting?.Trim() == "[]" || customerSetting?.Trim() == "{}")
            {
                return false;
            }
            var keyValue = new RMGlobalNameValueDto
            {
                Name = CUSTOM_SETTING,
                Value = customerSetting,
                Type = RMGlobalNameValueType.GlobalCustomSetting,
            };
            return GlobalKeyValueService.Save(keyValue);
        }

        [HttpGet]
        public List<string> GetAllDeployOtherDCTenantInfoes()
        {
            var key = $"{DEPLOY_OTHER_DC_TENANT_INFO}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.DeployOtherDCTenantInfo}";
            var keyValue = GlobalKeyValueService.Get(key);
            if (keyValue == null || string.IsNullOrEmpty(keyValue.Value))
            {
                logger.Info("No DeployOtherDCTenantInfo found.");
                return new List<string>();
            }

            return SerializerHelper.DeserializeByJsonSerializer<List<SpecialTenantInfo>>(keyValue.Value).Select(a=>a.TenantId).ToList();
        }

        [HttpPost]
        public DeployOtherDCTenantResult AddDeployOtherDCTenantInfo([FromBody] string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                logger.Error("tenantId is null or empty.");
                return DeployOtherDCTenantResult.FailedDueToUnKnown;
            }

            var key = $"{DEPLOY_OTHER_DC_TENANT_INFO}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.DeployOtherDCTenantInfo}";
            var keyValue = GlobalKeyValueService.Get(key);
            //List<string> tenantIds;
            List<SpecialTenantInfo> tenantInfoes;
            if (keyValue == null || string.IsNullOrEmpty(keyValue.Value))
            {
                logger.Info("No DeployOtherDCTenantInfo found, create new one.");
                tenantInfoes = new List<SpecialTenantInfo>();
            }
            else
            {
                tenantInfoes = SerializerHelper.DeserializeByJsonSerializer<List<SpecialTenantInfo>>(keyValue.Value);
                if (tenantInfoes.Select(a=>a.TenantId).ToList().Contains(tenantId))
                {
                    logger.Error($"tenantId: {tenantId} already exists.");
                    return DeployOtherDCTenantResult.FailedDueToAlreadyExist;
                }
            }
            if (CheckHasExistAvePointStorage(tenantId))
            {
                return DeployOtherDCTenantResult.FailedDueToAvepointStorage;
            }
            tenantInfoes.Add(new SpecialTenantInfo() { 
                TenantId = tenantId,
                DataSource = RA.Contract.Configurations.TheSpecialDCKey.SOUTHAFRICA
            });
            keyValue = new RMGlobalNameValueDto
            {
                Name = DEPLOY_OTHER_DC_TENANT_INFO,
                Value = SerializerHelper.SerializeByJsonSerializer(tenantInfoes),
                Type = RMGlobalNameValueType.DeployOtherDCTenantInfo,
            };
            GlobalKeyValueService.Save(keyValue);
            logger.Info($"Add tenantId: {tenantId} to DeployOtherDCTenantInfo successfully.");
            return DeployOtherDCTenantResult.Success;
        }
        private bool CheckHasExistAvePointStorage(string tenantId)
        {
            var tempTenantId = TenantLocalValue.LogonGroupId;
            bool result = false;
            try
            {
                var defaultStorage = TenantUtil.RunUnderTenant<StorageDeviceDto>(tenantId, StorageDeviceService.GetStorageDeviceById, RecordsConstants.AVEPOINT_DEFAULT_STORAGEID);
                if (defaultStorage != null)
                {
                    logger.Warn("add specail tenant id failed,there has exist avepoint storage");
                    result = true;
                }
            }
            catch (Exception e)
            {
                logger.Error($"error occured when AddDeployOtherDCTenantInfo,{e}");
            }
            return result;
        }
        [HttpPost]
        public DeployOtherDCTenantResult RemoveDeployOtherDCTenantInfo([FromBody] string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                logger.Error("tenantId is null or empty.");
                return DeployOtherDCTenantResult.FailedDueToUnKnown;
            }

            var key = $"{DEPLOY_OTHER_DC_TENANT_INFO}{RMGlobalNameValueDto.Seprator}{RMGlobalNameValueType.DeployOtherDCTenantInfo}";
            var keyValue = GlobalKeyValueService.Get(key);
            if (keyValue == null || string.IsNullOrEmpty(keyValue.Value))
            {
                logger.Error("No DeployOtherDCTenantInfo found.");
                return DeployOtherDCTenantResult.FailedDueToUnKnown;
            }

            var tenantInfoes = SerializerHelper.DeserializeByJsonSerializer<List<SpecialTenantInfo>>(keyValue.Value);
            var deletedTenant = tenantInfoes.FirstOrDefault(a => a.TenantId == tenantId);
            if (deletedTenant == null)
            {
                logger.Error($"tenantId: {tenantId} not exists.");
                return DeployOtherDCTenantResult.FailedDueToNotExist;
            }
            if (CheckHasExistAvePointStorage(tenantId))
            {
                return DeployOtherDCTenantResult.FailedDueToAlreadyDeployed;
            }
            tenantInfoes.Remove(deletedTenant);
            keyValue = new RMGlobalNameValueDto
            {
                Name = DEPLOY_OTHER_DC_TENANT_INFO,
                Value = SerializerHelper.SerializeByJsonSerializer(tenantInfoes),
                Type = RMGlobalNameValueType.DeployOtherDCTenantInfo,
            };
            GlobalKeyValueService.Save(keyValue);
            logger.Info($"Remove tenantId: {tenantId} from DeployOtherDCTenantInfo successfully.");
            return DeployOtherDCTenantResult.Success;
        }
    }
}