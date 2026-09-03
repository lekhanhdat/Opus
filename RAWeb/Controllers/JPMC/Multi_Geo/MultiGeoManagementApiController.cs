using AvePoint.RA.Common;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters.Multi_Geo;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.JPMC.Multi_Geo
{
    [ValidSupportMultiGeoFeature]
    public class MultiGEOManagementApiController : BaseApiController
    {
        private readonly IMultiGeoSettingService MultiGEOSettingService = PlatformWindsorManager.GetService<IMultiGeoSettingService>();
        [HttpGet]
        public async Task<bool> IsEnableMultiGeoFeature()
        {
            return await MultiGEOSettingService.IsEnableMultiGeoFeature();
        }

        [HttpPost]
        public async Task EnableMultiGeoFeature()
        {
            await MultiGEOSettingService.EnableMultiGeoFeature();
        }
    }
}
