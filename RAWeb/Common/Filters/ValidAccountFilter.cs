using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Web.Common.WIF;
using Cloud.Sdk.Data.AosModern;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class ValidNewLogicAccount : BaseAuthorizeAttribute
    {
        private readonly ITenantService _tenantService = PlatformWindsorManager.GetService<ITenantService>();
        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            return _tenantService.IsNewOpusTenant();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class ValidNotTrialAccount : BaseAuthorizeAttribute
    {
        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            return (await RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId)).Type != LicenseType.Trial;
        }
    }
}
