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
using AvePoint.GCommon;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CSD;
using AvePoint.RA.Contract.CSD.Service;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.Service.Services.ControlPanel.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.CSD
{
    [Audit]
    public class CSDCommonService : RMServiceBase, ICSDCommonService
    {
       // private AveLogger logger = AveLogger.GetInstance(typeof(CSDCommonService));
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        private IGeneralSettingService  GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService >();
        public ICSDApiKeyDao ApiKeyDao => PlatformWindsorManager.GetService<ICSDApiKeyDao>();


        public async Task<CSDApiKeyDto> GetApiKeyAsync(int id)
        {
            var dbItem = ApiKeyDao.GetApiKey(id);
            var gs = await GeneralSettingService.GetGeneralSettingAsync();
            return ConvertToApiKeyDto(gs, dbItem);
        }

        public async Task<(List<CSDApiKeyDto>, int)> GetApiKeysAsync(int pageIndex, int pageSize)
        {
            int totalCount;
            var apiKeys = ApiKeyDao.GetApiKeys(pageIndex, pageSize, out totalCount);
            var gs = await GeneralSettingService.GetGeneralSettingAsync();
            return (apiKeys.Select(a => ConvertToApiKeyDto(gs, a)).ToList(), totalCount);
        }

        public bool ExistsKeyName(int id, string name)
        {
            return ApiKeyDao.ExistsKeyName(id, name);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.CSDConfigApiKey, Action = AuditAction.CSDAddApiKey, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<bool> AddApiKeyAsync(string name, DateTime expiredTime, string operatorLoginName)
        {
            var dtExpired = await GeneralSettingService.ConvertDateTimeToUtcAsync(expiredTime);
            var keyValue = KeyGenerator.Create();
            var valuePrefix = keyValue.Substring(0, 6);
            return ApiKeyDao.AddApiKey(name, AesEncryptorWrapper.Encrypt(keyValue), valuePrefix, dtExpired.Ticks, operatorLoginName);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.CSDConfigApiKey, Action = AuditAction.CSDEditApiKey, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public async Task<bool> EditApiKeyAsync(int id, string name, DateTime expiredTime, string operatorLoginName)
        {
            var dtExpired = await GeneralSettingService.ConvertDateTimeToUtcAsync(expiredTime);
            return ApiKeyDao.EditApiKey(id, name, dtExpired.Ticks, operatorLoginName);
        }

        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.CSDConfigApiKey, Action = AuditAction.CSDDeleteApiKey, BeforeHandler = typeof(GlobalSettingBeforeAuditHandler), AfterHandler = typeof(GlobalSettingAfterAuditHandler))]
        public bool RemoveApiKeys(IEnumerable<int> ids)
        {
            return ApiKeyDao.RemoveApiKeys(ids);
        }


        private CSDApiKeyDto ConvertToApiKeyDto(GeneralSettingModel gls, CSDApiKey dbItem)
        {
            var viewPeriod = DateTime.UtcNow.AddHours(-1).Ticks;
            var expired = GeneralSettingService.ConvertTiksToDateTime(gls, dbItem.Expired, true);
            var created = GeneralSettingService.ConvertTiksToDateTime(gls, dbItem.Created, true);
            var modified = GeneralSettingService.ConvertTiksToDateTime(gls, dbItem.Modified, true);
            return new CSDApiKeyDto()
            {
                Id = dbItem.Id,
                Name = dbItem.Name,
                Value = dbItem.Created > viewPeriod ? AesEncryptorWrapper.CompatibleDecrypt(dbItem.Value) : dbItem.ValuePrefix + "****************",
                OperatorLoginName = dbItem.OperatorLoginName,
                Expired = expired.DataTime,
                Created = created.DataTime,
                Modified = modified.DataTime,
            };
        }
    }
}
