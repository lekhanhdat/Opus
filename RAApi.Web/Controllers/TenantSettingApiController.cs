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
using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using Cloud.sdk.Data.Opus;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AvePoint.RA.Api.Web.Controllers
{
    [Route("api/TenantSettingApi/[action]")]
    [ApiController]
    public class TenantSettingApiController : RAWebApiBase
    {
        private static readonly RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(TenantSettingApiController));

        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        [HttpGet]
        public TenantGlobalSetting GetTenantSetting()
        {
            return KeyValueService.GetAllAsync().GetAwaiter().GetResult();
        }

        [HttpPost]
        public bool UpdateTenantSetting([FromBody] TenantGlobalSetting setting)
        {
            return KeyValueService.UpdateAsync(setting).GetAwaiter().GetResult();
        }

        [HttpGet]
        public bool IsNewOpus()
        {
            try
            {
                var isNewOpus = TenantService.IsNewOpusTenant();
                logger.Info($"customer Id:{TenantLocalValue.LogonGroupId} is new Opus tenant: {isNewOpus}");
                return isNewOpus;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while check tenant:{ex.ToString()}");
                return false;
            }
        }

        [HttpGet]
        public bool HasUpgradeTeams()
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Info($"customer Id:{TenantLocalValue.LogonGroupId} isn't new Opus tenant");
                    return false;
                }
                if (!KeyValueDao.HasUpgradeTeams())
                {
                    logger.Info($"customer Id:{TenantLocalValue.LogonGroupId} isn't up to teams");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"error occurred while check HasUpgradeTeams:{ex.ToString()}");
                return false;
            }
        }
    }
}
