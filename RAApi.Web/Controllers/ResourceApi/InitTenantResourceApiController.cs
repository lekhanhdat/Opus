using AvePoint.RA.Api.Web.Common;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Multi_Geo.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.AccountManager;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.ResourceApi
{
    [Route("api/initialization/[action]")]
    [ApiController]
    public class InitTenantResourceApiController : RAWebApiBase
    {
        private readonly IRALogger _logger = new RALogger(typeof(RuleResourceApiController));

        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();

        private readonly IRMKeyValueDao _keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly ILoginService _loginService = PlatformWindsorManager.GetService<ILoginService>();

        private readonly IGeneralSettingService _generalSettingService = PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly ILicenseHelperService _licenseHelperService = PlatformWindsorManager.GetService<ILicenseHelperService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        [HttpPost]
        public async Task<bool> InitTenant([FromBody] InitMultiGeoTenantInfo tenantInfo)
        {
            try
            {
                _logger.Info($"Start init multi geo tenant....");
                TenantLocalValue.LogonUserEmail = tenantInfo.RegisterEmail;
                var isNewTenant = await _tenantService.InitMultiGeoTenantAsync(RA.Contract.Aos.Notification.MultiGeoStatus.MultiGeoDC);
                if (isNewTenant)
                {
                    await _tenantService.InitKeyForMultiGeoTenant(tenantInfo);
                    await _loginService.InitSecurityProfileAsync();
                    await _generalSettingService.VerifyAndCreateDefaultSecurityProfileAsync();
                }
                else
                {
                    await UserService.SyncLogonUserGroupAsync(TenantLocalValue.LogonUserId);
                }
                await _licenseHelperService.UpdateLicense(true);
                _logger.Info($"End init multi geo tenant..... Is new tenant: [{isNewTenant}]");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while init multi geo tenant. Error: {e}");
                return false;
            }
        }

        [HttpPost]
        public int IsTenantInitialized()
        {
            return (int)_tenantService.IsMultiGeoTenantInitialized();
        }
    }
}
