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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne
{
    [Route("api/googleone/initialization")]
    public class GoogleOneInitializationApiController : GoogleOneApiBaseController
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(GoogleOneInitializationApiController));

        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly ILoginService _loginService = PlatformWindsorManager.GetService<ILoginService>();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly ILicenseHelperService _licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();

        [HttpPost("init")]
        public async Task<bool> Init()
        {
            try
            {
                _logger.Info("Start init tenant for Google One.");
                var isNewTenant = await _tenantService.InitTenantAsync();
                if (isNewTenant)
                {
                    _keyValueDao.Save(new RMKeyValue() { Key = "RunDisposalInRecords", Value = "True" });
                    await _loginService.InitSecurityProfileAsync();
                    await _generalSettingService.VerifyAndCreateDefaultSecurityProfileAsync();
                }
                await _licenseHelperService.UpdateLicense(true);
                await _tenantService.UpdateInitGControlPlatformStatus();
                _logger.Info($"End init tenant for Google One. Is new tenant: [{isNewTenant}]");
                return true;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while init tenant for Google One. Error: {e}");
                return false;
            }
        }
    }
}
