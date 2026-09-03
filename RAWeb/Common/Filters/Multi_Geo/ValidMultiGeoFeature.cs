using AvePoint.RA.Common;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters.Multi_Geo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class ValidEnableMultiGeoFeature : BaseAuthorizeAttribute
    {
        private readonly IMultiGeoSettingService MultiGEOSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            return await MultiGEOSettingService.IsEnableMultiGeoFeature();
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class ValidSupportMultiGeoFeature : BaseAuthorizeAttribute
    {
        private readonly IRMKeyValueDao RMKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        protected override async Task<bool> IsAuthorizedAsync(AuthorizationFilterContext filterContext, RMIdentity Identity)
        {
            return RMKeyValueDao.IsSupportMultipleGeoFeature();
        }
    }
}
